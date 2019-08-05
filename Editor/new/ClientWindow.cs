using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class ClientWindow : EditorWindow
    {
        RemoteStream m_Server = new RemoteStream();
        RemoteStream m_Client = new RemoteStream();
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
            m_Server.reader.faceDataChanged += FaceDataChanged;

            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            m_Server.reader.faceDataChanged -= FaceDataChanged;

            m_Server.Disconnect();

            EditorApplication.update -= Update;
        }

        void OnGUI()
        {
            ServerGUI();
            ClientGUI();
        }

        void Update()
        {
            m_Server.reader.Receive();
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

                    m_Server.isServer = true;
                    m_Server.Connect();
                }
                if (GUILayout.Button("Stop"))
                    m_Server.Disconnect();
            }
            //m_Server.adapterVersion = (AdapterVersion)EditorGUILayout.EnumPopup("Adapter Version", m_Server.adapterVersion);
        }

        void ClientGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Connect"))
                {
                    m_Client.ip = "127.0.0.1";
                    m_Client.port = 9000;
                    m_Client.isServer = false;
                    m_Client.Connect();
                }
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
            
            m_Client.Write(faceData);
            */
            
            var data = new StreamBufferDataV1();

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }
    }
}
