using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace PerformanceRecorder
{
    public class NetworkStreamSource : IStreamSource
    {
        const int kMaxConnections = 1;
        List<Socket> m_ServerSockets = new List<Socket>();
        List<Thread> m_Threads = new List<Thread>();
        Stream m_Stream;

        public Stream stream
        {
            get { return m_Stream; }
        }

        public void ConnectToServer(string serverIP, int port)
        {
            Dispose();

            IPAddress ip;
            if (!IPAddress.TryParse(serverIP, out ip))
                return;

            var endPoint = new IPEndPoint(ip, port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var thread = new Thread(() =>
            {
                try
                {
                    Debug.Log("Client: Connecting to " + endPoint.Address.ToString());
                    socket.Connect(endPoint);
                    Debug.Log("Client: Connected");

                    if (m_Stream == null)
                    {
                        m_Stream = CreateBufferedNetworkStream(socket);
                        Debug.Log("Client: Stream Created");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Client: " + e.Message);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void StartServer(int port)
        {
            var addresses = GetIPAddresses();

            foreach (var address in addresses)
            {
                var serverSocket = CreateServerSocket(address, port);

                if (serverSocket != null)
                {
                    m_ServerSockets.Add(serverSocket);
                    m_Threads.Add(CreateServerThread(serverSocket));
                }
            }

            foreach (var thread in m_Threads)
                thread.Start();
        }

        public void StopConnections()
        {
            Dispose();
        }

        void Dispose()
        {
            DisposeStream();

            foreach (var socket in m_ServerSockets)
            {
                if (socket.Connected)
                    socket.Close();
            }

            foreach (var thread in m_Threads)
            {
                thread.Abort();
            }

            m_Threads.Clear();
            m_ServerSockets.Clear();
        }

        void DisposeStream()
        {
            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }
        }

        IPAddress[] GetIPAddresses()
        {
            try
            {
                var addresses = new List<IPAddress>();
                
                addresses.AddRange(Dns.GetHostEntry("localhost").AddressList);
                addresses.AddRange(Dns.GetHostEntry(Dns.GetHostName()).AddressList);

                return addresses.ToArray();
            }
            catch (Exception)
            {
                Debug.LogWarning("DNS-based method failed, using network interfaces to find local IP");

                var addresses = new List<IPAddress>();

                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    switch (networkInterface.OperationalStatus)
                    {
                        case OperationalStatus.Up:
                        case OperationalStatus.Unknown:
                            foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                            {
                                var address = ip.Address;

                                if (address.AddressFamily != AddressFamily.InterNetwork)
                                    continue;
                                
                                addresses.Add(address);
                            }

                            break;
                    }
                }

                return addresses.ToArray();
            }
        }

        Socket CreateServerSocket(IPAddress address, int port)
        {
            var socket = default(Socket);
            try
            {
                var endPoint = new IPEndPoint(address, port);
                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(kMaxConnections);
            }
            catch (Exception) {}

            return socket;
        }

        Thread CreateServerThread(Socket listenSocket)
        {
            Debug.Assert(listenSocket != null);

            var thread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var endPoint = listenSocket.LocalEndPoint as IPEndPoint;
                        Debug.Log("Server: Listening " + endPoint.Address);
                        var socket = listenSocket.Accept();
                        Debug.Log("Server: Accepted");

                        DisposeStream();

                        if (m_Stream == null)
                        {
                            m_Stream = CreateBufferedNetworkStream(socket);
                            Debug.Log("Server: Stream Created");
                        }
                    }
                }
                catch (Exception) {}
            });

            return thread;
        }

        Stream CreateBufferedNetworkStream(Socket socket)
        {
            return new BufferedStream(new NetworkStream(socket, true));
        }
    }
}
