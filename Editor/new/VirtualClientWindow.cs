using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class VirtualClientWindow : EditorWindow
    {
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        [SerializeField]
        Vector2 m_Scroll;
        [SerializeField]
        Client m_Client = new Client();

        [MenuItem("Window/Virtual Client")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(VirtualClientWindow));
            window.titleContent = new GUIContent("Virtual Client");;
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            m_Client.ip = "127.0.0.1";
            m_Client.port = 9000;
            m_Client.isServer = false;
        }

        void OnDisable()
        {
            m_Client.Disconnect();
        }

        void OnGUI()
        {
            using (var scrollview = new EditorGUILayout.ScrollViewScope(m_Scroll))
            {
                m_Scroll = scrollview.scrollPosition;

                using (new GUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("Client", EditorStyles.boldLabel);
                    ClientGUI();
                }
            }
        }

        void ClientGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Connect", EditorStyles.miniButton, kButtonWide))
                {
                    m_Client.Connect();
                }

                if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                {
                    m_Client.Disconnect();
                }

                if (GUILayout.Button("Send", EditorStyles.miniButton, kButtonMid))
                    SendFaceData();
                if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    m_Client.SendStartRecording();
                if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonWide))
                    m_Client.SendStopRecording();
            }
        }

        void SendFaceData()
        {
            var data = new FaceData();
            data.timeStamp = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_Client.SendFaceData(data);
        }

        void SendFaceDataV1()
        {
            var data = new StreamBufferDataV1();
            data.FrameTime = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.SendFaceData(data);
        }
    }
}
