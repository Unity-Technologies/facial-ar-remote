using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class ARFaceCaptureWindow : EditorWindow
    {
        enum StreamMode
        {
            StreamFromDevice,
            StreamFromFile
        }
        
        const int k_ProgressBarHeight = 22;
        
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_StreamReaderPrefab;
#pragma warning restore CS0649

        Dictionary<StreamReader, StreamMode> m_StreamReaderModes = new Dictionary<StreamReader, StreamMode>();
        
        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;
        
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_ConnectIcon;
        
        ClipBaker m_ClipBaker;

        PlaybackData m_PlaybackData;

        [MenuItem("Window/AR Face Capture")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(ARFaceCaptureWindow));
            window.titleContent = new GUIContent("AR Face Capture");
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            m_PlayIcon = EditorGUIUtility.IconContent("d_Animation.Play");
            m_RecordIcon = EditorGUIUtility.IconContent("d_Animation.Record");
            m_ConnectIcon = EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small");
        }

        void OnGUI()
        {
            SetupGUIStyles();
            
            var streamReaders = FindObjectsOfType<StreamReader>();
            if (streamReaders.Length == 0)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.Space();
                GUILayout.Label("Add a game object to the scene with a StreamReader");
                EditorGUILayout.Space();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.Space();
                GUILayout.Label("component or click the button to add a Stream Reader prefab.");
                EditorGUILayout.Space();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create", GUILayout.Width(100)))
                {
                    PrefabUtility.InstantiatePrefab(m_StreamReaderPrefab);
                    streamReaders = new[] { FindObjectOfType<StreamReader>() };
                    streamReaders[0].ConnectDependencies();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                return;
            }
            
            foreach (var streamReader in streamReaders)
            {
                if (!m_StreamReaderModes.ContainsKey(streamReader))
                    m_StreamReaderModes.Add(streamReader, StreamMode.StreamFromDevice);
                
                NetworkStream networkStream = null;
                PlaybackStream playbackStream = null;
                
                foreach (var source in streamReader.sources)
                {
                    var network = source as NetworkStream;
                    if (network != null)
                    {
                        networkStream = network;
                    }

                    var playback = source as PlaybackStream;
                    if (playback != null)
                    {
                        playbackStream = playback;
                    }
                }

                using (new GUILayout.VerticalScope("box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Stream Reader");
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.ObjectField(streamReader, typeof(StreamReader), true);
                        }
                    }
                    EditorGUILayout.Space();

                    using (new GUILayout.HorizontalScope())
                    {
                        m_StreamReaderModes[streamReader] = (StreamMode)EditorGUILayout.EnumPopup("", m_StreamReaderModes[streamReader]);

                        if (m_StreamReaderModes[streamReader] == StreamMode.StreamFromDevice)
                        {
                            if (GUILayout.Button(m_ConnectIcon, m_ButtonStyle))
                            {
                                using (new EditorGUI.DisabledGroupScope(networkStream == null || !networkStream.active))
                                {
                                    var streamSource = streamReader.streamSource;
                                    if (streamSource != null && streamSource.Equals(networkStream)
                                        && networkStream != null && networkStream.active)
                                    {
                                        if (GUILayout.Button(m_ConnectIcon, m_ButtonPressStyle))
                                            streamReader.streamSource = null;
                                    }
                                    else
                                    {
                                        if (GUILayout.Button(m_ConnectIcon, m_ButtonStyle))
                                            streamReader.streamSource = networkStream;
                                    }
                                }
                            }

                            var useRecorder = Application.isEditor && Application.isPlaying
                                && playbackStream != null && playbackStream.playbackData != null;
                            using (new EditorGUI.DisabledGroupScope(networkStream == null || !(networkStream.active && useRecorder)))
                            {
                                if (networkStream == null)
                                {
                                    GUILayout.Button(m_RecordIcon, m_ButtonStyle);
                                }
                                else if (networkStream.recording)
                                {
                                    if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                                        networkStream.StopRecording();
                                }
                                else
                                {
                                    if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                                        networkStream.StartRecording();
                                }
                            }
                        }
                        else if (m_StreamReaderModes[streamReader] == StreamMode.StreamFromFile)
                        {
                            using (new EditorGUI.DisabledGroupScope(networkStream == null || playbackStream == null
                                || !(networkStream.active || playbackStream.activePlaybackBuffer != null)))
                            {
                                if (playbackStream == null)
                                {
                                    GUILayout.Button(m_PlayIcon, m_ButtonStyle);
                                }
                                else if (playbackStream.active)
                                {
                                    if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                                    {
                                        foreach (var sr in streamReaders)
                                        {
                                            sr.streamSource = null;
                                        }

                                        playbackStream.StopPlayback();
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                                    {
                                        foreach (var sr in streamReaders)
                                        {
                                            sr.streamSource = playbackStream;
                                        }

                                        playbackStream.StartPlayback();
                                    }
                                }
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    if (m_StreamReaderModes[streamReader] == StreamMode.StreamFromDevice)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Source");
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.ObjectField(networkStream, typeof(NetworkStream), true);
                            }
                        }
                        
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Recorder");
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.ObjectField(playbackStream, typeof(PlaybackStream), true);
                            }
                        }
                    }
                    else if (m_StreamReaderModes[streamReader] == StreamMode.StreamFromFile)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Playback Stream");
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.ObjectField(playbackStream, typeof(PlaybackStream), true);
                            }
                        }
                        
                        EditorGUILayout.Space();
                        
                        if (m_ClipBaker == null)
                        {
                            var clipName = playbackStream == null || playbackStream.activePlaybackBuffer == null
                                ? "None"
                                : playbackStream.activePlaybackBuffer.name;

                            using (new EditorGUI.DisabledGroupScope(playbackStream == null))
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField("Playback Buffer", GUILayout.Width(100));
                                    
                                    if (playbackStream == null)
                                    {
                                        if (GUILayout.Button("Create new Playback Data asset"))
                                        {
                                            if (GUILayout.Button("Create new Playback Data asset")) { }
                                        }
                                    }
                                    else if (playbackStream.playbackData == null)
                                    {
                                        if (GUILayout.Button("Create new Playback Data asset"))
                                        {
                                            var asset = CreateInstance<PlaybackData>();

                                            AssetDatabase.CreateAsset(asset, "Assets/New Playback Data.asset");
                                            AssetDatabase.SaveAssets();
                                            playbackStream.playbackData = asset;
                                        }
                                    }
                                    else
                                    {
                                        using (new EditorGUI.DisabledGroupScope(playbackStream == null ||
                                            playbackStream.playbackData == null ||
                                            playbackStream.playbackData.playbackBuffers == null))
                                        {
                                            if (GUILayout.Button($"Play Stream: {clipName}"))
                                                ShowRecordStreamMenu(playbackStream, playbackStream.playbackData.playbackBuffers);
                                        }
                                    }
                                }
                                
                                if (playbackStream == null)
                                    EditorGUILayout.HelpBox("The Stream Reader does not have a Playback" +
                                        " Stream assigned.", MessageType.Warning);
                            }

                            EditorGUILayout.Space();

                            // Bake Clip Button
                            using (new EditorGUI.DisabledGroupScope(playbackStream == null || playbackStream.activePlaybackBuffer == null
                                || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
                            {
                                GUILayout.BeginHorizontal ();
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Bake Animation Clip", GUILayout.Width(150)))
                                {
                                    streamReader.streamSource = null;

                                    // Used to initialize values if they were changed before baking.
                                    streamReader.ConnectDependencies();

                                    var assetPath = Application.dataPath;
                                    var path = EditorUtility.SaveFilePanel("Save stream as animation clip",
                                        assetPath, clipName + ".anim", "anim");

                                    path = path.Replace(assetPath, "Assets");

                                    if (path.Length != 0)
                                    {
                                        var blendShapeController = streamReader.blendShapesController;

                                        var avatarController = streamReader.characterRigController;

                                        streamReader.streamSource = playbackStream;
                                        playbackStream.StartPlayback();

                                        var animClip = new AnimationClip();
                                        m_ClipBaker = new ClipBaker(animClip, streamReader, playbackStream,
                                            blendShapeController, avatarController, path);
                                    }
                                }
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal ();
                            }
                            
                            EditorGUILayout.Space();
                        }
                        else
                        {
                            BakeClipLoop();
                        }
                    }

