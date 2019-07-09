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
    [Serializable]
    public class NetworkStreamSource : IStreamSource
    {
        const int kMaxConnections = 1;
        [SerializeField]
        int m_Port = 9000;
        List<Socket> m_ServerSockets = new List<Socket>();
        List<Thread> m_Threads = new List<Thread>();
        Stream m_Stream;

        public Stream stream
        {
            get { return m_Stream; }
        }

        public void ConnectToServer(string serverIP)
        {
            Dispose();

            IPAddress ip;
            if (!IPAddress.TryParse(serverIP, out ip))
                return;

            var endPoint = new IPEndPoint(ip, m_Port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var thread = new Thread(() =>
            {
                try
                {
                    socket.Connect(endPoint);

                    if (m_Stream == null)
                    {
                        m_Stream = CreateBufferedNetworkStream(socket);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Exception trying to connect: " + e.Message);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void StartServer()
        {
            var addresses = GetIPAddresses();

            foreach (var address in addresses)
            {
                var serverSocket = CreateServerSocket(address, m_Port);

                if (serverSocket != null)
                    m_ServerSockets.Add(serverSocket);
                
                var serverThread = CreateServerThread(serverSocket);
                m_Threads.Add(serverThread);
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
            foreach (var thread in m_Threads)
            {
                if (thread.ThreadState == ThreadState.Running)
                    thread.Abort();
            }

            foreach (var socket in m_ServerSockets)
            {
                if (socket.Connected)
                    socket.Close();
            }
            
            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }

            m_Threads.Clear();
            m_ServerSockets.Clear();
        }

        IPAddress[] GetIPAddresses()
        {
            IPAddress[] addresses;
            try
            {
                addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            }
            catch (Exception)
            {
                Debug.LogWarning("DNS-based method failed, using network interfaces to find local IP");
                var addressList = new List<IPAddress>();
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

                                if (IPAddress.IsLoopback(address))
                                    continue;
                                
                                addressList.Add(address);
                            }

                            break;
                    }
                }

                addresses = addressList.ToArray();
            }
            return addresses;
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

                Debug.Log("Listening: " + address);
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
                        var socket = listenSocket.Accept();

                        if (m_Stream == null)
                        {
                            m_Stream = CreateBufferedNetworkStream(socket);
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
