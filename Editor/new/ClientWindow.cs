using System;
using System.IO;
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
        FaceDataRecorder m_Recoder = new FaceDataRecorder();
        TakePlayer m_Player = new TakePlayer();
        AnimationClip m_Clip;
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

        void FindController()
        {
            if (m_Controller == null)
                m_Controller = GameObject.FindObjectOfType<BlendShapesController>();
        }

        void OnGUI()
        {
            FindController();
            ServerGUI();
            ClientGUI();
        }

        void Update()
        {
            m_Server.reader.Receive();
            m_Player.Update();
        }

        void FaceDataChanged(FaceData data)
        {
            if (m_Controller != null)
            {
                m_Controller.blendShapeInput = data.blendShapeValues;
                m_Controller.UpdateBlendShapes();
            }

            if (m_Recoder.isRecording)
                m_Recoder.Record(data);
        }

        void ServerGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start"))
                {
                    m_Server.isServer = true;
                    m_Server.Connect();
                }
                if (GUILayout.Button("Stop"))
                    m_Server.Disconnect();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(m_Recoder.isRecording))
                {
                    if (GUILayout.Button("Record"))
                    {
                        m_Recoder.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Recoder.isRecording))
                {
                    if (GUILayout.Button("Stop Recording"))
                    {
                        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + GenerateFileName());
                        var path = uniqueAssetPath + ".arstream";

                        m_Recoder.StopRecording();
                        
                        using (var fileStream = File.Create(path))
                        {
                            var buffer = m_Recoder.GetBuffer();
                            fileStream.Write(buffer, 0, buffer.Length);
                        }

                        AssetDatabase.Refresh();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                m_Clip = EditorGUILayout.ObjectField(m_Clip, typeof(AnimationClip), true) as AnimationClip;

                using (new EditorGUI.DisabledGroupScope(m_Player.isPlaying))
                {
                    if (GUILayout.Button("Play"))
                    {
                        m_Player.Play(m_Controller.GetComponent<Animator>(), m_Clip);
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Player.isPlaying))
                {
                    if (GUILayout.Button("Stop"))
                    {
                        m_Player.Stop();
                    }
                    if (GUILayout.Button("Pause"))
                    {
                        m_Player.Pause();
                    }
                }
            }
            //m_Server.adapterVersion = (AdapterVersion)EditorGUILayout.EnumPopup("Adapter Version", m_Server.adapterVersion);
        }

        string GenerateFileName()
        {
            return string.Format("{0:yyyy_MM_dd_HH_mm}-Take", DateTime.Now);
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
            data.FrameTime = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_Client.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }
    }
}
