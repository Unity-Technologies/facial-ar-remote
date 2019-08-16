using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
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

    public class PerformanceCaptureWindow : EditorWindow
    {
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        static readonly string k_Assets = "Assets";
        [SerializeField]
        List<RemoteActor> m_Actors = new List<RemoteActor>();
        [SerializeField]
        Vector2 m_Scroll;

        [MenuItem("Window/Performance Capture")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(PerformanceCaptureWindow));
            window.titleContent = new GUIContent("Performance Capture");;
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

        /*
        PacketStream m_PacketStream = new PacketStream();
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
        */

        /*
        void SendPacket()
        {
            /*
            var faceData = new FaceData();
            faceData.timeStamp = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                faceData.blendShapeValues[i] = UnityEngine.Random.value;
            
            m_PacketStream.writer.Write(faceData);
            */
            /*
            var data = new StreamBufferDataV1();
            data.FrameTime = Time.realtimeSinceStartup;

            for (var i = 0; i < BlendShapeValues.Count; ++i)
                data.BlendshapeValues[i] = UnityEngine.Random.value;

            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }
        */

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
