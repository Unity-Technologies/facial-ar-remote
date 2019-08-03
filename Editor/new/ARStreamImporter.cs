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

        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (var memoryStream = new MemoryStream(File.ReadAllBytes(ctx.assetPath)))
            {
                var clip = new AnimationClip();
                Bake(memoryStream, m_SampleRate, clip);
                ctx.AddObjectToAsset("clip", clip);
                ctx.SetMainObject(clip);
            }
            
        }

        public static void Bake(Stream stream, int sampleRate, AnimationClip clip)
        {
            if (Array.IndexOf(s_SampleRates, sampleRate) == -1)
                return;

            var blendShapeCurves = new BlendShapesCurveBinding("", typeof(BlendShapesController), "m_BlendShapeValues");
            var headBonePositionCurves = new Vector3CurveBinding("", typeof(CharacterRigController), "m_HeadPose.position");
            var faceTrackingStateCurves = new BoolCurveBinding("", typeof(BlendShapesController), "m_TrackingActive");
            
            var buffer = new byte[1024];
            var descriptor = default(PacketDescriptor);

            try
            {
                descriptor = stream.Read<PacketDescriptor>(buffer);
            }
            catch
            {
                return;
            }

            if (descriptor.type != PacketType.Face)
                return;

            var data = default(FaceData);
            var lastData = default(FaceData);
            var startTime = 0f;
            var timeStep = 1.0 / (double)sampleRate;
            var timeAcc = 0.0;
            var lastFrameTime = 0.0;
            var first = true;

            while (stream.ReadFaceData(descriptor.version, out data, buffer))
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

            clip.ClearCurves();
            clip.frameRate = sampleRate;

            blendShapeCurves.SetCurves(clip);
            faceTrackingStateCurves.SetCurves(clip);
        }
    }

    [CustomEditor(typeof(ARStreamImporter))]
    [CanEditMultipleObjects]
    class ARStreamImporterEditor : ScriptedImporterEditor
    {
        SerializedProperty m_SampleRateProp;
        GUIContent[] m_PopupContents;

        public override void OnEnable()
        {
            base.OnEnable();

            m_SampleRateProp = serializedObject.FindProperty("m_SampleRate");

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

#if UNITY_2019_2_OR_NEWER
            serializedObject.ApplyModifiedProperties();
#endif
            ApplyRevertGUI();
        }
    }
}
