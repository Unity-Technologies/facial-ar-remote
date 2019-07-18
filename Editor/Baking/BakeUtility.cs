using System;
using UnityEngine;
using PerformanceRecorder;

namespace Unity.Labs.FacialRemote
{
    public enum CaptureType
    {
        CameraPose,
        FaceRig
    }

    public class BakeUtility
    {
        public static readonly int[] s_SampleRates = { 24, 25, 30, 50, 60 };

        public static void Bake(PlaybackBuffer playbackBuffer, CaptureType captureType, int sampleRate, AnimationClip clip)
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
            var faceTrackingStateCurves = new BoolCurveBinding("", typeof(BlendShapesController), "m_TrackingActive");
            var buffer = new byte[bufferSize];
            
            Buffer.BlockCopy(stream, 0, buffer, 0, bufferSize);

            var timeStep = 1.0 / (double)sampleRate;
            var timeAcc = 0.0;
            var lastData = buffer.ToStruct<StreamBufferDataV1>();
            var startTime = lastData.FrameTime;
            var lastFrameTime = 0.0;

            for (var i = 0; i < stream.Length; i+=bufferSize)
            {
                Buffer.BlockCopy(stream, i, buffer, 0, bufferSize);
                var data = buffer.ToStruct<StreamBufferDataV1>();
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
                        //faceTrackingStateCurves.AddKey(time, data.FaceTrackingActiveState != 0);
                        faceTrackingStateCurves.AddKey(time, true);
                    }
                }
                else
                {
                    var deltaTime = time - lastTime;
                    timeAcc += deltaTime;

                    while (timeAcc >= timeStep)
                    {
                        var frameTime = lastFrameTime + timeStep;
                        var t = (float)((frameTime - lastTime) / deltaTime);
                        var headPosition = Vector3.Lerp(lastData.HeadPosition, data.HeadPosition, t);
                        var headRotation = Quaternion.Slerp(lastData.HeadRotation, data.HeadRotation, t);
                        var cameraPosition = Vector3.Lerp(lastData.CameraPosition, data.CameraPosition, t);
                        var cameraRotation = Quaternion.Slerp(lastData.CameraRotation, data.CameraRotation, t);
                        var blendShapeValues = BlendShapeValues.Lerp(ref lastData.BlendshapeValues, ref data.BlendshapeValues, t);

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
                            //faceTrackingStateCurves.AddKey((float)frameTime, lastData.FaceTrackingActiveState != 0);
                            faceTrackingStateCurves.AddKey((float)frameTime, true);
                        }

                        timeAcc -= timeStep;
                        lastFrameTime = frameTime;
                    }
                }

                lastData = data;
            }

            clip.ClearCurves();
            clip.frameRate = sampleRate;

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
                faceTrackingStateCurves.SetCurves(clip);
            }
        }
    }
}
