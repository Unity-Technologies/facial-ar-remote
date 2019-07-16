using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class FaceDataDebugger : IData<FaceData>
    {
        FaceData m_Data = new FaceData();
        System.Text.StringBuilder m_StringBuilder = new System.Text.StringBuilder();

        public FaceData data
        {
            get { return m_Data; }
            set
            {
                m_Data = value;
                DebugLog();
            }
        }

        void DebugLog()
        {
            m_StringBuilder.Clear();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
            {
                m_StringBuilder.Append(m_Data.blendShapeValues[i]);

                if (i < BlendShapeValues.Count-1)
                    m_StringBuilder.Append(", ");
            }

            Debug.Log(m_StringBuilder.ToString());
        }
    }

    public class Server
    {
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        Thread m_Thread;
        StreamReader m_StreamReader = new StreamReader();

        public void Start()
        {
            m_StreamReader.streamSource = m_NetworkStreamSource;
            m_StreamReader.faceOutput = new FaceDataDebugger();

            m_NetworkStreamSource.StartServer(9000);

            m_Thread = new Thread(() =>
            {
                while (true)
                {
                    m_StreamReader.Read();
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

        public void Write(byte[] bytes)
        {
            m_NetworkStreamSource.stream.Write(bytes, 0 , bytes.Length);
            m_NetworkStreamSource.stream.Flush();
        }

        public void Write<T>(T packet) where T : struct, IPackageable
        {
            int size = Marshal.SizeOf<T>();
            var descriptor = packet.descriptor;
            
            descriptor.time = Time.realtimeSinceStartup;

            m_NetworkStreamSource.stream.Write(descriptor.ToBytes(), 0 , PacketDescriptor.Size);
            m_NetworkStreamSource.stream.Write(packet.ToBytes(), 0 , size);
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
            var faceData = new FaceData();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                faceData.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_Client.Write(faceData);
        }
    }
}
