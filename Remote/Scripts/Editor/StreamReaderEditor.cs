using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamReader))]
    public class StreamReaderEditor : Editor
    {
        SerializedProperty m_StreamSettings;
        SerializedProperty m_PlaybackData;
        SerializedProperty m_Character;
        SerializedProperty m_UseDebug;
        SerializedProperty m_Port;
        SerializedProperty m_CatchupThreshold;
        SerializedProperty m_CatchupSize;
        SerializedProperty m_TrackingLossPadding;
        SerializedProperty m_BlendShapesControllerOverride;
        SerializedProperty m_CharacterRigControllerOverride;
        SerializedProperty m_HeadBoneOverride;
        SerializedProperty m_CameraOverride;

        ClipBaker m_ClipBaker;
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_Connect;

        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;

        void Awake()
        {
            m_PlayIcon = EditorGUIUtility.IconContent("d_Animation.Play");
            m_RecordIcon = EditorGUIUtility.IconContent("d_Animation.Record");
            m_Connect = EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small");
        }

        void OnEnable()
        {
            m_StreamSettings = serializedObject.FindProperty("m_StreamSettings");
            m_PlaybackData = serializedObject.FindProperty("m_PlaybackData");
            m_Character = serializedObject.FindProperty("m_Character");
            m_UseDebug = serializedObject.FindProperty("m_UseDebug");
            m_Port = serializedObject.FindProperty("m_Port");
            m_CatchupThreshold = serializedObject.FindProperty("m_CatchupThreshold");
            m_CatchupSize = serializedObject.FindProperty("m_CatchupSize");
            m_TrackingLossPadding = serializedObject.FindProperty("m_TrackingLossPadding");
            m_BlendShapesControllerOverride = serializedObject.FindProperty("m_BlendShapesControllerOverride");
            m_CharacterRigControllerOverride = serializedObject.FindProperty("m_CharacterRigControllerOverride");
            m_HeadBoneOverride = serializedObject.FindProperty("m_HeadBoneOverride");
            m_CameraOverride = serializedObject.FindProperty("m_CameraOverride");
        }

        public override void OnInspectorGUI()
        {
            SetupGUIStyles();

            var streamReader = target as StreamReader;
            if (streamReader == null)
                return;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_StreamSettings);
                EditorGUILayout.PropertyField(m_PlaybackData);
                EditorGUILayout.PropertyField(m_Character);
                EditorGUILayout.PropertyField(m_UseDebug);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_Port);
                EditorGUILayout.PropertyField(m_CatchupThreshold);
                EditorGUILayout.PropertyField(m_CatchupSize);
                EditorGUILayout.PropertyField(m_TrackingLossPadding);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Controller Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_BlendShapesControllerOverride);
                EditorGUILayout.PropertyField(m_CharacterRigControllerOverride);
                EditorGUILayout.PropertyField(m_HeadBoneOverride);
                EditorGUILayout.PropertyField(m_CameraOverride);
                EditorGUILayout.Space();

                if (check.changed)
                {
                    streamReader.InitializeStreamReader();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.LabelField("Remote", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    using (new EditorGUI.DisabledGroupScope(!(streamReader.server.deviceConnected || streamReader.server.streamActive)))
                    {
                        if (!streamReader.server.streamActive)
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonStyle))
                                streamReader.server.ActivateStreamSource();
                        }
                        else
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonPressStyle))
                                streamReader.server.DeactivateStreamSource();
                        }
                    }


                    using (new EditorGUI.DisabledGroupScope( !(streamReader.server.streamActive && streamReader.server.useRecorder) ))
                    {
                        if (streamReader.server.isRecording)
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                                streamReader.server.StopPlaybackDataUsage();
                        }
                        else
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                            streamReader.server.StartPlaybackDataUsage();

                        }
                    }

                    using (new EditorGUI.DisabledGroupScope(!(streamReader.server.streamActive || streamReader.streamPlayback.activePlaybackBuffer != null)))
                    {
                        if (streamReader.streamPlayback.playing)
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                            {
                                streamReader.streamPlayback.DeactivateStreamSource();
                                streamReader.streamPlayback.StopPlaybackDataUsage();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                            {
                                streamReader.streamPlayback.ActivateStreamSource();
                                streamReader.streamPlayback.StartPlaybackDataUsage();
                            }

                        }
                    }
                }
            }

            EditorGUILayout.Space();

            var clipName = streamReader.streamPlayback.activePlaybackBuffer == null ? "None" : streamReader.streamPlayback.activePlaybackBuffer.name;

            using (new EditorGUI.DisabledGroupScope(streamReader.streamPlayback == null))
            {
                if (GUILayout.Button(string.Format("Play Stream: {0}", clipName)))
                {
                    ShowRecordStreamMenu(streamReader.streamPlayback, streamReader.playbackData.playbackBuffers);
                }
            }

            EditorGUILayout.Space();

            // Bake Clip Button
            using (new EditorGUI.DisabledGroupScope(streamReader.streamPlayback.activePlaybackBuffer == null
                || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Bake Animation Clip"))
                {
                    streamReader.streamPlayback.DeactivateStreamSource();

                    // Used to initialize values if they were changed before baking.
                    streamReader.InitializeStreamReader();

                    var assetPath = Application.dataPath;
                    var path = EditorUtility.SaveFilePanel("Save stream as animation clip", assetPath, clipName + ".anim", "anim");

                    path = path.Replace(assetPath, "Assets");

                    if (path.Length != 0)
                    {
                        var blendShapeController = streamReader.blendShapesController;

                        var avatarController = streamReader.characterRigController;

                        streamReader.streamPlayback.ActivateStreamSource();
                        streamReader.streamPlayback.SetReaderStreamSettings();
                        streamReader.streamPlayback.StartPlaybackDataUsage();

                        var animClip = new AnimationClip();
                        m_ClipBaker = new ClipBaker(animClip, streamReader, streamReader.streamPlayback,
                            blendShapeController,  avatarController, path);
                    }
                }
            }


            if (m_ClipBaker != null)
                BakeClipLoop();

            // Want editor to update every frame
            EditorUtility.SetDirty(target);
        }

        void SetupGUIStyles()
        {
            if (m_ButtonStyle == null || m_ButtonPressStyle == null)
            {
                m_ButtonStyle = new GUIStyle("Button");
                m_ButtonPressStyle = new GUIStyle("Button");

                m_ButtonPressStyle.active = m_ButtonStyle.normal;
                m_ButtonPressStyle.normal = m_ButtonStyle.active;
                m_ButtonPressStyle.fixedHeight = 24;
                m_ButtonStyle.fixedHeight = 24;
            }
        }

        void BakeClipLoop()
        {
            if (m_ClipBaker.baking && m_ClipBaker.currentFrame < m_ClipBaker.frameCount)
            {
                var progress = m_ClipBaker.currentFrame / (float)m_ClipBaker.frameCount;
                if (EditorUtility.DisplayCancelableProgressBar("Animation Baking Progress",
                    "Progress in baking animation frames", progress))
                {
                    m_ClipBaker.StopBake();
                }
                else
                {
                    m_ClipBaker.BakeClipLoop();
                }
                Repaint();
            }
            else
            {
                if (m_ClipBaker.baking)
                    m_ClipBaker.ApplyAnimationCurves();

                EditorUtility.ClearProgressBar();
                m_ClipBaker.StopBake();

                m_ClipBaker = null;
                Repaint();
            }
        }

        static void ShowRecordStreamMenu(StreamPlayback streamPlayback, PlaybackBuffer[] buffers)
        {
            var menu = new GenericMenu();
            foreach (var buffer in buffers)
            {
                if (buffer.recordStream == null || buffer.recordStream.Length<1)
                    continue;

                var label = new GUIContent(buffer.name);
                var playbackBuffer = buffer;
                var isActive = streamPlayback.activePlaybackBuffer == playbackBuffer;
                menu.AddItem(label, isActive, () => streamPlayback.SetPlaybackBuffer(playbackBuffer));
            }
            menu.ShowAsContext();
            Event.current.Use();
        }
    }
}
