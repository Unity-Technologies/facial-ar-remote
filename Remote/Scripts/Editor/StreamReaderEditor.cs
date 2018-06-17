using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamReader))]
    public class StreamReaderEditor : Editor
    {
        ClipBaker m_ClipBaker;
        GUIContent m_PlayIcon;
        GUIContent m_RecordIcon;
        GUIContent m_Connect;

        bool m_Playing;
        bool m_Recording;

        GUIStyle m_ButtonStyle;
        GUIStyle m_ButtonPressStyle;

        void OnEnable()
        {
            m_PlayIcon = EditorGUIUtility.IconContent("d_Animation.Play");
            m_RecordIcon = EditorGUIUtility.IconContent("d_Animation.Record");
            m_Connect = EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small");
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

        public override void OnInspectorGUI()
        {
            SetupGUIStyles();

            base.OnInspectorGUI();
            var streamReader = target as StreamReader;

            if (streamReader == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Remote", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    using (new EditorGUI.DisabledGroupScope(!(!streamReader.server.isSource || streamReader.server.streamActive)))
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
            if (GUILayout.Button(string.Format("Play Stream: {0}", clipName)))
            {
                ShowRecordStreamMenu(streamReader.streamPlayback, streamReader.playbackData.playbackBuffers);
            }

            EditorGUILayout.Space();

            // Bake Clip Button
            using (new EditorGUI.DisabledGroupScope(streamReader.streamPlayback.activePlaybackBuffer == null
                || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Bake Animation Clip"))
                {
                    streamReader.streamPlayback.DeactivateStreamSource();

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

        void ShowRecordStreamMenu(StreamPlayback streamPlayback, PlaybackBuffer[] buffers)
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
