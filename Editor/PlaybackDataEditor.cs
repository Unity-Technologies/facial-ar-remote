using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class FloatCurveBinding
    {
        string m_PropertyName;
        AnimationCurve m_Curve;
        string m_Path;
        Type m_Type;

        public FloatCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyName = propertyName;
            m_Curve = new AnimationCurve();
        }

        public void AddKey(float time, float value)
        {
            m_Curve.AddKey(time, value);
        }

        public void SetCurves(AnimationClip clip)
        {
            clip.SetCurve(m_Path, m_Type, m_PropertyName, m_Curve);
        }
    }

    public class Vector3CurveBinding
    {
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public Vector3CurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[3]
            {
                propertyName + ".x",
                propertyName + ".y",
                propertyName + ".z"
            };
            m_Curves = new AnimationCurve[3];

            for (var i = 0; i < 3; ++i)
                m_Curves[i] = new AnimationCurve();
        }

        public void AddKey(float time, Vector3 value)
        {
            for (var i = 0; i < 3; ++i)
            {
                var curve = m_Curves[i];
                curve.AddKey(time, value[i]);
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < 3; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }

    public class QuaternionCurveBinding
    {
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public QuaternionCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[4]
            {
                propertyName + ".x",
                propertyName + ".y",
                propertyName + ".z",
                propertyName + ".w"
            };
            m_Curves = new AnimationCurve[4];

            for (var i = 0; i < 4; ++i)
                m_Curves[i] = new AnimationCurve();
        }

        public void AddKey(float time, Quaternion value)
        {
            for (var i = 0; i < 4; ++i)
            {
                var curve = m_Curves[i];
                curve.AddKey(time, value[i]);
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < 3; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }

    public class BlendShapesCurveBinding
    {
        const int kBlendShapeCount = 52;
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public BlendShapesCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[kBlendShapeCount];
            m_Curves = new AnimationCurve[kBlendShapeCount];

            for (var i = 0; i < kBlendShapeCount; ++i)
            {
                m_PropertyNames[i] = propertyName + "." + ((BlendShapeLocation)i).ToString();
                m_Curves[i] = new AnimationCurve();
            }
        }

        public void AddKey(float time, ref BlendShapeValues value)
        {
            for (var i = 0; i < kBlendShapeCount; ++i)
            {
                var curve = m_Curves[i];
                curve.AddKey(time, value[i]);
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < kBlendShapeCount; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }

    [CustomEditor(typeof(PlaybackData))]
    public class PlaybackDataEditor : Editor
    {
        PlaybackData m_PlaybackData;
        readonly string[] m_Empty = { "None" };
        string[] m_BufferNames = {};
        int m_BufferIndex = 0;

        void OnEnable()
        {
            m_PlaybackData = target as PlaybackData;
            m_BufferNames = Array.ConvertAll(m_PlaybackData.playbackBuffers, (b) => b.name);
        }

        public override void OnInspectorGUI()
        {
            var names = m_BufferNames.Length == 0 ? m_Empty : m_BufferNames;
            m_BufferIndex = EditorGUILayout.Popup(new GUIContent("Playback Buffer"), m_BufferIndex, names);

            if (GUILayout.Button("Bake"))
            {
                var playbackBuffer = m_PlaybackData.playbackBuffers[m_BufferIndex];
                var path = default(string);

                if (SaveFilePanel(playbackBuffer.name, out path))
                {
                    var clip = new AnimationClip();

                    Bake(playbackBuffer, clip);

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
        }

        void Bake(PlaybackBuffer playbackBuffer, AnimationClip clip)
        {
            var stream = playbackBuffer.recordStream;
            var bufferSize = playbackBuffer.bufferSize;

            Debug.Assert(stream.Length % bufferSize == 0, "PlaybackBuffer not compatible");

            var positionCurves = new Vector3CurveBinding("", typeof(Transform), "localPosition");
            var rotationCurves = new QuaternionCurveBinding("", typeof(Transform), "localRotation");
            var blendShapeCurves = new BlendShapesCurveBinding("", typeof(BlendShapesController), "m_BlendShapeValues");
            
            var buffer = new byte[bufferSize];
            for (var i = 0; i < stream.Length; i+=bufferSize)
            {
                Buffer.BlockCopy(stream, i, buffer, 0, bufferSize);
                var data = StreamBufferData.Create(buffer);
                positionCurves.AddKey(data.FrameTime, data.CameraPose.position);
                rotationCurves.AddKey(data.FrameTime, data.CameraPose.rotation);
                blendShapeCurves.AddKey(data.FrameTime, ref data.blendshapeValues);
            }

            positionCurves.SetCurves(clip);
            rotationCurves.SetCurves(clip);
            blendShapeCurves.SetCurves(clip);
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
