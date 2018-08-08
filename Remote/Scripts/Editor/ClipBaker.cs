using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.Labs.FacialRemote
{
    public class ClipBaker
    {
        const int k_FramesPerStep = 16;
        const string k_BlendShapeProp = "blendShape.{0}";
        const string k_RotationX = "localRotation.x";
        const string k_RotationY = "localRotation.y";
        const string k_RotationZ = "localRotation.z";
        const string k_RotationW = "localRotation.w";
        static readonly string[] k_RotParams =
        {
            k_RotationX,
            k_RotationY,
            k_RotationZ,
            k_RotationW
        };

        AnimationClip m_Clip;
        StreamReader m_StreamReader;
        PlaybackStream m_PlaybackStream;
        CharacterRigController m_CharacterRigController;
        BlendShapesController m_BlendShapesController;

        readonly Dictionary<UnityObject, Dictionary<string, AnimationClipCurveData>> m_AnimationCurves =
            new Dictionary<UnityObject, Dictionary<string, AnimationClipCurveData>>();

        readonly List<AnimationClipCurveData> m_AnimationClipCurveData = new List<AnimationClipCurveData>();

        readonly float[] m_FrameTime = new float[1];
        float m_FirstFrameTime;

        string m_FilePath;

        public int currentFrame { get; private set; }
        public int frameCount { get; private set; }
        public bool baking { get; private set; }

        public ClipBaker(AnimationClip clip, StreamReader streamReader, PlaybackStream playbackStream,
            BlendShapesController blendShapesController, CharacterRigController characterRigController, string filePath)
        {
            m_Clip = clip;
            m_StreamReader = streamReader;
            m_PlaybackStream = playbackStream;
            m_BlendShapesController = blendShapesController;
            m_CharacterRigController = characterRigController;
            m_FilePath = filePath;

            StartClipBaker(m_BlendShapesController != null
                ? m_BlendShapesController.transform
                : m_CharacterRigController.transform);
        }

        void StartClipBaker(Transform transform)
        {
            var playbackBuffer = m_PlaybackStream.activePlaybackBuffer;
            if (playbackBuffer == null)
                return;

            if (m_CharacterRigController == null)
                Debug.LogWarning("No Character Rig Controller Found! Will not be able to bake Character Bone Animations.");

            if (m_BlendShapesController == null)
                Debug.LogWarning("No Blend Shape Controller Found! Will not be able to bake Character Blend Shape Animations.");

            m_AnimationClipCurveData.Clear();

            baking = true;

            if (m_BlendShapesController != null)
            {
                m_BlendShapesController.UpdateBlendShapeIndices(playbackBuffer);

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

            if (m_CharacterRigController != null)
            {
                m_CharacterRigController.UpdateBlendShapeIndices(playbackBuffer);

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

            currentFrame = 0;
            var recordStream = playbackBuffer.recordStream;
            frameCount = recordStream.Length / playbackBuffer.bufferSize;

            Buffer.BlockCopy(recordStream, playbackBuffer.FrameTimeOffset, m_FrameTime, 0, playbackBuffer.FrameTimeSize);
            m_FirstFrameTime = m_FrameTime[0];
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
                var path = curveData.path;
                var propertyName = curveData.propertyName;
                var type = curveData.type;
                var curve = curveData.curve;
                m_Clip.SetCurve(path, type, propertyName, curve);
                AnimationUtility.SetEditorCurve(m_Clip, EditorCurveBinding.FloatCurve(path, type, propertyName), curve);
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

        public void BakeClipLoop()
        {
            if (!BakeClipLoopInternal())
                StopBake();
        }

        bool BakeClipLoopInternal()
        {
            if (frameCount < 1)
                return false;

            var streamSettings = m_StreamReader.streamSource.streamSettings;
            var bufferSize = streamSettings.bufferSize;
            var frameTimeOffset = streamSettings.FrameTimeOffset;
            var frameTimeSize = streamSettings.FrameTimeSize;
            var recordStream = m_PlaybackStream.activePlaybackBuffer.recordStream;
            for (var i = 0; i < k_FramesPerStep && currentFrame < frameCount; i++, currentFrame++)
            {
                Buffer.BlockCopy(recordStream, currentFrame * bufferSize + frameTimeOffset, m_FrameTime, 0, frameTimeSize);

                // Run normal playback to record transform keyframes
                m_PlaybackStream.PlayBackLoop();
                m_PlaybackStream.UpdateCurrentFrameBuffer(true);

                if (m_BlendShapesController != null)
                    m_BlendShapesController.InterpolateBlendShapes(true);

                if (m_CharacterRigController != null)
                    m_CharacterRigController.InterpolateBlendShapes(true);

                KeyFrame(m_FrameTime[0] - m_FirstFrameTime);
            }

            return true;
        }

        void KeyFrame(float time)
        {
            // Key blend shapes
            if (m_BlendShapesController != null)
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
                                var index = datum.index;
                                if (index < 0)
                                    continue;

                                var curve = animationCurves[string.Format(k_BlendShapeProp, datum.name)].curve;
                                curve.AddKey(time, m_BlendShapesController.blendShapesScaled[index]);
                            }
                        }
                    }
                }
            }

            // Key bone rotation
            if (m_CharacterRigController)
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
                            var curveData = datum.Value;
                            var prop = curveData.propertyName;
                            var curve = curveData.curve;
                            switch (prop) {
                                case k_RotationX:
                                    curve.AddKey(time, bone.localRotation.x);
                                    break;
                                case k_RotationY:
                                    curve.AddKey(time, bone.localRotation.y);
                                    break;
                                case k_RotationZ:
                                    curve.AddKey(time, bone.localRotation.z);
                                    break;
                                case k_RotationW:
                                    curve.AddKey(time, bone.localRotation.w);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Fell through on {0} : {1}", datum.Key, prop);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
