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
        SerializedProperty m_UseDebug;
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
            m_Character = serializedObject.FindProperty("m_Character");
            m_UseDebug = serializedObject.FindProperty("m_UseDebug");
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
                EditorGUILayout.PropertyField(m_Character);
                EditorGUILayout.PropertyField(m_TrackingLossPadding);
                EditorGUILayout.PropertyField(m_UseDebug);
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

            var streamPlayback = streamReader.streamPlayback;
            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    var server = streamReader.server;
                    using (new EditorGUI.DisabledGroupScope(server == null || !server.active))
                    {
                        var streamSource = streamReader.streamSource;
                        if (streamSource != null && streamSource.Equals(server) && server.active)
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonPressStyle))
                                streamReader.streamSource = null;

                        }
                        else
                        {
                            if (GUILayout.Button(m_Connect, m_ButtonStyle))
                                streamReader.streamSource = server;
                        }
                    }

                    var useRecorder = Application.isEditor && Application.isPlaying
                        && streamPlayback != null && streamPlayback.playbackData != null;
                    using (new EditorGUI.DisabledGroupScope(server == null || !(server.active && useRecorder)))
                    {
                        if (server != null && server.recording)
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonPressStyle))
                                server.StopRecording();
                        }
                        else
                        {
                            if (GUILayout.Button(m_RecordIcon, m_ButtonStyle))
                                server.StartRecording();

                        }
                    }

                    using (new EditorGUI.DisabledGroupScope(server == null || streamPlayback == null
                        || !(server.active || streamPlayback.activePlaybackBuffer != null)))
                    {
                        if (streamPlayback != null && streamPlayback.active)
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonPressStyle))
                            {
                                streamReader.streamSource = null;
                                streamPlayback.StopPlayback();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(m_PlayIcon, m_ButtonStyle))
                            {
                                streamReader.streamSource = streamPlayback;
                                streamPlayback.StartPlayback();
                            }

                        }
                    }
                }
            }

            EditorGUILayout.Space();

            if (m_ClipBaker == null)
            {
                var clipName = streamPlayback == null || streamPlayback.activePlaybackBuffer == null ? "None" : streamPlayback.activePlaybackBuffer.name;

                using (new EditorGUI.DisabledGroupScope(streamPlayback == null))
                {
                    if (GUILayout.Button(string.Format("Play Stream: {0}", clipName)))
                    {
                        ShowRecordStreamMenu(streamPlayback, streamReader.playbackData.playbackBuffers);
                    }
                }

                EditorGUILayout.Space();

                // Bake Clip Button
                using (new EditorGUI.DisabledGroupScope(streamPlayback == null || streamPlayback.activePlaybackBuffer == null
                    || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (GUILayout.Button("Bake Animation Clip"))
                    {
                        streamReader.streamSource = null;

                        // Used to initialize values if they were changed before baking.
                        streamReader.InitializeStreamReader();

                        var assetPath = Application.dataPath;
                        var path = EditorUtility.SaveFilePanel("Save stream as animation clip", assetPath, clipName + ".anim", "anim");

                        path = path.Replace(assetPath, "Assets");

                        if (path.Length != 0)
                        {
                            var blendShapeController = streamReader.blendShapesController;

                            var avatarController = streamReader.characterRigController;

                            streamReader.streamSource = streamPlayback;
                            streamPlayback.StartPlayback();

                            var animClip = new AnimationClip();
                            m_ClipBaker = new ClipBaker(animClip, streamReader, streamPlayback,
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
            if (m_ClipBaker.baking && m_ClipBaker.currentFrame < m_ClipBaker.frameCount)
            {
                var progress = m_ClipBaker.currentFrame / (float)m_ClipBaker.frameCount;
                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = GUILayoutUtility.GetRect(lastRect.width, k_ProgressBarHeight);
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(rect, progress, "Baking...");
                // if (EditorUtility.DisplayCancelableProgressBar("Animation Baking Progress",
                //     "Progress in baking animation frames", progress))
                if (GUILayout.Button("Cancel"))
                {
                    m_ClipBaker.StopBake();
                }
                else
                {
                    m_ClipBaker.BakeClipLoop();
                }

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
