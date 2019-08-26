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
        [SerializeField]
        Transform m_TrackedObject;
        Pose m_CurrentPose = new Pose(Vector3.positiveInfinity, Quaternion.identity);
        bool m_WasConnected = false;
        float m_StartTime = 0f;

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

            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            m_Client.Disconnect();

            EditorApplication.update -= Update;
        }

        void Update()
        {
            if (m_Client.isConnected)
            {
                m_WasConnected = true;

                SendPoseData();
            }
            else if (m_WasConnected)
            {
                m_WasConnected = false;

                m_Client.Disconnect();
            }
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
                {
                    m_StartTime = Time.realtimeSinceStartup;
                    m_Client.SendStartRecording();
                } 
                if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonWide))
                    m_Client.SendStopRecording();
            }

            m_TrackedObject = EditorGUILayout.ObjectField(m_TrackedObject, typeof(Transform), true) as Transform;
        }

        float GetTimeStamp()
        {
            return Time.realtimeSinceStartup - m_StartTime;
        }

        void SendPoseData()
        {
            if (m_TrackedObject == null)
                return;
            
            var pose = new Pose(m_TrackedObject.localPosition, m_TrackedObject.localRotation);

            if (pose != m_CurrentPose)
            {
                m_CurrentPose = pose;

                var data = new PoseData()
                {
                    timeStamp = GetTimeStamp(),
                    pose = pose
                };

                m_Client.Send(data);
            }
        }

        void SendFaceData()
        {
            var data = new FaceData();
            data.timeStamp = GetTimeStamp();

            for (var i = 0; i < BlendShapeValues.count; ++i)
                data.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_Client.Send(data);
        }

        void SendFaceDataV1()
        {
            var data = new StreamBufferDataV1();
            data.FrameTime = GetTimeStamp();

            for (var i = 0; i < BlendShapeValues.count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.Send(data);
        }
    }
}
