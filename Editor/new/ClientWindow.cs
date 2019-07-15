using System;
using System.IO;
using System.Threading;
using Microsoft.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PerformanceRecorder
{
    public class Server
    {
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        Thread m_Thread;
        byte[] m_Buffer = new byte[1024];

        public void Start()
        {
            m_NetworkStreamSource.StartServer(9000);

            m_Thread = new Thread(() =>
            {
                while (true)
                {
                    if (m_NetworkStreamSource.stream != null)
                    {
                        ReadPacket(m_NetworkStreamSource.stream);
                    }

                    Thread.Sleep(1);
                };
            });
            m_Thread.Start();
        }

        public void Stop()
        {
            DisposeThread();
            m_NetworkStreamSource.StopConnections();
        }

        void DisposeThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        void ReadPacket(Stream stream)
        {
            var bytes = new byte[PacketDescriptor.Size];
            var readByteCount = stream.Read(bytes, 0, bytes.Length);

            if (readByteCount > 0)
            {
                var packet = bytes.ToStruct<PacketDescriptor>();
                Debug.Log(packet.type + " " + packet.version);
            }
        }
    }

    public class Client
    {
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();

        public void ConnectToServer(string ip)
        {
            m_NetworkStreamSource.ConnectToServer(ip, 9000);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.StopConnections();
        }

        public void Send(byte[] bytes)
        {
            m_NetworkStreamSource.stream.Write(bytes, 0 , bytes.Length);
            m_NetworkStreamSource.stream.Flush();
        }
    }

    public class ClientWindow : EditorWindow
    {
        Server m_Server = new Server();
        Client m_Client = new Client();

        [MenuItem("Window/Test Client")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(ClientWindow));
            window.titleContent = new GUIContent("Test Client");;
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            
        }

        void OnDisable()
        {
            m_Server.Stop();
            m_Client.Disconnect();
        }

        void OnGUI()
        {
            ServerGUI();
            ClientGUI();
        }

        void ServerGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start"))
                    m_Server.Start();
                if (GUILayout.Button("Stop"))
                    m_Server.Stop();
            }
        }

        void ClientGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Connect"))
                    m_Client.ConnectToServer("127.0.0.1");
                if (GUILayout.Button("Disconnect"))
                    m_Client.Disconnect();
                if (GUILayout.Button("Send"))
                    SendPacket();
            }
        }

        void SendPacket()
        {
            var packet = new PacketDescriptor()
            {
                type = PacketType.FaceRig,
                version = UnityEngine.Random.Range(0, 256)
            };
            
            m_Client.Send(packet.ToBytes());
        }
    }
}
