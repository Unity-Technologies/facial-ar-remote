using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class ARFaceCaptureWindow : EditorWindow
    {
        static readonly string[] s_Empty = { "None" };

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
        
        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;
        
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_ConnectIcon;

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
            
            autoRepaintOnSceneChange = true;
        }

        void OnGUI()
        {
            SetupGUIStyles();
            
            var streamReaders = FindObjectsOfType<StreamReader>();
            
            if (streamReaders.Length == 0)
                DoCreateStreamReaderGUI(streamReaders);
            else
            {
                foreach (var streamReader in streamReaders)
                    DoStreamReaderGUI(streamReader);
            }
        }

        void DoCreateStreamReaderGUI(StreamReader[] streamReaders)
        {
            Debug.Assert(streamReaders.Length == 0);

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
        }

        void DoStreamReaderGUI(StreamReader streamReader)
        {
            if (!m_StreamReaderModes.ContainsKey(streamReader))
                m_StreamReaderModes.Add(streamReader, StreamSource.Device);
            
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
                    var bufferCount = playbackStream.playbackData.playbackBuffers.Length;
                    
                    if (bufferCount > 0)
                    {
                        if (playbackStream.activePlaybackBuffer == null)
                            playbackStream.SetPlaybackBuffer(playbackStream.playbackData.playbackBuffers[0]);
                    }
                    else
                    {
                        playbackStream.SetPlaybackBuffer(null);
                    }

                    var clipName = playbackStream == null || playbackStream.activePlaybackBuffer == null
                    ? s_Empty[0]
                    : playbackStream.activePlaybackBuffer.name;

                    using (new EditorGUI.DisabledGroupScope(playbackStream == null))
                    using (new GUILayout.HorizontalScope())
                    {
                        if (playbackStream == null)
                        {
                            EditorGUILayout.HelpBox("Playback Stream Componenet not found", MessageType.Warning);
                        }
                        else if (playbackStream.playbackData == null)
                        {
                            EditorGUILayout.LabelField("Playback Buffer", GUILayout.Width(100));
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
                                var bufferNames = s_Empty;
                                var clipIndex = 0;

                                if (bufferCount > 0)
                                {
                                    bufferNames = Array.ConvertAll(playbackStream.playbackData.playbackBuffers, b => b.name );
                                    clipIndex = Array.IndexOf(bufferNames, clipName, 0);
                                }
                                
                                using (var change = new EditorGUI.ChangeCheckScope())
                                {
                                    clipIndex = EditorGUILayout.Popup("Playback Buffer", clipIndex, bufferNames);

                                    if (change.changed)
                                    {
                                        var playbackBuffer = playbackStream.playbackData.playbackBuffers[clipIndex];
                                        playbackStream.SetPlaybackBuffer(playbackBuffer);
                                    }
                                }
                            }
                        }
                    }

                    if (playbackStream == null)
                    {
                        EditorGUILayout.HelpBox("The Stream Reader does not have a Playback" +
                            " Stream assigned.", MessageType.Warning);
                    }
                }

                DoButtonsGUI(streamReader, networkStream, playbackStream);
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
                            //if (GUILayout.Button(m_ConnectIcon, m_ButtonPressStyle))
                            if (GUILayout.Button("Connect"))
                                streamReader.streamSource = null;
                        }
                        else
                        {
                            //if (GUILayout.Button(m_ConnectIcon, m_ButtonStyle))
                            if (GUILayout.Button("Connect"))
                                streamReader.streamSource = networkStream;
                        }
                    }

                    var useRecorder = Application.isEditor && Application.isPlaying
                        && playbackStream != null && playbackStream.playbackData != null;
                    using (new EditorGUI.DisabledGroupScope(networkStream == null || !(networkStream.isActive && useRecorder)))
                    {
                        if (networkStream == null)
                        {
                            //GUILayout.Button(m_RecordIcon, m_ButtonStyle);
                        }
                        else if (networkStream.recording)
                        {
                            //if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                            if (GUILayout.Button("Record"))
                                networkStream.StopRecording();
                        }
                        else
                        {
                            //if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                            if (GUILayout.Button("Record"))
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
                            GUILayout.Button(m_PlayIcon, m_ButtonStyle);
                        }
                        else if (playbackStream.isActive)
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                            {
                                streamReader.streamSource = null;
                                playbackStream.StopPlayback();
                            }
                        }
                        else
                        {
                            //if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                            if (GUILayout.Button("Play"))
                            {
                                streamReader.streamSource = playbackStream;
                                playbackStream.StartPlayback();
                            }
                        }
                    }
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
    }
}
