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
        
        [SerializeField]
        GameObject m_StreamReaderPrefab;

        StreamMode m_StreamMode = StreamMode.StreamFromDevice;
        
        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;
        
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_ConnectIcon;
        
        Dictionary<IStreamReader, NetworkStream> m_NetworkStreams = new Dictionary<IStreamReader, NetworkStream>();
        Dictionary<IStreamReader, PlaybackStream> m_PlaybackStreams = new Dictionary<IStreamReader, PlaybackStream>();

        ClipBaker m_ClipBaker;

        PlaybackData m_PlaybackData;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/AR Face Capture")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(ARFaceCaptureWindow));
            window.titleContent = new GUIContent("AR Face Capture");
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
                GUILayout.Label("Add a game object to the scene with a StreamReader component or click to create one.");
                
                if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                {
                    PrefabUtility.InstantiatePrefab(m_StreamReaderPrefab);
                    streamReaders = new[] { FindObjectOfType<StreamReader>() };
                }

                return;
            }
            
            // TODO Handle if there are too many of one type of source
            foreach (var streamReader in streamReaders)
            {
                foreach (var source in streamReader.sources)
                {
                    var network = source as NetworkStream;
                    if (network != null)
                    {
                        if (m_NetworkStreams.ContainsKey(streamReader))
                        {
                            m_NetworkStreams.TryGetValue(streamReader, out var ns);
                            if (ns != network)
                            {
                                m_NetworkStreams[streamReader] = network;
                            }
                        }
                        else
                        {
                            m_NetworkStreams.Add(streamReader, network);
                        }
                    }

                    var playback = source as PlaybackStream;
                    if (playback != null)
                    {
                        if (m_PlaybackStreams.ContainsKey(streamReader))
                        {
                            m_PlaybackStreams.TryGetValue(streamReader, out var pb);
                            if (pb != playback)
                            {
                                m_PlaybackStreams[streamReader] = playback;
                            }
                        }
                        else
                        {
                            m_PlaybackStreams.Add(streamReader, playback);
                        }
                    }
                }

                // Remove deleted Stream Readers from the dictionaries
                if (!m_NetworkStreams.ContainsKey(streamReader))
                {
                    m_NetworkStreams.Remove(streamReader);
                }
                
                if (!m_PlaybackStreams.ContainsKey(streamReader))
                {
                    m_PlaybackStreams.Remove(streamReader);
                }
            }
            
            foreach (var streamReader in streamReaders)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(streamReader.name);
                    if (streamReader.blendShapesController != null)
                    {
                        EditorGUILayout.LabelField(streamReader.blendShapesController.name);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Blend Shape Controller");
                    }
                    if (streamReader.characterRigController != null)
                    {
                        EditorGUILayout.LabelField(streamReader.characterRigController.name);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Character Rig Controller");
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    m_StreamMode = (StreamMode)EditorGUILayout.EnumPopup("", m_StreamMode);

                    if (m_StreamMode == StreamMode.StreamFromDevice)
                    {
                        if (GUILayout.Button(m_ConnectIcon, m_ButtonStyle))
                        {
                            using (new EditorGUI.DisabledGroupScope(!m_NetworkStreams.ContainsKey(streamReader) 
                                || m_NetworkStreams[streamReader] == null || !m_NetworkStreams[streamReader].active))
                            {
                                var streamSource = streamReader.streamSource;
                                if (streamSource != null && streamSource.Equals(m_NetworkStream)
                                    && m_NetworkStream != null && m_NetworkStream.active)
                                {
                                    if (GUILayout.Button(m_ConnectIcon, m_ButtonPressStyle))
                                        streamReader.streamSource = null;
                                }
                                else
                                {
                                    if (GUILayout.Button(m_ConnectIcon, m_ButtonStyle))
                                        streamReader.streamSource = m_NetworkStreams[streamReader];
                                }
                            }
                        }
                        
                        if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                        {
                            
                        }
                    }
                    else if (m_StreamMode == StreamMode.StreamFromFile)
                    {
                        if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                        {
                            
                        }
                        
                        if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                        {
                            
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Stream Recorder");
                    
                    //m_PlaybackData = (PlaybackStream)EditorGUILayout.ObjectField(m_PlaybackData, typeof(PlaybackData));
                    
                    if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                    {
                        PrefabUtility.InstantiatePrefab(m_StreamReaderPrefab);
                        streamReaders = new[] { FindObjectOfType<StreamReader>() };
                    }
                }
            }


            return;
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
            
            
            if (m_NetworkStreams.Count == 0)
            {
                EditorGUILayout.HelpBox("No Network Stream Component has been set or found. You will be unable " +
                    "to connect to a device!", MessageType.Warning);
            }

            if (m_PlaybackStreams.Count == 0)
            {
                EditorGUILayout.HelpBox("No Playback Stream Component has been set or found. You Will be unable " +
                    "to Record, Playback, or Bake a Stream Data!", MessageType.Warning);
            }

            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || !m_NetworkStream.active))
                    {
                        foreach (var sr in streamReaders)
                        {
                            var streamSource = sr.streamSource;
                            if (streamSource != null && streamSource.Equals(m_NetworkStream)
                                && m_NetworkStream != null && m_NetworkStream.active)
                            {
                                if (GUILayout.Button(m_Connect, m_ButtonPressStyle))
                                    sr.streamSource = null;
                            }
                            else
                            {
                                if (GUILayout.Button(m_Connect, m_ButtonStyle))
                                    sr.streamSource = m_NetworkStreams[sr];
                            }
                        }
                    }

                    var useRecorder = Application.isEditor && Application.isPlaying
                        && m_PlaybackStream != null && m_PlaybackStream.playbackData != null;
                    using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || !(m_NetworkStream.active && useRecorder)))
                    {
                        foreach (var sr in streamReaders)
                        {
                            NetworkStream ns;
                            m_NetworkStreams.TryGetValue(sr, out ns);
                            if (ns == null)
                            {
                                GUILayout.Button(m_RecordIcon, m_ButtonStyle);
                            }
                            else if (ns.recording)
                            {
                                if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                                    ns.StopRecording();
                            }
                            else
                            {
                                if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                                    ns.StartRecording();
                            }
                        }
                    }

                    //using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || m_PlaybackStream == null
                    //    || !(m_NetworkStream.active || m_PlaybackStream.activePlaybackBuffer != null)))
                    //{
                        if (m_PlaybackStream == null)
                        {
                            GUILayout.Button(m_PlayIcon, m_ButtonStyle);
                        }
                        else if (m_PlaybackStream.active)
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                            {
                                foreach (var sr in streamReaders)
                                {
                                    sr.streamSource = null;
                                }
                                m_PlaybackStream.StopPlayback();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                            {
                                foreach (var sr in streamReaders)
                                {
                                    sr.streamSource = m_PlaybackStream;
                                }                                
                                m_PlaybackStream.StartPlayback();
                            }
                        }
                    //}
                }
            }

            EditorGUILayout.Space();

            if (m_ClipBaker == null)
            {
                var clipName = m_PlaybackStream == null || m_PlaybackStream.activePlaybackBuffer == null ? "None"
                    : m_PlaybackStream.activePlaybackBuffer.name;

                using (new EditorGUI.DisabledGroupScope(m_PlaybackStream == null))
                {
                    if (m_PlaybackStream == null || m_PlaybackStream.playbackData == null)
                    {
                        if (GUILayout.Button("Create new Playback Data asset"))
                        {
                            var asset = CreateInstance<PlaybackData>();

                            AssetDatabase.CreateAsset(asset, "Assets/New Playback Data.asset");
                            AssetDatabase.SaveAssets();
                            m_PlaybackStream.playbackData = asset;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(string.Format("Play Stream: {0}", clipName)))
                            ShowRecordStreamMenu(m_PlaybackStream, m_PlaybackStream.playbackData.playbackBuffers);
                    }
                }

                EditorGUILayout.Space();

                // Bake Clip Button
                using (new EditorGUI.DisabledGroupScope(m_PlaybackStream == null || m_PlaybackStream.activePlaybackBuffer == null
                    || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (GUILayout.Button("Bake Animation Clip"))
                    {
                        m_Strea
                        streamReader.streamSource = null;

                        // Used to initialize values if they were changed before baking.
                        streamReader.ConnectDependencies();

                        var assetPath = Application.dataPath;
                        var path = EditorUtility.SaveFilePanel("Save stream as animation clip", assetPath, clipName + ".anim", "anim");

                        path = path.Replace(assetPath, "Assets");

                        if (path.Length != 0)
                        {
                            var blendShapeController = streamReader.blendShapesController;

                            var avatarController = streamReader.characterRigController;

                            streamReader.streamSource = m_PlaybackStream;
                            m_PlaybackStream.StartPlayback();

                            var animClip = new AnimationClip();
                            m_ClipBaker = new ClipBaker(animClip, streamReader, m_PlaybackStream,
                                blendShapeController, avatarController, path);
                        }
                    }
                }
            }
            else
            {
                BakeClipLoop();
            }
            
            EditorGUILayout.LabelField("f");
            */

        }
        
        void SetupGUIStyles()
        {
            if (m_ButtonStyle == null || m_ButtonPressStyle == null)
            {
                m_ButtonStyle = new GUIStyle("Button")
                {
                    fixedHeight = 24
                };

                m_ButtonPressStyle = new GUIStyle("Button")
                {
                    active = m_ButtonStyle.normal,
                    normal = m_ButtonStyle.active,
                    fixedHeight = 24
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
