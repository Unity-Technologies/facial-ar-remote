using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class FaceDataDebugger : IData<FaceData>
    {
        FaceData m_Data = new FaceData();
        System.Text.StringBuilder m_StringBuilder = new System.Text.StringBuilder();
        public BlendShapesController controller { get; set; }

        public FaceData data
        {
            get { return m_Data; }
            set
            {
                m_Data = value;

                if (controller != null)
                {
                    controller.blendShapeInput = value.blendShapeValues;
                    controller.UpdateBlendShapes();
                }
                //DebugLog();
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
        AdapterSource m_Adapter = new AdapterSource();

        public AdapterVersion adapterVersion
        {
            get { return m_Adapter.version; }
            set { m_Adapter.version = value; }
        }

        public StreamReader streamReader
        {
            get { return m_StreamReader; }
        }

        public void Start()
        {
            m_Adapter.streamSource = m_NetworkStreamSource;
            m_StreamReader.streamSource = m_Adapter;

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
            m_NetworkStreamSource.StopConnections();
            DisposeThread();
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
        static readonly int PacketDescriptorSize = Marshal.SizeOf<PacketDescriptor>();

        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        public void ConnectToServer(string ip, int port)
        {
            m_NetworkStreamSource.ConnectToServer(ip, port);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.StopConnections();
        }

        public void Write(byte[] bytes, int count)
        {
            var stream = m_Manager.GetStream();
            stream.Write(bytes, 0, count);
            m_Queue.Enqueue(stream);
        }

        public void Write<T>(PacketDescriptor descriptor, T packet) where T : struct
        {
            var stream = m_Manager.GetStream();
            int size = Marshal.SizeOf<T>();

            stream.Write(descriptor.ToBytes(), 0, PacketDescriptorSize);
            stream.Write(packet.ToBytes(), 0, size);

            m_Queue.Enqueue(stream);
        }

        public void Send()
        {
            var outputStream = m_NetworkStreamSource.stream;
            var stream = default(MemoryStream);

            while (m_Queue.TryDequeue(out stream))
            {
                var count = (int)stream.Position;
                outputStream.Write(stream.GetBuffer(), 0 , count);
                stream.Dispose();
            }

            outputStream.Flush();
        }
    }

    public class ClientWindow : EditorWindow
    {
        Server m_Server = new Server();
        Client m_Client = new Client();
        BlendShapesController m_Controller;
        FaceDataDebugger m_FaceDataDebugger = new FaceDataDebugger();

        [MenuItem("Window/Test Client")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(ClientWindow));
            window.titleContent = new GUIContent("Test Client");;
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            m_Server.Stop();
            m_Client.Disconnect();

            EditorApplication.update -= Update;
        }

        void OnGUI()
        {
            ServerGUI();
            ClientGUI();
        }

        void Update()
        {
            if (m_Controller == null)
                return;
            
            m_FaceDataDebugger.controller = m_Controller;

            m_Server.streamReader.faceDataOutput = m_FaceDataDebugger;
            m_Server.streamReader.Dequeue();
        }

        void ServerGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start"))
                {
                    m_Controller = GameObject.FindObjectOfType<BlendShapesController>();

                    m_Server.Start();
                }
                if (GUILayout.Button("Stop"))
                    m_Server.Stop();
            }
            m_Server.adapterVersion = (AdapterVersion)EditorGUILayout.EnumPopup("Adapter Version", m_Server.adapterVersion);
        }

        void ClientGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Connect"))
                    m_Client.ConnectToServer("127.0.0.1", 9000);
                if (GUILayout.Button("Disconnect"))
                    m_Client.Disconnect();
                if (GUILayout.Button("Send"))
                    SendPacket();
            }
        }

        void SendPacket()
        {
            /*
            var faceData = new FaceData();
            faceData.timeStamp = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                faceData.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_Client.Write(PacketDescriptor.Get(PacketType.Face), faceData);
            */

            var data = new StreamBufferDataV1();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
            
            m_Client.Send();
        }
    }
}
