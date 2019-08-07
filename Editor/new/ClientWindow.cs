using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public enum PreviewState
    {
        None,
        Playback,
        LiveStream
    }

    public class RemoteActor
    {
        static readonly string k_DefaultDirectory = "Assets/";
        RemoteStream m_Stream = new RemoteStream();
        FaceDataRecorder m_Recoder = new FaceDataRecorder();
        TakePlayer m_Player = new TakePlayer();
        BlendShapesController m_Controller;
        PreviewState m_PrevState = PreviewState.None;
        AnimationClip m_Clip;
        string m_Directory = k_DefaultDirectory;
        bool m_ChangingState = false;

        public TakePlayer player
        {
            get { return m_Player; }
        }

        public PreviewState state
        {
            get { return m_PrevState; }
        }

        public BlendShapesController controller
        {
            get { return m_Controller; }
            set { m_Controller = value; }
        }

        public AnimationClip clip
        {
            get { return m_Clip; }
            set { m_Clip = value; }
        }

        public string directory
        {
            get { return m_Directory; }
            set { m_Directory = value; }
        }

        public RemoteActor()
        {
            m_Stream.reader.faceDataChanged += FaceDataChanged;
        }

        ~RemoteActor()
        {
            Dispose();
        }

        public void Dispose()
        {
            Disconnect();
            m_Stream.reader.faceDataChanged -= FaceDataChanged;
        }

        void SetPreviewState(PreviewState newState)
        {
            if (m_ChangingState)
                return;
            
            if (m_PrevState == newState)
                return;
            
            m_ChangingState = true;

            if (m_PrevState == PreviewState.LiveStream)
            {
                StopRecording();
            }
            else if (m_PrevState == PreviewState.Playback)
            {
                if (m_Player.isPlaying)
                    m_Player.Stop();
            }

            if (newState == PreviewState.LiveStream)
            {

            }
            else if (newState == PreviewState.Playback)
            {
                var animator = m_Controller.GetComponent<Animator>();
                m_Player.Play(animator, m_Clip);
            }

            m_PrevState = newState;
            m_ChangingState = false;
        }

        public void Connect()
        {
            m_Stream.isServer = true;
            m_Stream.Connect();
            SetPreviewState(PreviewState.LiveStream);
        }

        public void Disconnect()
        {
            m_Stream.Disconnect();
            SetPreviewState(PreviewState.None);
        }

        public void Update()
        {
            m_Stream.reader.Receive();
            
            if (m_PrevState == PreviewState.Playback)
                m_Player.Update();    
        }

        public void StartRecording()
        {
            if (m_PrevState != PreviewState.LiveStream)
                return;

            m_Recoder.StartRecording();
        }

        public bool IsRecording()
        {
            return m_Recoder.isRecording;
        }

        public void StopRecording()
        {
            if (!m_Recoder.isRecording)
                return;
            
            m_Recoder.StopRecording();
        }

        public byte[] GetRecording()
        {
            return m_Recoder.GetBuffer();
        }

        public void StartLiveStream()
        {
            SetPreviewState(PreviewState.LiveStream);
        }

        public void StopLiveStream()
        {
            if (m_PrevState != PreviewState.LiveStream)
                return;

            SetPreviewState(PreviewState.None);
        }

        public void StartPlayback()
        {
            SetPreviewState(PreviewState.Playback);
        }

        public void StopPlayback()
        {
            if (m_PrevState != PreviewState.Playback)
                return;
            
            SetPreviewState(PreviewState.None);
        }

        public void PausePlayback()
        {
            m_Player.Pause();
        }

        void FaceDataChanged(FaceData data)
        {
            if (m_Controller == null)
                return;

            if (state != PreviewState.LiveStream)
                return;

            m_Controller.blendShapeInput = data.blendShapeValues;
            m_Controller.UpdateBlendShapes();

            if (m_Recoder.isRecording)
                m_Recoder.Record(data);
        }
    }

    public class ClientWindow : EditorWindow
    {
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        static readonly string kAssets = "Assets";
        RemoteStream m_Client = new RemoteStream();
        RemoteActor m_Actor = new RemoteActor();

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
            m_Actor.Dispose();
            AnimationMode.StopAnimationMode();

            EditorApplication.update -= Update;
        }

        void FindController()
        {
            if (m_Actor.controller == null)
                m_Actor.controller = GameObject.FindObjectOfType<BlendShapesController>();
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

                using (new EditorGUI.DisabledGroupScope(m_Actor.state != PreviewState.LiveStream))
                {    
                    DoRecorderGUI();
                }

                using (new EditorGUI.DisabledGroupScope(m_Actor.state == PreviewState.LiveStream))
                {
                    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                    DoPlayerGUI();
                }
            }

            /*
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Client", EditorStyles.boldLabel);
                ClientGUI();
            }
            */

            EditorGUIUtility.labelWidth = 0f;
        }

        void Update()
        {
            m_Actor.Update();
        }

        void DeviceGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_Actor.state != PreviewState.None))
                {
                    if (GUILayout.Button("Connect", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Actor.Connect();

                        AnimationMode.StartAnimationMode();
                        PrepareAnimationModeCurveBindings(m_Actor.controller.gameObject);
                    }
                }
                using (new EditorGUI.DisabledGroupScope(m_Actor.state != PreviewState.LiveStream))
                {
                    if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Actor.Disconnect();

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
                EditorGUILayout.TextField("Directory", m_Actor.directory);

                if (GUILayout.Button("o", EditorStyles.miniButton, kButtonSmall))
                {
                    var path = EditorUtility.OpenFolderPanel("Record Directory", m_Actor.directory, "");

                    if (path != m_Actor.directory)
                    {
                        var index = path.IndexOf(kAssets);
                        
                        m_Actor.directory = path.Substring(index) + "/";
                    }
                }
            }
        }

        void DoRecorderGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_Actor.IsRecording()))
                {
                    if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    {
                        m_Actor.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Actor.IsRecording()))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        m_Actor.StopRecording();

                        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(m_Actor.directory + GenerateFileName());
                        var path = uniqueAssetPath + ".arstream";

                        using (var fileStream = File.Create(path))
                        {
                            var buffer = m_Actor.GetRecording();
                            fileStream.Write(buffer, 0, buffer.Length);
                        }

                        AssetDatabase.Refresh();

                        m_Actor.clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                        EditorGUIUtility.PingObject(m_Actor.clip);
                    }
                }
            }
        }

        void DoPlayerGUI()
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_Actor.clip = EditorGUILayout.ObjectField("Clip", m_Actor.clip, typeof(AnimationClip), true) as AnimationClip;

                if (change.changed)
                {
                    var wasPlaying = m_Actor.state == PreviewState.Playback;

                    m_Actor.StopPlayback();

                    if (wasPlaying)
                        m_Actor.StartPlayback();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledGroupScope(m_Actor.clip == null || m_Actor.controller == null))
            {
                GUILayout.FlexibleSpace();

                if (m_Actor.player.isPlaying)
                {
                    if (GUILayout.Button("Pause", EditorStyles.miniButton, kButtonMid))
                    {
                        m_Actor.PausePlayback();
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledGroupScope(IsAnimationModeInExternalUse()))
                    {
                        if (GUILayout.Button("Play", EditorStyles.miniButton, kButtonMid))
                        {
                            AnimationMode.StartAnimationMode();
                            PrepareAnimationModeCurveBindings(m_Actor.controller.gameObject);

                            m_Actor.StartPlayback();
                        }
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!(m_Actor.player.isPlaying || m_Actor.player.isPaused)))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        m_Actor.StopPlayback();

                        AnimationMode.StopAnimationMode();
                    }
                }
            }
        }

        bool IsAnimationModeInExternalUse()
        {
            return AnimationMode.InAnimationMode() && m_Actor.state != PreviewState.None;
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
