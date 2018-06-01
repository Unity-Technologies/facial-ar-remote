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
                    if (GUILayout.Button("Start Server"))
                        streamPlayback.ActivateStreamSource();
                }
                else
                {
                    if (GUILayout.Button("Stop Server"))
                        streamPlayback.DeactivateStreamSource();
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
    }
}
