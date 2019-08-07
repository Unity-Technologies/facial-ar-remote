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
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        static readonly string kAssets = "Assets";
        RemoteStream m_Server = new RemoteStream();
        RemoteStream m_Client = new RemoteStream();
        FaceDataRecorder m_Recoder = new FaceDataRecorder();
        string m_RecorderDirectory = kAssets;
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

            EditorGUIUtility.labelWidth = 60f;

            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Device", EditorStyles.boldLabel);
                DeviceGUI();

                EditorGUILayout.LabelField("Recorder", EditorStyles.boldLabel);
                DoDirectoryGUI();
                DoRecorderGUI();

                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                DoPlayerGUI();
            }
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Client", EditorStyles.boldLabel);
                ClientGUI();
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        void Update()
        {
            m_Server.reader.Receive();

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                Stop();

            if (m_Player.isPlaying)
                m_Player.Update();
        }

        void FaceDataChanged(FaceData data)
        {
            if (m_Controller != null)
            {
                Stop();

                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                    PrepareAnimationModeCurveBindings(m_Controller.gameObject);
                }

                m_Controller.blendShapeInput = data.blendShapeValues;
                m_Controller.UpdateBlendShapes();
            }

            if (m_Recoder.isRecording)
                m_Recoder.Record(data);
        }

        void DeviceGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_Server.isListening))
                {
                    if (GUILayout.Button("Connect", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Server.isServer = true;
                        m_Server.Connect();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Server.isListening))
                {
                    if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Server.Disconnect();

                        if (AnimationMode.InAnimationMode())
                            AnimationMode.StopAnimationMode();
                    }
                }
            }
            //m_Server.adapterVersion = (AdapterVersion)EditorGUILayout.EnumPopup("Adapter Version", m_Server.adapterVersion);
        }

        void DoDirectoryGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Directory", m_RecorderDirectory);

                if (GUILayout.Button("o", EditorStyles.miniButton, kButtonSmall))
                {
                    var path = EditorUtility.OpenFolderPanel("Record Directory", m_RecorderDirectory, "");

                    if (path != m_RecorderDirectory)
                    {
                        var index = path.IndexOf(kAssets);
                        
                        m_RecorderDirectory = path.Substring(index) + "/";
                    }
                }
            }
        }

        void DoRecorderGUI()
        {
            using (new EditorGUI.DisabledGroupScope(!m_Player.isStopped))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_Recoder.isRecording))
                {
                    if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Recoder.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Recoder.isRecording))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(m_RecorderDirectory + GenerateFileName());
                        var path = uniqueAssetPath + ".arstream";

                        m_Recoder.StopRecording();
                        
                        using (var fileStream = File.Create(path))
                        {
                            var buffer = m_Recoder.GetBuffer();
                            fileStream.Write(buffer, 0, buffer.Length);
                        }

                        AssetDatabase.Refresh();

                        m_Clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                        EditorGUIUtility.PingObject(m_Clip);
                    }
                }
            }
        }

        void DoPlayerGUI()
        {
            using (new EditorGUI.DisabledGroupScope(m_Recoder.isRecording))
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    m_Clip = EditorGUILayout.ObjectField("Clip", m_Clip, typeof(AnimationClip), true) as AnimationClip;

                    if (change.changed)
                    {
                        var wasPlaying = m_Player.isPlaying;

                        Stop();

                        if (wasPlaying)
                            Play();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                using (new EditorGUI.DisabledGroupScope(m_Clip == null || m_Controller == null))
                {
                    GUILayout.FlexibleSpace();

                    if (m_Player.isPlaying)
                    {
                        if (GUILayout.Button("Pause", EditorStyles.miniButton, kButtonMid))
                            Pause();
                    }
                    else
                    {
                        using (new EditorGUI.DisabledGroupScope(IsAnimationModeInExternalUse()))
                        {
                            if (GUILayout.Button("Play", EditorStyles.miniButton, kButtonMid))
                                Play();
                        }
                    }
                    using (new EditorGUI.DisabledGroupScope(!(m_Player.isPlaying || m_Player.isPaused)))
                    {
                        if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                            Stop();
                    }
                }
            }
        }

        bool IsAnimationModeInExternalUse()
        {
            return AnimationMode.InAnimationMode() && m_Player.isStopped;
        }

        void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                IsAnimationModeInExternalUse() ||
                m_Clip == null ||
                m_Controller == null)
                return;
            
            var animator = m_Controller.GetComponent<Animator>();

            if (m_Player.isStopped)
            {
                AnimationMode.StartAnimationMode();

                var bindings = AnimationUtility.GetCurveBindings(m_Clip);
                foreach (var binding in bindings)
                    AnimationMode.AddEditorCurveBinding(animator.gameObject, binding);
            }
                
            m_Player.Play(animator, m_Clip);
        }

        void Stop()
        {
            if (!m_Player.isStopped)
            {
                m_Player.Stop();

                if (AnimationMode.InAnimationMode())
                    AnimationMode.StopAnimationMode();
            }
        }

        void Pause()
        {
            m_Player.Pause();
        }

        string GenerateFileName()
        {
            return string.Format("{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
        }

        void ClientGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Connect", EditorStyles.miniButton, kButtonWide))
                {
                    m_Client.ip = "127.0.0.1";
                    m_Client.port = 9000;
                    m_Client.isServer = false;
                    m_Client.Connect();
                }
                if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                    m_Client.Disconnect();
                if (GUILayout.Button("Send", EditorStyles.miniButton, kButtonMid))
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

        void PrepareAnimationModeCurveBindings(GameObject go)
        {
            for (var i = 0; i < BlendShapeValues.Count; ++i)
            {
                var binding = new EditorCurveBinding()
                {
                    path = "",
                    type = typeof(BlendShapesController),
                    propertyName = "m_BlendShapeValues." + ((BlendShapeLocation)i).ToString() 
                };
                AnimationMode.AddEditorCurveBinding(go, binding);
            }
        }
    }
}
