using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamPlayback))]
    public class StreamPlaybackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var streamPlayback = target as StreamPlayback;

            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                if (!streamPlayback.streamActive)
                {
                    if (GUILayout.Button("Start StreamPlayer"))
                        streamPlayback.ActivateStreamSource();
                }
                else
                {
                    if (GUILayout.Button("Stop StreamPlayer"))
                        streamPlayback.DeactivateStreamSource();
                }

                if (GUILayout.Button("Select Record Stream"))
                {
                    ShowRecordStreamMenu(streamPlayback, streamPlayback.playbackData.playbackBuffers);
                }

                using (new EditorGUI.DisabledGroupScope(!streamPlayback.streamActive))
                {
                    if (streamPlayback.playing)
                    {
                        if (GUILayout.Button("Stop Playback"))
                            streamPlayback.StopPlayBack();
                    }
                    else
                    {
                        if (GUILayout.Button("Start PlayBack"))
                            streamPlayback.StartPlayBack();
                    }
                }
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
                var buffer1 = buffer;
                var isActive = streamPlayback.activePlaybackBuffer == buffer1;
                menu.AddItem(label, isActive, () => streamPlayback.SetPlaybackBuffer(buffer1));
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

    }
}
