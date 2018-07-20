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
        static readonly string[] k_RotParams =
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

        readonly Dictionary<Object, Dictionary<string, AnimationClipCurveData>> m_AnimationCurves =
            new Dictionary<Object, Dictionary<string, AnimationClipCurveData>>();

        readonly List<AnimationClipCurveData> m_AnimationClipCurveData = new List<AnimationClipCurveData>();

        readonly float[] m_FrameTimes = new float[2];

        string m_FilePath;

        public int currentFrame { get; private set; }
        public int frameCount { get; private set; }
        public bool baking { get; private set; }

        bool useCharacterRigController { get { return m_CharacterRigController != null; } }
        bool useBlendShapeController { get { return m_BlendShapesController != null; } }

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
            if (!useCharacterRigController)
                Debug.LogWarning("No Character Rig Controller Found! Will not be able to bake Character Bone Animations.");

            if (!useBlendShapeController)
                Debug.LogWarning("No Blend Shape Controller Found! Will not be able to bake Character Blend Shape Animations.");

            m_AnimationClipCurveData.Clear();
            m_StreamPlayback.SetPlaybackBuffer(m_StreamPlayback.activePlaybackBuffer);
            m_StreamReader.streamSource = m_StreamPlayback;

            baking = true;

            if (useBlendShapeController)
            {
                m_BlendShapesController.SetupBlendShapeIndices();

                // Get curve data for blend shapes
                foreach (var skinnedMeshRenderer in m_BlendShapesController.skinnedMeshRenderers)
                {
                    if (skinnedMeshRenderer == null || m_AnimationCurves.ContainsKey(skinnedMeshRenderer)) // Skip duplicates
                        continue;

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
            }

            var streamSettings = m_StreamReader.streamSettings;
            if (useCharacterRigController)
            {
                m_CharacterRigController.SetupBlendShapeIndices(streamSettings);

                m_CharacterRigController.SetupCharacterRigController();

                // Get curve data for bone transforms
                var transformType = typeof(Transform);
                foreach (var bone in m_CharacterRigController.animatedBones)
                {
                    if (bone == null || m_AnimationCurves.ContainsKey(bone)) // Skip duplicates
                        continue;

                    var path = AnimationUtility.CalculateTransformPath(bone, transform);
                    var animationCurves = new Dictionary<string, AnimationClipCurveData>();
                    foreach (var prop in k_RotParams)
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
            }

            streamSettings = m_StreamPlayback.activePlaybackBuffer;
            if (streamSettings == null)
            {
                return;
            }

            currentFrame = 0;
            frameCount = m_StreamPlayback.activePlaybackBuffer.recordStream.Length / streamSettings.BufferSize;
        }


        public void StopBake()
        {
            baking = false;
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
                // This overrides the reference in the asset file
                // ReSharper disable once RedundantAssignment
                fileClip = m_Clip;
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(m_FilePath);
            }

            StopBake();
            Debug.Log("End Bake");
        }

        bool m_AnimatorInitialized;

        public void BakeClipLoop()
        {
            baking = BakeClipLoopInternal();
            if (!baking)
                StopBake();
        }

        bool BakeClipLoopInternal()
        {
            var streamSettings = m_StreamReader.streamSettings;
            while (currentFrame <= frameCount)
            {
                if (currentFrame == 0)
                {
                    var startFrameBuffer = new byte[streamSettings.BufferSize];
                    Buffer.BlockCopy(m_StreamPlayback.activePlaybackBuffer.recordStream, 0, startFrameBuffer, 0,
                        streamSettings.BufferSize);
                    Buffer.BlockCopy(startFrameBuffer, streamSettings.FrameTimeOffset, m_FrameTimes, 0, sizeof(float));
                    Thread.Sleep(1);
                }

                if (currentFrame < frameCount)
                {
                    m_StreamPlayback.PlayBackLoop(true);
                    m_StreamPlayback.UpdateCurrentFrameBuffer(true);

                    if (useBlendShapeController)
                        m_BlendShapesController.InterpolateBlendShapes(true);

                    if (useCharacterRigController)
                        m_CharacterRigController.InterpolateBlendShapes(true);

                    // Get next key frame time
                    Buffer.BlockCopy(m_StreamPlayback.currentFrameBuffer, streamSettings.FrameTimeOffset, m_FrameTimes,
                        sizeof(float), sizeof(float));
                    KeyFrame(m_FrameTimes[1] - m_FrameTimes[0]);

                    currentFrame++;
                }

                if (currentFrame == frameCount)
                {
                    currentFrame++;
                }

                return true;
            }
            return false;
        }

        void KeyFrame(float time)
        {
            // Key blend shapes
            if (useBlendShapeController)
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

            // Key bone rotation
            if (useCharacterRigController)
            {
                for (var i = 0; i < m_CharacterRigController.animatedBones.Length; i++)
                {
                    // Use head
                    if (i == 0 && !m_CharacterRigController.driveHead)
                        continue;

                    // Use neck
                    if (i == 1 && !m_CharacterRigController.driveNeck)
                        continue;

                    // Use Eyes
                    if (i == 2 && !m_CharacterRigController.driveEyes)
                        continue;

                    if (i == 3 && !m_CharacterRigController.driveEyes)
                        continue;

                    var bone = m_CharacterRigController.animatedBones[i];
                    Dictionary<string, AnimationClipCurveData> animationCurves;
                    if (m_AnimationCurves.TryGetValue(bone, out animationCurves))
                    {
                        foreach (var datum in animationCurves)
                        {
                            var prop = datum.Value.propertyName;
                            var curve = datum.Value.curve;
                            if (prop == k_RotParams[0])
                            {
                                curve.AddKey(time, bone.localRotation.x);
                            }
                            else if (prop == k_RotParams[1])
                            {
                                curve.AddKey(time, bone.localRotation.y);
                            }
                            else if (prop == k_RotParams[2])
                            {
                                curve.AddKey(time, bone.localRotation.z);
                            }
                            else if (prop == k_RotParams[3])
                            {
                                curve.AddKey(time, bone.localRotation.w);
                            }
                            else
                            {
                                Debug.LogErrorFormat("Fell through on {0} : {1}", datum.Key, prop);
                            }
                        }
                    }
                }
            }
        }

    }
}
