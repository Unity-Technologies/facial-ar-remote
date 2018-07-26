using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamReader))]
    public class StreamReaderEditor : Editor
    {
        const int k_ProgressBarHeight = 22;

        SerializedProperty m_Character;
        SerializedProperty m_TrackingLossPadding;
        SerializedProperty m_VerboseLogging;
        SerializedProperty m_BlendShapesControllerOverride;
        SerializedProperty m_CharacterRigControllerOverride;
        SerializedProperty m_HeadBoneOverride;
        SerializedProperty m_CameraOverride;
        SerializedProperty m_StreamSourceOverrides;

        ClipBaker m_ClipBaker;
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_Connect;

        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;

        NetworkStream m_NetworkStream;
        PlaybackStream m_PlaybackStream;

        void Awake()
        {
            m_PlayIcon = EditorGUIUtility.IconContent("d_Animation.Play");
            m_RecordIcon = EditorGUIUtility.IconContent("d_Animation.Record");
            m_Connect = EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small");
        }

        void OnEnable()
        {
            m_Character = serializedObject.FindProperty("m_Character");
            m_VerboseLogging = serializedObject.FindProperty("m_VerboseLogging");
            m_TrackingLossPadding = serializedObject.FindProperty("m_TrackingLossPadding");
            m_BlendShapesControllerOverride = serializedObject.FindProperty("m_BlendShapesControllerOverride");
            m_CharacterRigControllerOverride = serializedObject.FindProperty("m_CharacterRigControllerOverride");
            m_HeadBoneOverride = serializedObject.FindProperty("m_HeadBoneOverride");
            m_CameraOverride = serializedObject.FindProperty("m_CameraOverride");
            m_StreamSourceOverrides = serializedObject.FindProperty("m_StreamSourceOverrides");

            var streamReader = (StreamReader)target;
            streamReader.ConnectDependencies();
            foreach (var source in streamReader.sources)
            {
                var network = source as NetworkStream;
                if (network != null)
                    m_NetworkStream = network;

                var playback = source as PlaybackStream;
                if (playback != null)
                    m_PlaybackStream = playback;
            }
        }

        public override void OnInspectorGUI()
        {
            SetupGUIStyles();

            var streamReader = (StreamReader)target;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_Character);
                EditorGUILayout.PropertyField(m_TrackingLossPadding);
                EditorGUILayout.PropertyField(m_VerboseLogging);
                EditorGUILayout.Space();

                if (streamReader.blendShapesController == null)
                {
                    EditorGUILayout.HelpBox("No Blend Shape Controller has been set or found. Note this data can " +
                        "still be recorded in the stream.", MessageType.Warning);
                }

                if (streamReader.characterRigController == null)
                {
                    EditorGUILayout.HelpBox("No Character Rig Controller has been set or found. Note this data can " +
                        "still be recorded in the stream.", MessageType.Warning);
                }

                if (streamReader.headBone == null)
                {
                    EditorGUILayout.HelpBox("No Head Bone Transform has been set or found. Note this data can still " +
                        "be recorded in the stream.", MessageType.Warning);
                }

                if (streamReader.cameraTransform == null)
                {
                    EditorGUILayout.HelpBox("No Camera has been set or found. Note this data can still be recorded " +
                        "in the stream.", MessageType.Warning);
                }

                EditorGUILayout.LabelField("Controller Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_BlendShapesControllerOverride);
                EditorGUILayout.PropertyField(m_CharacterRigControllerOverride);
                EditorGUILayout.PropertyField(m_HeadBoneOverride);
                EditorGUILayout.PropertyField(m_CameraOverride);
                EditorGUILayout.PropertyField(m_StreamSourceOverrides, true);

                if (m_NetworkStream == null)
                {
                    EditorGUILayout.HelpBox("No Network Stream Component has been set or found. You will be unable " +
                        "to connect to a device!", MessageType.Warning);
                }

                if (m_PlaybackStream == null)
                {
                    EditorGUILayout.HelpBox("No Playback Stream Component has been set or found. You Will be unable " +
                        "to Record, Playback, or Bake a Stream Data!", MessageType.Warning);
                }

                EditorGUILayout.Space();

                if (check.changed)
                {
                    streamReader.ConnectDependencies();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.LabelField("Remote", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || !m_NetworkStream.active))
                    {
                        var streamSource = streamReader.streamSource;
                        if (streamSource != null && streamSource.Equals(m_NetworkStream)
                            && m_NetworkStream != null && m_NetworkStream.active)
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonPressStyle))
                                streamReader.streamSource = null;
                        }
                        else
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonStyle))
                                streamReader.streamSource = m_NetworkStream;
                        }
                    }

                    var useRecorder = Application.isEditor && Application.isPlaying
                        && m_PlaybackStream != null && m_PlaybackStream.playbackData != null;
                    using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || !(m_NetworkStream.active && useRecorder)))
                    {
                        if (m_NetworkStream == null)
                        {
                            GUILayout.Button(m_RecordIcon, m_ButtonStyle);
                        }
                        else if (m_NetworkStream.recording)
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                                m_NetworkStream.StopRecording();
                        }
                        else
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                                m_NetworkStream.StartRecording();
                        }
                    }

                    using (new EditorGUI.DisabledGroupScope(m_NetworkStream == null || m_PlaybackStream == null
                        || !(m_NetworkStream.active || m_PlaybackStream.activePlaybackBuffer != null)))
                    {
                        if (m_PlaybackStream == null)
                        {
                            GUILayout.Button(m_PlayIcon, m_ButtonStyle);
                        }
                        else if (m_PlaybackStream.active)
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                            {
                                streamReader.streamSource = null;
                                m_PlaybackStream.StopPlayback();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                            {
                                streamReader.streamSource = m_PlaybackStream;
                                m_PlaybackStream.StartPlayback();
                            }
                        }
                    }
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
                        GUILayout.Button("Play Stream: NULL!");
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

            // Want editor to update every frame
            EditorUtility.SetDirty(target);
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
    }
}
