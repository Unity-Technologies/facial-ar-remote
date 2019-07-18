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

    public class AdapterStream : MemoryStream
    {
        byte[] m_Buffer = new byte[1024];
        int m_RemainingBytes = 0;
        public Stream input { get; set; }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0 || input == null)
                return 0;

            var remainingBytes = m_RemainingBytes;

            if (remainingBytes == 0)
            {
                var size = Marshal.SizeOf<StreamBufferData>();
                var readBuffer = GetBuffer(size);
                var readBytes = input.Read(readBuffer, 0, size);

                if (readBytes == 0) 
                    return 0;
                
                var oldStruct = readBuffer.ToStruct<StreamBufferData>();

                var desc = new PacketDescriptor()
                {
                    type = PacketType.Face,
                    version = 0
                };
                var data = new FaceData()
                {
                    id = 0,
                    timeStamp = oldStruct.FrameTime,
                    blendShapeValues = oldStruct.BlendshapeValues,
                };

                m_RemainingBytes = Marshal.SizeOf<PacketDescriptor>();
                m_RemainingBytes += Marshal.SizeOf<FaceData>();

                Position = 0;
                Write(desc.ToBytes(), 0, Marshal.SizeOf<PacketDescriptor>());
                Write(data.ToBytes(), 0, Marshal.SizeOf<FaceData>());
                Flush();
                Position = 0;
            }
            else if (m_RemainingBytes < count)
            {
                m_RemainingBytes = 0;
                return base.Read(buffer, offset, remainingBytes);
            }
            
            m_RemainingBytes -= count;

            return base.Read(buffer, offset, count);
        }
    }

    public class AdapterSource : IStreamSource
    {
        AdapterStream m_AdapterStream = new AdapterStream();
        public IStreamSource streamSource { get; set; }

        public Stream stream
        {
            get
            {
                if (streamSource == null)
                    m_AdapterStream.input = null;
                else
                    m_AdapterStream.input = streamSource.stream;
                
                return m_AdapterStream;
            }
        }
    }

    public class Server
    {
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        Thread m_Thread;
        StreamReader m_StreamReader = new StreamReader();
        AdapterSource m_Adapter = new AdapterSource();

        public void Start()
        {
            m_Adapter.streamSource = m_NetworkStreamSource;
            m_StreamReader.streamSource = m_Adapter;
            m_StreamReader.faceDataOutput = new FaceDataDebugger();

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
            m_NetworkStreamSource.stream.Write(bytes, 0 , count);
            m_NetworkStreamSource.stream.Flush();
        }

        public void Write<T>(T packet) where T : struct, IPackageable
        {
            int size = Marshal.SizeOf<T>();
            var descriptor = packet.descriptor;
            
            packet.timeStamp = Time.realtimeSinceStartup;

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

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                faceData.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_Client.Write(faceData);
            */

            var data = new StreamBufferData();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferData>());
        }
    }
}
