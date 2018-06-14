using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Labs.FacialRemote
{
    public class ClipBaker
    {
        const string k_BlendShapeProp = "blendShape.{0}";
        static readonly string[] s_RotParams =
        {
            "localRotation.x",
            "localRotation.y",
            "localRotation.z",
            "localRotation.w"
        };


        AnimationClip m_Clip;

        StreamReader m_StreamReader;

        StreamPlayback m_StreamPlayback;

        CharacterRigController m_CharacterRigController;

        BlendShapesController m_BlendShapesController;

        Dictionary<Object, Dictionary<string, AnimationClipCurveData>> m_AnimationCurves =
            new Dictionary<Object, Dictionary<string, AnimationClipCurveData>>();

        List<AnimationClipCurveData> m_AnimationClipCurveData = new List<AnimationClipCurveData>();

        float[] m_FrameTimes = new float[2];

        public int currentFrame { get { return m_CurrentFrame; } }
        public int frameCount { get { return m_FrameCount; } }
        public bool baking { get { return m_Baking; } }

        string m_FilePath;

        int m_CurrentFrame;
        IStreamSettings m_StreamSettings;
        int m_FrameCount;

        bool m_Baking;

        public ClipBaker(AnimationClip clip, StreamReader streamReader, StreamPlayback streamPlayback,
            BlendShapesController blendShapesController, CharacterRigController characterRigController, string filePath)
        {
            m_Clip = clip;
            m_StreamReader = streamReader;
            m_StreamPlayback = streamPlayback;
            m_BlendShapesController = blendShapesController;
            m_CharacterRigController = characterRigController;
            m_FilePath = filePath;

            StartClipBaker(m_BlendShapesController.transform);
        }

        void StartClipBaker(Transform transform)
        {
            m_AnimationClipCurveData.Clear();
            m_StreamPlayback.SetPlaybackBuffer(m_StreamPlayback.activePlaybackBuffer); // TODO needed?
            m_StreamReader.SetStreamSource(m_StreamPlayback);

            m_BlendShapesController.SetupBlendShapeIndices();
            m_CharacterRigController.SetupBlendShapeIndices();

            m_CharacterRigController.SetupCharacterRigController();

            m_Baking = true;

            // Get curve data for blend shapes
            foreach (var skinnedMeshRenderer in m_BlendShapesController.skinnedMeshRenderers)
            {
                var path = AnimationUtility.CalculateTransformPath(skinnedMeshRenderer.transform, transform);
                path = path.Replace(string.Format("{0}/", skinnedMeshRenderer.transform), "");

                var mesh = skinnedMeshRenderer.sharedMesh;
                var count = mesh.blendShapeCount;

                var animationCurves = new Dictionary<string, AnimationClipCurveData>();
                for (var i = 0; i < count; i++)
                {
                    var shapeName = mesh.GetBlendShapeName(i);
                    var prop = string.Format(k_BlendShapeProp, shapeName);
                    var curve = new AnimationCurve();
                    var curveData = new AnimationClipCurveData
                    {
                        path = path,
                        curve = curve,
                        propertyName = prop,
                        type = skinnedMeshRenderer.GetType()
                    };
                    m_AnimationClipCurveData.Add(curveData);
                    animationCurves.Add(prop, curveData);
                }

                m_AnimationCurves.Add(skinnedMeshRenderer, animationCurves);
            }

            // Get curve data for bone transforms
            var transformType = typeof(Transform);
            foreach (var bone in m_CharacterRigController.animatedBones)
            {
                var path = AnimationUtility.CalculateTransformPath(bone, transform);
                var animationCurves = new Dictionary<string, AnimationClipCurveData>();
                foreach (var prop in s_RotParams)
                {
                    var curve = new AnimationCurve();
                    var curveData = new AnimationClipCurveData
                    {
                        path = path,
                        curve = curve,
                        propertyName = prop,
                        type = transformType
                    };
                    m_AnimationClipCurveData.Add(curveData);
                    animationCurves.Add(prop, curveData);
                }
                m_AnimationCurves.Add(bone, animationCurves);
            }

            m_StreamSettings = m_StreamPlayback.activePlaybackBuffer;
            if (m_StreamSettings == null)
            {
                return;
            }

            m_CurrentFrame = 0;
            m_FrameCount = m_StreamPlayback.activePlaybackBuffer.recordStream.Length / m_StreamSettings.BufferSize;
        }


        public void StopBake()
        {
            m_Baking = false;
            m_CharacterRigController.ResetBonePoses();
        }

        public void ApplyAnimationCurves()
        {
            foreach (var curveData in m_AnimationClipCurveData)
            {
                m_Clip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);
                AnimationUtility.SetEditorCurve(m_Clip, EditorCurveBinding.FloatCurve(curveData.path, curveData.type,
                    curveData.propertyName), curveData.curve);
            }
            var fileClip = AssetDatabase.LoadAssetAtPath(m_FilePath, typeof(AnimationClip));
            if (fileClip == null)
            {
                AssetDatabase.CreateAsset(m_Clip, m_FilePath);
            }
            else
            {
                fileClip = m_Clip;
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(m_FilePath);
            }

            StopBake();
            Debug.Log("End Bake");
        }

        bool m_AnimatorInitialized;
        public bool animatorInitialized { get { return m_AnimatorInitialized; }  set { m_AnimatorInitialized = value; } }

        public bool avatarSettingUp;

        public void BakeClipLoop()
        {
            m_Baking = BakeClipLoopInternal();
            if (!m_Baking)
                StopBake();
        }

        bool BakeClipLoopInternal()
        {
            while (m_CurrentFrame <= frameCount)
            {
                if (m_CurrentFrame == 0)
                {
                    var startFrameBuffer = new byte[m_StreamSettings.BufferSize];
                    Buffer.BlockCopy(m_StreamPlayback.activePlaybackBuffer.recordStream, 0, startFrameBuffer, 0,
                        m_StreamSettings.BufferSize);
                    Buffer.BlockCopy(startFrameBuffer, m_StreamSettings.FrameTimeOffset, m_FrameTimes, 0, sizeof(float));
                    Thread.Sleep(1);
                }

                if (m_CurrentFrame < frameCount)
                {
                    m_StreamPlayback.PlayBackLoop(true);
                    m_StreamPlayback.UpdateCurrentFrameBuffer(true);
                    m_BlendShapesController.InterpolateBlendShapes(true);
                    m_CharacterRigController.InterpolateBlendShapes(true);

                    // Get next key frame time
                    Buffer.BlockCopy(m_StreamPlayback.currentFrameBuffer, m_StreamSettings.FrameTimeOffset, m_FrameTimes,
                        sizeof(float), sizeof(float));
                    KeyFrame(m_FrameTimes[1] - m_FrameTimes[0]);

                    m_CurrentFrame++;
                }

                if (m_CurrentFrame == frameCount)
                {
                    m_CurrentFrame++;
                }

                return true;
            }
            return false;
        }

        void KeyFrame(float time)
        {
            // Key blend shapes
            foreach (var skinnedMeshRenderer in m_BlendShapesController.skinnedMeshRenderers)
            {
                Dictionary<string, AnimationClipCurveData> animationCurves;
                if (m_AnimationCurves.TryGetValue(skinnedMeshRenderer, out animationCurves))
                {
                    BlendShapeIndexData[] shapeIndices;
                    if (m_BlendShapesController.blendShapeIndices.TryGetValue(skinnedMeshRenderer, out shapeIndices))
                    {
                        var length = shapeIndices.Length;
                        for (var i = 0; i < length; i++)
                        {
                            var datum = shapeIndices[i];
                            if (datum.index < 0)
                                continue;

                            var curve = animationCurves[string.Format(k_BlendShapeProp, datum.name)].curve;
                            curve.AddKey(time, m_BlendShapesController.blendShapesScaled[datum.index]);
                        }
                    }
                }
            }

            // Key bone rotation
            foreach (var bone in m_CharacterRigController.animatedBones)
            {
                Dictionary<string, AnimationClipCurveData> animationCurves;
                if (m_AnimationCurves.TryGetValue(bone, out animationCurves))
                {
                    foreach (var datum in animationCurves)
                    {
                        var prop = datum.Value.propertyName;
                        var curve = datum.Value.curve;
                        if (prop == s_RotParams[0])
                        {
                            curve.AddKey(time, bone.localRotation.x);
                        }
                        else if (prop == s_RotParams[1])
                        {
                            curve.AddKey(time, bone.localRotation.y);
                        }
                        else if (prop == s_RotParams[2])
                        {
                            curve.AddKey(time, bone.localRotation.z);
                        }
                        else if (prop == s_RotParams[3])
                        {
                            curve.AddKey(time, bone.localRotation.w);
                        }
                        else
                        {
                            Debug.LogErrorFormat("Fell through on {0} : {1}", datum.Key, prop );
                        }
                    }
                }
            }
        }

    }
}