//                    if (m_StreamReaderModes[streamReader] == StreamMode.StreamFromFile)
//                    {
//                        EditorGUILayout.LabelField("Not connected");
//                    }
                }
                
            }
        }
        
        void SetupGUIStyles()
        {
            if (m_ButtonStyle == null || m_ButtonPressStyle == null)
            {
                m_ButtonStyle = new GUIStyle("Button")
                {
                    fixedHeight = 24,
                    fixedWidth = 24
                };

                m_ButtonPressStyle = new GUIStyle("Button")
                {
                    active = m_ButtonStyle.normal,
                    normal = m_ButtonStyle.active,
                    fixedHeight = 24,
                    fixedWidth = 24
                };
            }
        }

        static void ShowRecordStreamMenu(PlaybackStream playbackStream, PlaybackBuffer[] buffers)
        {
            var menu = new GenericMenu();
            foreach (var buffer in buffers)
            {
                if (buffer.recordStream == null || buffer.recordStream.Length < 1)
                    continue;

                var label = new GUIContent(buffer.name);
                var playbackBuffer = buffer;
                var isActive = playbackStream.activePlaybackBuffer == playbackBuffer;
                menu.AddItem(label, isActive, () => playbackStream.SetPlaybackBuffer(playbackBuffer));
            }

            menu.ShowAsContext();
            Event.current.Use();
        }
        
        void BakeClipLoop()
        {
            var currentFrame = m_ClipBaker.currentFrame;
            var frameCount = m_ClipBaker.frameCount;
            if (m_ClipBaker.baking && currentFrame < frameCount)
            {
                var progress = currentFrame / (float)frameCount;
                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = GUILayoutUtility.GetRect(lastRect.width, k_ProgressBarHeight);
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(rect, progress, string.Format("Baking Frame {0} / {1}", currentFrame, frameCount));
                if (GUILayout.Button("Cancel"))
                    m_ClipBaker.StopBake();
                else
                    m_ClipBaker.BakeClipLoop();

                Repaint();
            }
            else if (Event.current.type == EventType.Repaint)
            {
                if (m_ClipBaker.baking)
                    m_ClipBaker.ApplyAnimationCurves();

                EditorUtility.ClearProgressBar();
                m_ClipBaker.StopBake();

                m_ClipBaker = null;
                GUIUtility.ExitGUI();
            }
        }
    }
}
