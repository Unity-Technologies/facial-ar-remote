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
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        PacketStream m_PacketStream = new PacketStream();

        [MenuItem("Window/Virtual Client")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(VirtualClientWindow));
            window.titleContent = new GUIContent("Virtual Client");;
            window.minSize = new Vector2(300, 100);
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
                    m_NetworkStreamSource.ConnectToServer("127.0.0.1", 9000);
                    m_PacketStream.streamSource = m_NetworkStreamSource;
                    m_PacketStream.Start();
                }

                if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                {
                    m_PacketStream.Stop();
                    m_NetworkStreamSource.StopConnections();
                }

                if (GUILayout.Button("Send", EditorStyles.miniButton, kButtonMid))
                    SendFaceData();
                if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    SendStartRecording();
            }
        }

        void SendFaceData()
        {
            var faceData = new FaceData();
            faceData.timeStamp = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                faceData.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_PacketStream.writer.Write(faceData);

            /*
            var data = new StreamBufferDataV1();
            data.FrameTime = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
            */
        }

        void SendStartRecording()
        {
            m_PacketStream.writer.Write(new Command(CommandType.StartRecording));
        }
    }
}
