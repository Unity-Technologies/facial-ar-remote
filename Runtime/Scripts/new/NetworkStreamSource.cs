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
        Socket m_StreamSocket;

        public Stream stream
        {
            get
            {
                if (isConnected)
                    return m_Stream;

                return null;
            }
        }

        public bool isListening { get; private set; }
        public bool isConnecting { get; private set; }
        public bool isConnected
        {
            get { return m_StreamSocket != null && m_StreamSocket.Connected; }
        }

        public void ConnectToServer(string serverIP, int port)
        {
            StopServer();

            IPAddress ip;
            if (!IPAddress.TryParse(serverIP, out ip))
                return;

            var endPoint = new IPEndPoint(ip, port);
            m_StreamSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var thread = new Thread(() =>
            {
                try
                {
                    isConnecting = true;
                    Debug.Log("Client: Connecting to " + endPoint.Address.ToString());
                    m_StreamSocket.Connect(endPoint);
                    Debug.Log("Client: Connected");

                    if (m_Stream == null)
                    {
                        m_Stream = CreateBufferedNetworkStream(m_StreamSocket);
                        Debug.Log("Client: Stream Created");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Client: " + e.Message);
                }
                finally
                {
                    isConnecting = false;
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void StartServer(int port)
        {
            isListening = true;
            
            var addresses = NetworkUtilities.GetIPAddresses();

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

        public void DisconnectClient()
        {
            DisposeStream();
            DisposeSocket(m_StreamSocket);
            m_StreamSocket = null;
        }

        public void StopServer()
        {
            isListening = false;

            DisconnectClient();

            foreach (var socket in m_ServerSockets)
                DisposeSocket(socket);

            foreach (var thread in m_Threads)
            {
                thread.Join();
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

        void DisposeSocket(Socket socket)
        {
            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close(1);
                }
                socket.Dispose();
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
                var endPoint = listenSocket.LocalEndPoint as IPEndPoint;

                if (endPoint == null)
                    return;
                        
                while (isListening)
                {
                    try
                    {
                        Debug.Log("Server: Listening " + endPoint.Address);
                        var socket = listenSocket.Accept();
                        Debug.Log("Server: Accepted " + endPoint.Address);

                        DisposeStream();

                        if (m_Stream == null)
                        {
                            m_Stream = CreateBufferedNetworkStream(socket);
                            Debug.Log("Server: Stream Created");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Server: " + e.Message);
                    }
                }
            });

            return thread;
        }

        Stream CreateBufferedNetworkStream(Socket socket)
        {
            m_StreamSocket = socket;
            return new BufferedStream(new NetworkStream(socket, true));
        }
    }
}
