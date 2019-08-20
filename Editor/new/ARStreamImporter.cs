using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [ScriptedImporter(1, "arstream")]
    public class ARStreamImporter : ScriptedImporter
    {
        internal static readonly int[] s_SampleRates = { 24, 25, 30, 50, 60 };

        [SerializeField]
        int m_SampleRate = 24;
        [SerializeField]
        bool m_LoopTime = false;
        [SerializeField]
        bool m_LoopPose = false;
        [SerializeField]
        float m_CycleOffset = 0f;
        byte[] m_Buffer = new byte[1024];

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!ArrayUtility.Contains(s_SampleRates, m_SampleRate))
                return;

            using (var memoryStream = new MemoryStream(File.ReadAllBytes(ctx.assetPath)))
            {
                var clip = new AnimationClip();
                var settings = new AnimationClipSettings()
                {
                    loopTime = m_LoopTime,
                    loopBlend = m_LoopPose,
                    cycleOffset = m_CycleOffset
                };

                AnimationUtility.SetAnimationClipSettings(clip, settings);
                clip.frameRate = m_SampleRate;

                Bake(memoryStream, m_SampleRate, clip);

                ctx.AddObjectToAsset("clip", clip);
                ctx.SetMainObject(clip);
            }
        }

        void Bake(Stream stream, int sampleRate, AnimationClip clip)
        {
            var descriptor = default(ARStreamDescriptor);

            if (!stream.TryRead<ARStreamDescriptor>(out descriptor, m_Buffer))
                return;

            switch (descriptor.version)
            {
                default:
                    BakeV0(stream, sampleRate, clip);
                    break;
            }
        }

        void BakeV0(Stream stream, int sampleRate, AnimationClip clip)
        {
            var descriptor = default(PacketBufferDescriptor);

            while (stream.TryRead<PacketBufferDescriptor>(out descriptor, m_Buffer))
            {
                switch (descriptor.packetDescriptor.type)
                {
                    case PacketType.Invalid:
                        return;
                    case PacketType.Pose:
                        BakePoseData(stream, descriptor.packetDescriptor, descriptor.length, sampleRate, clip);
                        break;
                    case PacketType.Face:
                        BakeFaceData(stream, descriptor.packetDescriptor, descriptor.length, sampleRate, clip);
                        break;
                    default:
                        break;
                }
            }
        }

        void BakePoseData(Stream stream, PacketDescriptor descriptor, int length, int sampleRate, AnimationClip clip)
        {
            var positionCurves = new Vector3CurveBinding("", typeof(Transform), "m_LocalPosition");
            var rotationCurves = new QuaternionCurveBinding("", typeof(Transform), "m_LocalRotation");
            var data = default(PoseData);
            var lastData = default(PoseData);
            var startTime = 0f;
            var timeStep = 1.0 / (double)sampleRate;
            var timeAcc = 0.0;
            var lastFrameTime = 0.0;
            var first = true;
            var count = (long)0;
            var payloadSize = descriptor.GetPayloadSize();

            if (payloadSize == 0)
                return;

            var packetCount = length / payloadSize;

            while (count < packetCount &&
                stream.TryReadPoseData(descriptor.version, out data, m_Buffer))
            {
                if (first)
                {
                    first = false;
                    lastData = data;
                    startTime = data.timeStamp;
                    positionCurves.AddKey(0f, data.pose.position);
                    rotationCurves.AddKey(0f, data.pose.rotation);
                }
                else
                {
                    var time = data.timeStamp - startTime;
                    var lastTime = lastData.timeStamp - startTime;
                    var deltaTime = time - lastTime;
                    timeAcc += deltaTime;

                    while (timeAcc >= timeStep)
                    {
                        var frameTime = lastFrameTime + timeStep;
                        var t = (float)((frameTime - lastTime) / deltaTime);
                        var position = Vector3.Lerp(lastData.pose.position, data.pose.position, t);
                        var rotation = Quaternion.Slerp(lastData.pose.rotation, data.pose.rotation, t);
                        positionCurves.AddKey((float)frameTime, position);
                        rotationCurves.AddKey((float)frameTime, rotation);
                        
                        timeAcc -= timeStep;
                        lastFrameTime = frameTime;
                    }
                }

                lastData = data;
            }

            positionCurves.SetCurves(clip);
            rotationCurves.SetCurves(clip);
        }

        void BakeFaceData(Stream stream, PacketDescriptor descriptor, int length, int sampleRate, AnimationClip clip)
        {
            var blendShapeCurves = new BlendShapesCurveBinding("", typeof(BlendShapesController), "m_BlendShapeValues");
            //var headBonePositionCurves = new Vector3CurveBinding("", typeof(CharacterRigController), "m_HeadPose.position");
            var faceTrackingStateCurves = new BoolCurveBinding("", typeof(BlendShapesController), "m_TrackingActive");
            var data = default(FaceData);
            var lastData = default(FaceData);
            var startTime = 0f;
            var timeStep = 1.0 / (double)sampleRate;
            var timeAcc = 0.0;
            var lastFrameTime = 0.0;
            var first = true;
            var count = (long)0;
            var payloadSize = descriptor.GetPayloadSize();

            if (payloadSize == 0)
                return;

            var packetCount = length / payloadSize;

            while (count < packetCount &&
                stream.TryReadFaceData(descriptor.version, out data, m_Buffer))
            {
                if (first)
                {
                    first = false;
                    lastData = data;
                    startTime = data.timeStamp;
                    blendShapeCurves.AddKey(0f, ref data.blendShapeValues);
                    //faceTrackingStateCurves.AddKey(0f, data.FaceTrackingActiveState != 0);
                    faceTrackingStateCurves.AddKey(0f, true);
                }
                else
                {
                    var time = data.timeStamp - startTime;
                    var lastTime = lastData.timeStamp - startTime;
                    var deltaTime = time - lastTime;
                    timeAcc += deltaTime;

                    while (timeAcc >= timeStep)
                    {
                        var frameTime = lastFrameTime + timeStep;
                        var t = (float)((frameTime - lastTime) / deltaTime);
                        var blendShapeValues = BlendShapeValues.Lerp(ref lastData.blendShapeValues, ref data.blendShapeValues, t);

                        blendShapeCurves.AddKey((float)frameTime, ref blendShapeValues);
                        //faceTrackingStateCurves.AddKey((float)frameTime, lastData.FaceTrackingActiveState != 0);
                        faceTrackingStateCurves.AddKey((float)frameTime, true);

                        timeAcc -= timeStep;
                        lastFrameTime = frameTime;
                    }
                }

                lastData = data;
            }

            blendShapeCurves.SetCurves(clip);
            faceTrackingStateCurves.SetCurves(clip);
        }
    }

    [CustomEditor(typeof(ARStreamImporter))]
    [CanEditMultipleObjects]
    class ARStreamImporterEditor : ScriptedImporterEditor
    {
        SerializedProperty m_SampleRateProp;
        SerializedProperty m_LoopTimeProp;
        SerializedProperty m_LoopPoseProp;
        SerializedProperty m_CycleOffsetProp;
        GUIContent[] m_PopupContents;

        public override void OnEnable()
        {
            base.OnEnable();

            m_SampleRateProp = serializedObject.FindProperty("m_SampleRate");
            m_LoopTimeProp = serializedObject.FindProperty("m_LoopTime");
            m_LoopPoseProp = serializedObject.FindProperty("m_LoopPose");
            m_CycleOffsetProp = serializedObject.FindProperty("m_CycleOffset");

            m_PopupContents = Array.ConvertAll(ARStreamImporter.s_SampleRates, (sr) => new GUIContent(sr.ToString()));
        }

        public override void OnInspectorGUI()
        {
#if UNITY_2019_2_OR_NEWER
            // starting in 2019.2, AssetImporterEditor work the same as other Editors
            // and needs to call update/apply on serializedObject if any changes are made to serializedProperties.
            serializedObject.UpdateIfRequiredOrScript();
#endif
            EditorGUILayout.IntPopup(m_SampleRateProp, m_PopupContents, ARStreamImporter.s_SampleRates);
            EditorGUILayout.PropertyField(m_LoopTimeProp);
            
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledGroupScope(!m_LoopTimeProp.boolValue))
            {
                EditorGUILayout.PropertyField(m_LoopPoseProp);
                EditorGUILayout.PropertyField(m_CycleOffsetProp);
            }

            EditorGUI.indentLevel--;

#if UNITY_2019_2_OR_NEWER
            serializedObject.ApplyModifiedProperties();
#endif
            ApplyRevertGUI();
        }
    }
}
