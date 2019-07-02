using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(PlaybackData))]
    public class PlaybackDataEditor : Editor
    {
        public enum CaptureType
        {
            CameraPose,
            FacialRig
        }

        PlaybackData m_PlaybackData;
        static readonly string[] s_Empty = { "None" };
        static readonly int[] s_SampleRates = { 24, 25, 30, 50, 60 };
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

                        Bake(playbackBuffer, m_CaptureType, s_SampleRates[m_SampleRateIndex], clip);

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

        void Bake(PlaybackBuffer playbackBuffer, CaptureType captureType, int sampleRate, AnimationClip clip)
        {
            var stream = playbackBuffer.recordStream;
            var bufferSize = playbackBuffer.bufferSize;

            if (stream.Length < bufferSize || stream.Length % bufferSize != 0)
                throw new Exception("Invalid PlaybackBuffer");

            if (Array.IndexOf(s_SampleRates, sampleRate) == -1)
                throw new Exception("Invalid SampleRate");

            var positionCurves = new Vector3CurveBinding("", typeof(Transform), "localPosition");
            var rotationCurves = new QuaternionCurveBinding("", typeof(Transform), "localRotation");
            var blendShapeCurves = new BlendShapesCurveBinding("", typeof(BlendShapesController), "m_BlendShapeValues");
            var headBonePositionCurves = new Vector3CurveBinding("", typeof(CharacterRigController), "m_HeadPose.position");
            var headBoneRotationCurves = new QuaternionCurveBinding("", typeof(CharacterRigController), "m_HeadPose.rotation");
            var cameraPositionCurves = new Vector3CurveBinding("", typeof(CharacterRigController), "m_CameraPose.position");
            var cameraRotationCurves = new QuaternionCurveBinding("", typeof(CharacterRigController), "m_CameraPose.rotation");
            var buffer = new byte[bufferSize];
            
            Buffer.BlockCopy(stream, 0, buffer, 0, bufferSize);

            var timeStep = 1.0 / (double)sampleRate;
            var timeAcc = 0.0;
            var lastData = StreamBufferData.Create(buffer);
            var startTime = lastData.FrameTime;
            var lastFrameTime = 0.0;

            for (var i = 0; i < stream.Length; i+=bufferSize)
            {
                Buffer.BlockCopy(stream, i, buffer, 0, bufferSize);
                var data = StreamBufferData.Create(buffer);
                var time = data.FrameTime - startTime;
                var lastTime = lastData.FrameTime - startTime;

                if (i == 0)
                {
                    if (captureType == CaptureType.CameraPose)
                    {
                        positionCurves.AddKey(time, data.CameraPosition);
                        rotationCurves.AddKey(time, data.CameraRotation);
                    }
                    else
                    {
                        blendShapeCurves.AddKey(time, ref data.BlendshapeValues);
                        headBonePositionCurves.AddKey(time, data.HeadPosition);
                        headBoneRotationCurves.AddKey(time, data.HeadRotation);
                        cameraPositionCurves.AddKey(time, data.CameraPosition);
                        cameraRotationCurves.AddKey(time, data.CameraRotation);
                    }
                }
                else
                {
                    var deltaTime = time - lastTime;
                    timeAcc += deltaTime;

                    while (timeAcc >= timeStep)
                    {
                        var frameTime = lastFrameTime + timeStep;
                        var t = (float)((frameTime -  lastTime) / deltaTime);
                        var headPosition = Vector3.Lerp(lastData.HeadPosition, data.HeadPosition, t);
                        var headRotation = Quaternion.Slerp(lastData.HeadRotation, data.HeadRotation, t);
                        var cameraPosition = Vector3.Lerp(lastData.CameraPosition, data.CameraPosition, t);
                        var cameraRotation = Quaternion.Slerp(lastData.CameraRotation, data.CameraRotation, t);
                        var blendShapeValues = BlendShapeValues.Lerp(ref lastData.BlendshapeValues, ref data.BlendshapeValues, t);
                        
                        timeAcc -= timeStep;
                        lastFrameTime = frameTime;

                        if (captureType == CaptureType.CameraPose)
                        {
                            positionCurves.AddKey((float)frameTime, cameraPosition);
                            rotationCurves.AddKey((float)frameTime, cameraRotation);
                        }
                        else
                        {
                            blendShapeCurves.AddKey((float)frameTime, ref blendShapeValues);
                            headBonePositionCurves.AddKey((float)frameTime, headPosition);
                            headBoneRotationCurves.AddKey((float)frameTime, headRotation);
                            cameraPositionCurves.AddKey((float)frameTime, cameraPosition);
                            cameraRotationCurves.AddKey((float)frameTime, cameraRotation);
                        }
                    }
                }

                lastData = data;
            }

            if (captureType == CaptureType.CameraPose)
            {
                positionCurves.SetCurves(clip);
                rotationCurves.SetCurves(clip);
            }
            else
            {
                blendShapeCurves.SetCurves(clip);
                headBonePositionCurves.SetCurves(clip);
                headBoneRotationCurves.SetCurves(clip);
                cameraPositionCurves.SetCurves(clip);
                cameraRotationCurves.SetCurves(clip);
            }
            
            clip.frameRate = sampleRate;
        }
    }
}
