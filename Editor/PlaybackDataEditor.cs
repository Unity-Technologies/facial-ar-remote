using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(PlaybackData))]
    public class PlaybackDataEditor : Editor
    {
        PlaybackData m_PlaybackData;
        static readonly string[] s_Empty = { "None" };
        string[] s_SampleRateNames = { "24", "25", "30", "50", "60"};
        string[] m_BufferNames = {};
        int m_BufferIndex = 0;
        int m_SampleRateIndex = 0;
        CaptureType m_CaptureType;

        void OnEnable()
        {
            m_PlaybackData = target as PlaybackData;
            m_BufferNames = Array.ConvertAll(m_PlaybackData.playbackBuffers, (b) => b.name);
        }

        public override void OnInspectorGUI()
        {
            var names = m_BufferNames.Length == 0 ? s_Empty : m_BufferNames;
            m_BufferIndex = EditorGUILayout.Popup(new GUIContent("Playback Buffer"), m_BufferIndex, names);
            m_CaptureType = (CaptureType)EditorGUILayout.EnumPopup(new GUIContent("Capture Type"), m_CaptureType);
            m_SampleRateIndex = EditorGUILayout.Popup(new GUIContent("Sample Rate"), m_SampleRateIndex, s_SampleRateNames);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Bake Animation Clip"))
                {
                    var playbackBuffer = m_PlaybackData.playbackBuffers[m_BufferIndex];
                    var path = default(string);

                    if (SaveFilePanel(playbackBuffer.name, out path))
                    {
                        var clip = new AnimationClip();

                        BakeUtility.Bake(playbackBuffer, m_CaptureType, BakeUtility.s_SampleRates[m_SampleRateIndex], clip);

                        var fileClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                        if (fileClip == null)
                            AssetDatabase.CreateAsset(clip, path);
                        else
                        {
                            EditorUtility.CopySerialized(clip, fileClip);
                            DestroyImmediate(clip);
                        }

                        AssetDatabase.ImportAsset(path);
                        AssetDatabase.SaveAssets();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        bool SaveFilePanel(string name, out string path)
        {
            var assetPath = Application.dataPath;
            path = EditorUtility.SaveFilePanel("Save stream as animation clip", assetPath, name + ".anim", "anim");
            path = path.Replace(assetPath, "Assets");
            return path.Length > 0;
        }
    }
}
