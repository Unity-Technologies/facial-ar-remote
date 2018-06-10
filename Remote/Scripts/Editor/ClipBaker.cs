using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Labs.FacialRemote
{
    public class ClipBaker //: IUseEditorCallbackTicker
    {
        const string k_BlendShapeProp = "blendShape.{0}";

        AnimationClip m_Clip;

        Animator m_Animator;

        StreamReader m_StreamReader;

        StreamPlayback m_StreamPlayback;

        AvatarController m_AvatarController;

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
            BlendShapesController blendShapesController, AvatarController avatarController, Animator animator, string filePath)
        {
            m_Clip = clip;
            m_StreamReader = streamReader;
            m_StreamPlayback = streamPlayback;
            m_BlendShapesController = blendShapesController;
            m_Animator = animator;
            m_AvatarController = avatarController;
            m_FilePath = filePath;

            StartClipBaker(m_BlendShapesController.transform);
        }


        void StartClipBaker(Transform transform)
        {
            m_AnimationClipCurveData.Clear();
            m_StreamPlayback.SetPlaybackBuffer(m_StreamPlayback.activePlaybackBuffer); // TODO needed?
            m_StreamReader.SetStreamSource(m_StreamPlayback);
            m_BlendShapesController.SetupBlendShapeIndices();

            m_Baking = true;

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

            m_StreamSettings = m_StreamPlayback.activePlaybackBuffer;
            if (m_StreamSettings == null)
            {
                return;
            }

            m_CurrentFrame = 0;
            m_FrameCount = m_StreamPlayback.activePlaybackBuffer.recordStream.Length / m_StreamSettings.BufferSize;
            m_AvatarController.StartAnimatorSetup();
        }

        public void StopBake()
        {
            m_Baking = false;
        }

        public void ApplyAnimationCurves()
        {
            foreach (var curveData in m_AnimationClipCurveData)
            {
                m_Clip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);
                AnimationUtility.SetEditorCurve(m_Clip, EditorCurveBinding.FloatCurve(curveData.path, curveData.type, curveData.propertyName), curveData.curve);
            }

            AssetDatabase.CreateAsset(m_Clip, m_FilePath);

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
//                Debug.Log("current frame : " + m_CurrentFrame);
                if (m_CurrentFrame == 0)
                {
                    var startFrameBuffer = new byte[m_StreamSettings.BufferSize];
                    Buffer.BlockCopy(m_StreamPlayback.activePlaybackBuffer.recordStream, 0, startFrameBuffer, 0, m_StreamSettings.BufferSize);
                    Buffer.BlockCopy(startFrameBuffer, m_StreamSettings.FrameTimeOffset, m_FrameTimes, 0, sizeof(float));
                    Thread.Sleep(1);
                }

                if (m_CurrentFrame < frameCount)
                {
                    m_StreamPlayback.PlayBackLoop(true);
                    m_StreamPlayback.UpdateCurrentFrameBuffer(true);
                    m_BlendShapesController.InterpolateBlendShapes(true);
                    Buffer.BlockCopy(m_StreamPlayback.currentFrameBuffer, m_StreamSettings.FrameTimeOffset, m_FrameTimes, sizeof(float), sizeof(float));

                    KeyBlendShapes(m_FrameTimes[1] - m_FrameTimes[0]);

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

        void KeyBlendShapes(float time)
        {
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
        }

    }
}
