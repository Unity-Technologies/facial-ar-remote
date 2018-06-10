using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamReader))]
    public class StreamReaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var streamReader = target as StreamReader;

            if (streamReader == null)
                return;

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                if (!streamReader.server.streamActive)
                {
                    if (GUILayout.Button("Start Server"))
                        streamReader.server.ActivateStreamSource();
                }
                else
                {
                    if (GUILayout.Button("Stop Server"))
                        streamReader.server.DeactivateStreamSource();
                }
            }

            using (new EditorGUI.DisabledGroupScope(!streamReader.server.useRecorder))
            {
                if (streamReader.server.isRecording)
                {
                    if (GUILayout.Button("Stop Recording"))
                        streamReader.server.StopPlaybackDataUsage();
                }
                else
                {
                    if (GUILayout.Button("Start Recording"))
                        streamReader.server.StartPlaybackDataUsage();
                }
            }

            EditorGUILayout.Space();

             using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                if (!streamReader.streamPlayback.streamActive)
                {
                    if (GUILayout.Button("Start StreamPlayer"))
                        streamReader.streamPlayback.ActivateStreamSource();
                }
                else
                {
                    if (GUILayout.Button("Stop StreamPlayer"))
                        streamReader.streamPlayback.DeactivateStreamSource();
                }

                using (new EditorGUI.DisabledGroupScope(!streamReader.streamPlayback.streamActive))
                {
                    if (streamReader.streamPlayback.playing)
                    {
                        if (GUILayout.Button("Stop Playback"))
                            streamReader.streamPlayback.StopPlaybackDataUsage();
                    }
                    else
                    {
                        if (GUILayout.Button("Start PlayBack"))
                            streamReader.streamPlayback.StartPlaybackDataUsage();
                    }
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Select Record Stream"))
            {
                ShowRecordStreamMenu(streamReader.streamPlayback, streamReader.playbackData.playbackBuffers);
            }

            var clipName = streamReader.streamPlayback.activePlaybackBuffer == null ? string.Empty : streamReader.streamPlayback.activePlaybackBuffer.name;
            EditorGUILayout.LabelField(new GUIContent("Active Clip Name: "), new GUIContent(clipName));
            using (new EditorGUI.DisabledGroupScope(streamReader.streamPlayback.activePlaybackBuffer == null))
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
                        blendShapeController.Init();

                        var avatarController = streamReader.avatarController;
                        var animator = streamReader.animator;

                        streamReader.streamPlayback.ActivateStreamSource();
                        streamReader.streamPlayback.SetReaderStreamSettings();
                        streamReader.streamPlayback.StartPlaybackDataUsage();

                        var animClip = new AnimationClip();
                        m_ClipBaker = new ClipBaker(animClip, streamReader, streamReader.streamPlayback,
                            blendShapeController,  avatarController, animator, path);
                    }
                }
            }

            if (m_ClipBaker != null)
            {
                if (m_ClipBaker.baking && m_ClipBaker.currentFrame < m_ClipBaker.frameCount)
                {
                    var progress = m_ClipBaker.currentFrame / (float)m_ClipBaker.frameCount;
                    m_ClipBaker.animatorInitialized = streamReader.animator.isInitialized;
                    if (EditorUtility.DisplayCancelableProgressBar("Animation Baking Progress",
                        "Progress in baking animation frames", progress))
                    {
                        streamReader.avatarController.StopAnimatorSetup();
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
                    EditorUtility.ClearProgressBar();
                    if (m_ClipBaker.baking)
                        m_ClipBaker.ApplyAnimationCurves();
                    else
                        m_ClipBaker.StopBake();

                    m_ClipBaker = null;
                    Repaint();
                }
            }
        }

        ClipBaker m_ClipBaker;

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
