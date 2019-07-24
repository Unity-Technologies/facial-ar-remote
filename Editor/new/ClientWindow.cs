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
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        StreamWriter m_StreamWriter = new StreamWriter();

        public StreamWriter streamWriter
        {
            get { return m_StreamWriter; }
        }

        public Client()
        {
            m_StreamWriter.streamSource = m_NetworkStreamSource;
        }

        public void ConnectToServer(string ip, int port)
        {
            m_NetworkStreamSource.ConnectToServer(ip, port);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.StopConnections();
        }
    }

    public class ClientWindow : EditorWindow
    {
        Server m_Server = new Server();
        Client m_Client = new Client();
        BlendShapesController m_Controller;

        [MenuItem("Window/Test Client")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(ClientWindow));
            window.titleContent = new GUIContent("Test Client");;
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            m_Server.streamReader.faceDataChanged += FaceDataChanged;

            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            m_Server.streamReader.faceDataChanged -= FaceDataChanged;

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
            m_Server.streamReader.Dequeue();
        }

        void FaceDataChanged(FaceData faceData)
        {
            if (m_Controller != null)
            {
                m_Controller.blendShapeInput = faceData.blendShapeValues;
                m_Controller.UpdateBlendShapes();
            }
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
            
            m_Client.streamWriter.Write(faceData);
            */
            
            var data = new StreamBufferDataV1();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.streamWriter.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
            
            m_Client.streamWriter.Send();
        }
    }
}
