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

            EditorGUILayout.Space();

            if (GUILayout.Button("Select Record Stream"))
            {
                ShowRecordStreamMenu(streamPlayback, streamPlayback.playbackData.playbackBuffers);
            }

            var clipName = streamPlayback.activePlaybackBuffer == null ? string.Empty : streamPlayback.activePlaybackBuffer.name;
            EditorGUILayout.LabelField(new GUIContent("Active Clip Name: "), new GUIContent(clipName));
            using (new EditorGUI.DisabledGroupScope(streamPlayback.activePlaybackBuffer == null))
            {
                if (GUILayout.Button("Bake Animation Clip"))
                {
                    streamPlayback.DeactivateStreamSource();

                    var assetPath = Application.dataPath;
                    var path = EditorUtility.SaveFilePanel("Save stream as animation clip", assetPath, clipName + ".anim", "anim");

                    path = path.Replace(assetPath, "Assets");

                    if (path.Length != 0)
                    {
                        var blendShapeController = streamPlayback.blendShapesController;
                        blendShapeController.Init();

                        streamPlayback.ActivateStreamSource();
                        streamPlayback.RefreshStreamSettings();
                        streamPlayback.StartPlayBack();

                        var animClip = new AnimationClip();
                        var clipBaker = new ClipBaker(animClip, streamPlayback, blendShapeController);

                        // TODO needs to be animator transform
                        clipBaker.BakeClip(blendShapeController.transform);

                        // TODO stop saving over asset creating new guid
                        AssetDatabase.CreateAsset(animClip, path);

                        streamPlayback.DeactivateStreamSource();
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
