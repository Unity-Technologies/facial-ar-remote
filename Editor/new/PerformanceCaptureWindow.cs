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
    public class ActorTracker
    {
        static List<Actor> s_Actors = new List<Actor>();

        static ActorTracker()
        {
            Actor.actorEnabled += ActorEnabled;
            Actor.actorDisabled += ActorDisabled;
        }

        static void ActorEnabled(Actor actor)
        {
            s_Actors.Add(actor);
        }

        static void ActorDisabled(Actor actor)
        {
            s_Actors.Remove(actor);
        }

        public static Actor[] GetActors()
        {
            return s_Actors.ToArray();
        }
    }

    public class PerformanceCaptureWindow : EditorWindow
    {
        static readonly GUILayoutOption kButtonSmall = GUILayout.Width(20f);
        static readonly GUILayoutOption kButtonMid = GUILayout.Width(36f);
        static readonly GUILayoutOption kButtonWide = GUILayout.Width(60f);
        static readonly string k_Assets = "Assets";
        [SerializeField]
        List<ActorServer> m_ActorServers = new List<ActorServer>();
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
            Actor.actorEnabled += ActorEnabled;
            Actor.actorDisabled += ActorDisabled;
            ActorServer.recordingStateChanged += RecordingStateChanged;

            var actors = ActorTracker.GetActors();

            foreach (var actor in actors)
                ActorEnabled(actor);
        }

        void OnDisable()
        {
            foreach (var actorServer in m_ActorServers)
                actorServer.Dispose();
            
            m_ActorServers.Clear();

            AnimationMode.StopAnimationMode();

            ActorServer.recordingStateChanged -= RecordingStateChanged;
            Actor.actorEnabled -= ActorEnabled;
            Actor.actorDisabled -= ActorDisabled;
            EditorApplication.update -= Update;
        }

        void ActorEnabled(Actor actor)
        {
            ActorServer actorServer = null;

            if (actor is BlendShapesController)
                actorServer = new FaceActorServer();
            else if (actor is VirtualCameraActor)
                actorServer = new VirtualCameraActorServer();

            if (actorServer != null)
            {
                actorServer.actor = actor;
                m_ActorServers.Add(actorServer);
            }

            Repaint();
        }

        void ActorDisabled(Actor actor)
        {
            var remoteActor = m_ActorServers.Find((a) => a.actor == actor);

            if (remoteActor != null)
            {
                remoteActor.Dispose();
                m_ActorServers.Remove(remoteActor);
            }

            Repaint();
        }

        void RecordingStateChanged(ActorServer actor)
        {
            Repaint();
        }

        void OnGUI()
        {
            using (var scrollview = new EditorGUILayout.ScrollViewScope(m_Scroll))
            {
                m_Scroll = scrollview.scrollPosition;

                foreach (var actor in m_ActorServers)
                    DoActorGUI(actor);
            }
        }

        void DoActorGUI(ActorServer actorServer)
        {
            if (actorServer == null || actorServer.actor == null)
                return;
            
            EditorGUIUtility.labelWidth = 60f;

            using (new GUILayout.VerticalScope("box"))
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField("Target", actorServer.actor.gameObject, typeof(GameObject), true);
                }

                EditorGUILayout.LabelField("Device", EditorStyles.boldLabel);
                DeviceGUI(actorServer);

                EditorGUILayout.LabelField("Recorder", EditorStyles.boldLabel);
                DoDirectoryGUI(actorServer);

                using (new EditorGUI.DisabledGroupScope(actorServer.state != PreviewState.LiveStream))
                {    
                    DoRecorderGUI(actorServer);
                }

                using (new EditorGUI.DisabledGroupScope(actorServer.state == PreviewState.LiveStream))
                {
                    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                    DoPlayerGUI(actorServer);
                }
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        void Update()
        {
            foreach (var actor in m_ActorServers)
                actor.Update();
        }

        void DeviceGUI(ActorServer actor)
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
                        RegisterBindingsToAnimationMode(actor.actor.gameObject);
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
        }

        void DoDirectoryGUI(ActorServer actor)
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

        void DoRecorderGUI(ActorServer actorServer)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(actorServer.IsRecording()))
                {
                    if (GUILayout.Button("Record", EditorStyles.miniButton, kButtonWide))
                    {
                        actorServer.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!actorServer.IsRecording()))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        actorServer.StopRecording();
                    }
                }
            }
        }

        void DoPlayerGUI(ActorServer actorServer)
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                actorServer.clip = EditorGUILayout.ObjectField("Clip", actorServer.clip, typeof(AnimationClip), true) as AnimationClip;

                if (change.changed)
                {
                    var wasPlaying = actorServer.state == PreviewState.Playback;

                    actorServer.StopPlayback();
                    StopAnimationMode();

                    if (wasPlaying && actorServer.clip != null)
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actorServer.actor.gameObject, actorServer.clip);
                        actorServer.StartPlayback();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledGroupScope(actorServer.clip == null || actorServer.actor == null))
            {
                GUILayout.FlexibleSpace();

                if (actorServer.player.isPlaying)
                {
                    if (GUILayout.Button("Pause", EditorStyles.miniButton, kButtonMid))
                    {
                        actorServer.PausePlayback();
                    }
                }
                else
                {
                    if (GUILayout.Button("Play", EditorStyles.miniButton, kButtonMid))
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actorServer.actor.gameObject, actorServer.clip);
                        actorServer.StartPlayback();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!(actorServer.player.isPlaying || actorServer.player.isPaused)))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        actorServer.StopPlayback();
                        StopAnimationMode();
                    }
                }
            }
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
                foreach (var actor in m_ActorServers)
                {
                    if (actor.state != PreviewState.None)
                        return;
                }

                AnimationMode.StopAnimationMode();
            }
        }

        void RegisterBindingsToAnimationMode(GameObject go)
        {
            for (var i = 0; i < BlendShapeValues.count; ++i)
            {
                var binding = new EditorCurveBinding()
                {
                    path = "",
                    type = typeof(BlendShapesController),
                    propertyName = "m_BlendShapeValues." + ((BlendShapeLocation)i).ToString()
                };
                AnimationMode.AddEditorCurveBinding(go, binding);
            }

            foreach (var axis in new string [] { "x", "y", "z" })
            {
                var binding = new EditorCurveBinding()
                {
                    path = "",
                    type = typeof(Transform),
                    propertyName = "m_LocalPosition." + axis
                };
                AnimationMode.AddEditorCurveBinding(go, binding);
            }

            foreach (var q in new string [] { "x", "y", "z", "w" })
            {
                var binding = new EditorCurveBinding()
                {
                    path = "",
                    type = typeof(Transform),
                    propertyName = "m_LocalRotation." + q
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
