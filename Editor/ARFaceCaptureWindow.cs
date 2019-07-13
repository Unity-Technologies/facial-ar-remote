using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class ARFaceCaptureWindow : EditorWindow
    {
        private class Contents
        {
            public static string none = "None";
            public static GUIContent titleContent = new GUIContent("AR Face Capture");
            public static GUIContent addStreamReader = new GUIContent("Add a GameObject to the scene with a StreamReader Component or click the button to add a StreamReader prefab.");
            public static string playbackStreamMissing = "The StreamReader does not have a PlaybackStream assigned.";
            public static GUIContent playbackBuffer = new GUIContent("Playback Buffer");
            public static GUIContent createPlaybackData = new GUIContent("Create new PlaybackData Asset");
            public static GUIContent connect = new GUIContent("Connect");
            public static GUIContent play = new GUIContent("Play");
            public static GUIContent stop = new GUIContent("Stop");
            public static GUIContent record = new GUIContent("Record");
        }

        private class Styles
        {
            public GUIContent playIcon;
            public GUIContent recordIcon;
            public GUIContent connectIcon;
            public GUIStyle button;
            public GUIStyle buttonPress;
            public GUIStyle centeredLabel;

            public Styles()
            {
                playIcon = EditorGUIUtility.IconContent("d_Animation.Play");
                recordIcon = EditorGUIUtility.IconContent("d_Animation.Record");
                connectIcon = EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small");

                button = new GUIStyle("Button")
                {
                    fixedHeight = 24,
                    fixedWidth = 24
                };

                buttonPress = new GUIStyle("Button")
                {
                    active = button.normal,
                    normal = button.active,
                    fixedHeight = 24,
                    fixedWidth = 24
                };

                centeredLabel = GUI.skin.GetStyle("Label");
                centeredLabel.alignment = TextAnchor.UpperCenter;
                centeredLabel.wordWrap = true;
            }
        }

        Styles m_Styles;
        Styles styles 
        {
            get
            {
                if (m_Styles == null)
                    m_Styles = new Styles();

                return m_Styles;
            }
        }

        static readonly string[] s_Empty = { Contents.none };

        enum StreamSource
        {
            Device,
            File
        }
        
        const int k_ProgressBarHeight = 22;
        
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_StreamReaderPrefab;
#pragma warning restore CS0649

        Dictionary<StreamReader, StreamSource> m_StreamReaderModes = new Dictionary<StreamReader, StreamSource>();
        
        StreamReader[] m_StreamReaders = {};

        [MenuItem("Window/AR Face Capture")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(ARFaceCaptureWindow));
            window.titleContent = Contents.titleContent;
            window.minSize = new Vector2(300, 100);
        }

        void OnEnable()
        {
            Initialize();
            
            autoRepaintOnSceneChange = true;

            EditorApplication.hierarchyChanged -= Initialize;
            EditorApplication.hierarchyChanged += Initialize;
            AssemblyReloadEvents.afterAssemblyReload -= Initialize;
            AssemblyReloadEvents.afterAssemblyReload += Initialize;
            Undo.undoRedoPerformed -= Initialize;
            Undo.undoRedoPerformed += Initialize;
        }

        void OnDisable()
        {
            EditorApplication.hierarchyChanged -= Initialize;
            AssemblyReloadEvents.afterAssemblyReload -= Initialize;
            Undo.undoRedoPerformed -= Initialize;
        }

        void Initialize()
        {
            m_StreamReaders = FindObjectsOfType<StreamReader>();

            foreach (var streamReader in m_StreamReaders)
            {
                streamReader.ConnectDependencies();

                if (!m_StreamReaderModes.ContainsKey(streamReader))
                    m_StreamReaderModes.Add(streamReader, StreamSource.Device);
            }
        }

        void OnGUI()
        {
            if (m_StreamReaders.Length == 0)
                DoCreateStreamReaderGUI(m_StreamReaders);
            else
            {
                foreach (var streamReader in m_StreamReaders)
                {
                    if (streamReader != null)
                        DoStreamReaderGUI(streamReader);
                }
            }
        }

        void DoCreateStreamReaderGUI(StreamReader[] streamReaders)
        {
            Debug.Assert(streamReaders.Length == 0);

            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Contents.addStreamReader, styles.centeredLabel, GUILayout.MaxWidth(350));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add StreamReader Prefab"))
            {
                PrefabUtility.InstantiatePrefab(m_StreamReaderPrefab);
                streamReaders = new[] { FindObjectOfType<StreamReader>() };
                streamReaders[0].ConnectDependencies();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
        }

        void DoStreamReaderGUI(StreamReader streamReader)
        {
            Debug.Assert(streamReader != null);

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
                m_StreamReaderModes[streamReader] = (StreamSource)EditorGUILayout.EnumPopup(streamReader.name, m_StreamReaderModes[streamReader]);

                if (m_StreamReaderModes[streamReader] == StreamSource.File)
                {
                    if (playbackStream == null)
                    {
                        EditorGUILayout.HelpBox(Contents.playbackStreamMissing, MessageType.Warning);
                    }
                    else
                    {
                        DoPlaybackStreamGUI(playbackStream);
                    }
                }

                DoButtonsGUI(streamReader, networkStream, playbackStream);
            }
        }

        void DoPlaybackStreamGUI(PlaybackStream playbackStream)
        {
            Debug.Assert(playbackStream != null);

            if (playbackStream.playbackData == null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(Contents.playbackBuffer, GUILayout.Width(100));
                    
                    GUILayout.FlexibleSpace();
                    
                    if (GUILayout.Button(Contents.createPlaybackData))
                    {
                        var asset = CreateInstance<PlaybackData>();

                        AssetDatabase.CreateAsset(asset, "Assets/New Playback Data.asset");
                        AssetDatabase.SaveAssets();
                        playbackStream.playbackData = asset;
                    }
                }
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(playbackStream == null ||
                    playbackStream.playbackData == null ||
                    playbackStream.playbackData.playbackBuffers == null))
                {
                    var clipName = playbackStream == null || playbackStream.activePlaybackBuffer == null
                                    ? s_Empty[0]
                                    : playbackStream.activePlaybackBuffer.name;
                    var bufferCount = playbackStream.playbackData.Count;
                    var bufferNames = s_Empty;
                    var clipIndex = 0;

                    if (bufferCount > 0)
                    {
                        if (playbackStream.activePlaybackBuffer == null)
                            playbackStream.SetPlaybackBuffer(playbackStream.playbackData.playbackBuffers[0]);
                        
                        bufferNames = Array.ConvertAll(playbackStream.playbackData.playbackBuffers, b => b.name );
                        clipIndex = Array.IndexOf(bufferNames, clipName, 0);
                    }
                    else
                    {
                        playbackStream.SetPlaybackBuffer(null);
                    }
                    
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        clipIndex = EditorGUILayout.Popup(Contents.playbackBuffer, clipIndex, bufferNames);

                        if (change.changed)
                        {
                            var playbackBuffer = default(PlaybackBuffer);

                            if (bufferCount > 0 && clipIndex < bufferCount)
                                playbackBuffer = playbackStream.playbackData.playbackBuffers[clipIndex];
                            
                            playbackStream.SetPlaybackBuffer(playbackBuffer);
                        }
                    }
                }
            }
        }

        void DoButtonsGUI(StreamReader streamReader, NetworkStream networkStream, PlaybackStream playbackStream)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (m_StreamReaderModes[streamReader] == StreamSource.Device)
                {
                    using (new EditorGUI.DisabledGroupScope(networkStream == null || !networkStream.isActive))
                    {
                        var streamSource = streamReader.streamSource;
                        if (streamSource != null && streamSource.Equals(networkStream)
                            && networkStream != null && networkStream.isActive)
                        {
                            if (GUILayout.Button(Contents.connect))
                                streamReader.streamSource = null;
                        }
                        else
                        {
                            if (GUILayout.Button(Contents.connect))
                                streamReader.streamSource = networkStream;
                        }
                    }

                    var useRecorder = Application.isEditor && Application.isPlaying
                        && playbackStream != null && playbackStream.playbackData != null;
                    using (new EditorGUI.DisabledGroupScope(networkStream == null || !(networkStream.isActive && useRecorder)))
                    {
                        if (networkStream == null)
                        {

                        }
                        else if (networkStream.recording)
                        {
                            if (GUILayout.Button(Contents.record))
                                networkStream.StopRecording();
                        }
                        else
                        {
                            if (GUILayout.Button(Contents.record))
                                networkStream.StartRecording();
                        }
                    }
                }
                else if (m_StreamReaderModes[streamReader] == StreamSource.File)
                {
                    using (new EditorGUI.DisabledGroupScope(networkStream == null || playbackStream == null
                        || !(networkStream.isActive || playbackStream.activePlaybackBuffer != null)))
                    {
                        if (playbackStream == null)
                        {
                            GUILayout.Button(Contents.play);
                        }
                        else if (playbackStream.isActive)
                        {
                            if (GUILayout.Button(Contents.stop))
                            {
                                streamReader.streamSource = null;
                                playbackStream.StopPlayback();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(Contents.play))
                            {
                                streamReader.streamSource = playbackStream;
                                playbackStream.StartPlayback();
                            }
                        }
                    }
                }
            }
        }
    }
}
