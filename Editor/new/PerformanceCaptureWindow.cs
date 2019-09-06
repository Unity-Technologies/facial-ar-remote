using System;
using System.Net;
using System.Collections.Generic;
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
        static readonly GUILayoutOption kButtonLarge = GUILayout.Width(80f);
        static readonly string k_Assets = "Assets";

        [SerializeField]
        List<ActorServer> m_ActorServers = new List<ActorServer>();

        [SerializeField]
        Vector2 m_Scroll;

        IPAddress[] m_ServerAddresses;

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
            ActorServer.actorServerChanged += ActorServerChanged;

            var actors = ActorTracker.GetActors();

            foreach (var actor in actors)
                ActorEnabled(actor);

            m_ServerAddresses = NetworkUtilities.GetIPAddresses();
        }

        void OnDisable()
        {
            foreach (var actorServer in m_ActorServers)
                actorServer.Dispose();
            
            m_ActorServers.Clear();

            AnimationMode.StopAnimationMode();

            ActorServer.actorServerChanged -= ActorServerChanged;
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

        void ActorServerChanged(ActorServer actorServer)
        {
            switch (actorServer.state) 
            {
                case PreviewState.LiveStream:
                {
                    if (!AnimationMode.InAnimationMode())
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actorServer.actor.gameObject);
                    }

                    break;
                }
                case PreviewState.Playback:
                {
                    if (!AnimationMode.InAnimationMode())
                    {
                        StartAnimationMode();
                        RegisterBindingsToAnimationMode(actorServer.actor.gameObject, actorServer.clip);
                    }

                    break;
                }
                case PreviewState.None:
                {
                    if (AnimationMode.InAnimationMode())
                    {
                        StopAnimationMode();
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Repaint();
        }

        void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(m_Scroll))
            {
                m_Scroll = scrollView.scrollPosition;

                foreach (var actor in m_ActorServers)
                    DoActorGUI(actor);
            }
        }

        void DoActorGUI(ActorServer actorServer)
        {
            if (actorServer == null || actorServer.actor == null)
                return;
            
            EditorGUIUtility.labelWidth = 100f;

            using (new GUILayout.VerticalScope("box"))
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField("Target", actorServer.actor.gameObject, typeof(GameObject), true);
                }

                EditorGUILayout.LabelField("Server", EditorStyles.boldLabel);
                ServerGUI(actorServer);

                EditorGUILayout.LabelField("Client", EditorStyles.boldLabel);
                ClientGUI(actorServer);

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

                EditorGUILayout.LabelField("State", EditorStyles.boldLabel);
                
                actorServer.OnGUI();
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        void Update()
        {
            foreach (var actor in m_ActorServers)
                actor.Update();
        }

        void ServerGUI(ActorServer actorServer)
        {
            EditorGUILayout.LabelField("Available Interfaces");

            EditorGUI.indentLevel++;

            foreach (var address in m_ServerAddresses)
                EditorGUILayout.LabelField(address.ToString());

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh", EditorStyles.miniButton, kButtonWide))
                {
                    m_ServerAddresses = NetworkUtilities.GetIPAddresses();
                }
            }

            EditorGUI.indentLevel--;

            actorServer.port = EditorGUILayout.IntField("Port", actorServer.port);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(actorServer.IsServerRunning()))
                {
                    if (GUILayout.Button("Start", EditorStyles.miniButton, kButtonWide))
                    {
                        actorServer.StartServer();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!actorServer.IsServerRunning()))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonWide))
                    {
                        actorServer.StopServer();
                    }
                }
            }
        }

        void ClientGUI(ActorServer actorServer)
        {
            var status = new GUIContent("Disconnected");

            if (actorServer.IsClientConnected())
                status = new GUIContent("Connected");

            EditorGUILayout.LabelField(new GUIContent("Status"), status);

            using (new EditorGUILayout.HorizontalScope())
            {
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(!actorServer.IsClientConnected()))
                {
                    if (GUILayout.Button("Disconnect", EditorStyles.miniButton, kButtonLarge))
                    {
                        actorServer.DisconnectClient();
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

                    if (wasPlaying && actorServer.clip != null)
                    {
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
                        actorServer.StartPlayback();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!(actorServer.player.isPlaying || actorServer.player.isPaused)))
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButton, kButtonMid))
                    {
                        actorServer.StopPlayback();
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
