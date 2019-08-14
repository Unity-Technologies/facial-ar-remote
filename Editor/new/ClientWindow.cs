using System;
using System.Collections.Generic;
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

    [Serializable]
    public class RemoteActor
    {
        static readonly string k_DefaultDirectory = "Assets/";
        PacketStream m_Stream = new PacketStream();
        FaceDataRecorder m_Recoder = new FaceDataRecorder();
        TakePlayer m_Player = new TakePlayer();
        PreviewState m_PrevState = PreviewState.None;
        [SerializeField]
        BlendShapesController m_Controller;
        [SerializeField]
        AnimationClip m_Clip;
        [SerializeField]
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
            m_Recoder.StopRecording();
            m_Player.Stop();
            m_Stream.Disconnect();
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

        public void WriteRecording(Stream stream)
        {
            var length = default(long);
            var buffer = m_Recoder.GetBuffer(out length);
            var descriptor = PacketDescriptor.Get(m_Recoder.packetType);
            var packetCount = new PacketCount
            {
                value = length / descriptor.GetPayloadSize()
            };

            stream.Write<PacketDescriptor>(descriptor);
            stream.Write<PacketCount>(packetCount);
            stream.Write(buffer, 0, (int)length);
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
            if (state == PreviewState.Playback)
                m_Player.Play(controller.GetComponent<Animator>(), m_Clip);
            else
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

    [InitializeOnLoad]
    public class BlendShapeControllerTracker
    {
        static List<BlendShapesController> s_Controllers = new List<BlendShapesController>();

        static BlendShapeControllerTracker()
        {
            BlendShapesController.controllerEnabled += ControllerEnabled;
            BlendShapesController.controllerDisabled += ControllerDisabled;
        }

        static void ControllerEnabled(BlendShapesController controller)
        {
            s_Controllers.Add(controller);
        }

        static void ControllerDisabled(BlendShapesController controller)
        {
            s_Controllers.Remove(controller);
        }

        public static BlendShapesController[] GetControllers()
        {
            return s_Controllers.ToArray();
        }
    }

    public class ClientWindow : EditorWindow
    {
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        static readonly string k_Assets = "Assets";
        PacketStream m_PacketStream = new PacketStream();
        [SerializeField]
        List<RemoteActor> m_Actors = new List<RemoteActor>();
        [SerializeField]
        Vector2 m_Scroll;

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
            BlendShapesController.controllerEnabled += ControllerEnabled;
            BlendShapesController.controllerDisabled += ControllerDisabled;

            var controllers = BlendShapeControllerTracker.GetControllers();

            foreach (var controller in controllers)
                ControllerEnabled(controller);
        }

        void OnDisable()
        {
            foreach (var actor in m_Actors)
                actor.Dispose();
            
            m_Actors.Clear();

            AnimationMode.StopAnimationMode();

            BlendShapesController.controllerEnabled -= ControllerEnabled;
            BlendShapesController.controllerDisabled -= ControllerDisabled;
            EditorApplication.update -= Update;
        }

        void ControllerEnabled(BlendShapesController controller)
        {
            var actor = new RemoteActor();
            actor.controller = controller;
            m_Actors.Add(actor);

            Repaint();
        }

        void ControllerDisabled(BlendShapesController controller)
        {
            var actor = m_Actors.Find((a) => a.controller == controller);

            if (actor != null)
            {
                actor.Dispose();
                m_Actors.Remove(actor);
            }

            Repaint();
        }

        void OnGUI()
        {
            using (var scrollview = new EditorGUILayout.ScrollViewScope(m_Scroll))
            {
                m_Scroll = scrollview.scrollPosition;

                foreach (var actor in m_Actors)
                    DoActorGUI(actor);
                
                /*
                using (new GUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("Client", EditorStyles.boldLabel);
                    ClientGUI();
                }
                */
            }
        }

        void DoActorGUI(RemoteActor actor)
        {
            if (actor == null || actor.controller == null)
                return;
            
            EditorGUIUtility.labelWidth = 60f;

            using (new GUILayout.VerticalScope("box"))
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField("Target", actor.controller.gameObject, typeof(GameObject), true);
                }

                EditorGUILayout.LabelField("Device", EditorStyles.boldLabel);
                DeviceGUI(actor);

                EditorGUILayout.LabelField("Recorder", EditorStyles.boldLabel);
                DoDirectoryGUI(actor);

                using (new EditorGUI.DisabledGroupScope(actor.state != PreviewState.LiveStream))
                {    
                    DoRecorderGUI(actor);
                }

                using (new EditorGUI.DisabledGroupScope(actor.state == PreviewState.LiveStream))
                {
                    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                    DoPlayerGUI(actor);
                }
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        void Update()
        {
            foreach (var actor in m_Actors)
                actor.Update();
        }

        void DeviceGUI(RemoteActor actor)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(actor.state != PreviewState.None))
                {
                    if (GUILayout.Button("Connect", EditorStyles.miniButton, kButtonWide))
                    {
                        actor.Connect();

                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actor.controller.gameObject);
                    }
                }
                using (new EditorGUI.DisabledGroupScope(actor.state != PreviewState.LiveStream))
                {
                    if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                    {
                        actor.Disconnect();
                        StopAnimationMode();
                    }
                }
            }
            //m_Server.adapterVersion = (AdapterVersion)EditorGUILayout.EnumPopup("Adapter Version", m_Server.adapterVersion);
        }

        void DoDirectoryGUI(RemoteActor actor)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Directory", actor.directory);

                if (GUILayout.Button("o", EditorStyles.miniButton, kButtonSmall))
                {
                    var path = EditorUtility.OpenFolderPanel("Record Directory", actor.directory, "");

                    if (!string.IsNullOrEmpty(path) && path != actor.directory)
                    {
                        var index = path.IndexOf(k_Assets);
                        
                        actor.directory = path.Substring(index) + "/";
                    }
                }
            }
        }

        void DoRecorderGUI(RemoteActor actor)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(actor.IsRecording()))
                {
                    if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    {
                        actor.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!actor.IsRecording()))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        actor.StopRecording();

                        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(actor.directory + GenerateFileName());
                        var path = uniqueAssetPath + ".arstream";

                        using (var fileStream = File.Create(path))
                        {
                            actor.WriteRecording(fileStream);
                        }

                        AssetDatabase.Refresh();

                        actor.clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                        EditorGUIUtility.PingObject(actor.clip);
                    }
                }
            }
        }

        void DoPlayerGUI(RemoteActor actor)
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                actor.clip = EditorGUILayout.ObjectField("Clip", actor.clip, typeof(AnimationClip), true) as AnimationClip;

                if (change.changed)
                {
                    var wasPlaying = actor.state == PreviewState.Playback;

                    actor.StopPlayback();
                    StopAnimationMode();

                    if (wasPlaying && actor.clip != null)
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actor.controller.gameObject, actor.clip);
                        actor.StartPlayback();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledGroupScope(actor.clip == null || actor.controller == null))
            {
                GUILayout.FlexibleSpace();

                if (actor.player.isPlaying)
                {
                    if (GUILayout.Button("Pause", EditorStyles.miniButton, kButtonMid))
                    {
                        actor.PausePlayback();
                    }
                }
                else
                {
                    if (GUILayout.Button("Play", EditorStyles.miniButton, kButtonMid))
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actor.controller.gameObject, actor.clip);
                        actor.StartPlayback();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!(actor.player.isPlaying || actor.player.isPaused)))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        actor.StopPlayback();
                        StopAnimationMode();
                    }
                }
            }
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
                    m_PacketStream.ip = "127.0.0.1";
                    m_PacketStream.port = 9000;
                    m_PacketStream.isServer = false;
                    m_PacketStream.Connect();
                }
                if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonWide))
                    m_PacketStream.Disconnect();
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
            
            m_PacketStream.writer.Write(faceData);
            */
            
            var data = new StreamBufferDataV1();
            data.FrameTime = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }

        void StartAnimationMode()
        {
            if (AnimationMode.InAnimationMode())
                return;

            AnimationMode.StartAnimationMode();
        }

        void StopAnimationMode()
        {
            if (AnimationMode.InAnimationMode())
            {
                foreach (var actor in m_Actors)
                {
                    if (actor.state != PreviewState.None)
                        return;
                }

                AnimationMode.StopAnimationMode();
            }
        }

        void RegisterBindingsToAnimationMode(GameObject go)
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

        void RegisterBindingsToAnimationMode(GameObject go, AnimationClip clip)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
                AnimationMode.AddEditorCurveBinding(go, binding);
        }
    }
}
