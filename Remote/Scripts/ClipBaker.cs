using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class ClipBaker
    {
        const string k_BlendShapeProp = "blendShape.{0}";

        [SerializeField]
        AnimationClip m_Clip;

        [SerializeField]
        Animator m_Animator;

        [SerializeField]
        StreamReader m_StreamReader;

        [SerializeField]
        StreamPlayback m_StreamPlayback;

        [SerializeField]
        AvatarController m_AvatarController;

        [SerializeField]
        BlendShapesController m_BlendShapesController;

        Dictionary<Object, Dictionary<string, AnimationClipCurveData>> m_AnimationCurves =
            new Dictionary<Object, Dictionary<string, AnimationClipCurveData>>();
        Dictionary<Object, string> m_ComponentPaths = new Dictionary<Object, string>();

        List<AnimationClipCurveData> m_AnimationClipCurveData = new List<AnimationClipCurveData>();

        public ClipBaker(AnimationClip clip, StreamPlayback streamPlayback, BlendShapesController blendShapesController)
        {
            m_Clip = clip;
            m_StreamPlayback = streamPlayback;
            m_BlendShapesController = blendShapesController;
        }

        public void SetupClipBaker(Transform transform)
        {
            m_ComponentPaths.Clear();
            m_AnimationClipCurveData.Clear();

            m_BlendShapesController.SetupBlendShapeIndices();

            foreach (var skinnedMeshRenderer in m_BlendShapesController.skinnedMeshRenderers)
            {
                var path = AnimationUtility.CalculateTransformPath(skinnedMeshRenderer.transform, transform);
                path = path.Replace(string.Format("{0}/", skinnedMeshRenderer.transform), "");
                m_ComponentPaths.Add(skinnedMeshRenderer, path);

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
        }

        public void KeyBlendShapes(float time)
        {
            foreach (var skinnedMeshRenderer in m_BlendShapesController.skinnedMeshRenderers)
            {
                var animationCurves = new Dictionary<string, AnimationClipCurveData>();
                if (m_AnimationCurves.TryGetValue(skinnedMeshRenderer, out animationCurves))
                {
                    var shapeIndices = new BlendShapeIndexData[] { };
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

        public void BakeClip(Transform transform)
        {
            SetupClipBaker(transform);

            // bake frames
            var streamSettings = m_StreamPlayback.GetStreamSettings();
            var frameCount = m_StreamPlayback.activePlaybackBuffer.recordStream.Length / streamSettings.BufferSize;
            var startFrameBuffer = new byte[streamSettings.BufferSize];
            Buffer.BlockCopy(m_StreamPlayback.activePlaybackBuffer.recordStream, 0, startFrameBuffer, 0, streamSettings.BufferSize);
            var frameTimes = new float[2];
            Buffer.BlockCopy(startFrameBuffer, streamSettings.FrameTimeOffset, frameTimes, 0, sizeof(float));

            for (var i = 0; i < frameCount; i++)
            {
                m_StreamPlayback.PlayBackLoop(true);
                m_StreamPlayback.UpdateReader(true);
                m_BlendShapesController.InterpolateBlendShapes(true);
                Buffer.BlockCopy(m_StreamPlayback.currentFrameBuffer, streamSettings.FrameTimeOffset, frameTimes, sizeof(float), sizeof(float));

                KeyBlendShapes(frameTimes[1] - frameTimes[0]);
            }

            // set clip data
            foreach (var curveData in m_AnimationClipCurveData)
            {
                m_Clip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);
                AnimationUtility.SetEditorCurve(m_Clip, EditorCurveBinding.FloatCurve(curveData.path, curveData.type, curveData.propertyName), curveData.curve);
            }
        }
    }
}
