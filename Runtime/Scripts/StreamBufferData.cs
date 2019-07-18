using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StreamBufferDataV1
    {
        public byte ErrorCheck;
        public BlendShapeValues BlendshapeValues;
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        public Vector3 CameraPosition;
        public Quaternion CameraRotation;
        public int FrameNumber;
        public float FrameTime;
        public byte TrackingActive;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StreamBufferDataV2
    {
        public byte ErrorCheck;
        public BlendShapeValues BlendshapeValues;
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        public Vector3 CameraPosition;
        public Quaternion CameraRotation;
        public int FrameNumber;
        public float FrameTime;
        public int InputState;
        public byte FaceTrackingActiveState;
        public byte CameraTrackingActiveState;
    }
}
