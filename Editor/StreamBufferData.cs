using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [StructLayout(LayoutKind.Explicit)]
    public struct StreamBufferData
    {
        [FieldOffset(0)] public byte ErrorCheck;
        [FieldOffset(1)] public BlendShapeValues BlendshapeValues;
        [FieldOffset(209)] public Vector3 HeadPosition;
        [FieldOffset(221)] public Quaternion HeadRotation;
        [FieldOffset(237)] public Vector3 CameraPosition;
        [FieldOffset(249)] public Quaternion CameraRotation;
        [FieldOffset(265)] public int FrameNumber;
        [FieldOffset(269)] public float FrameTime;
        [FieldOffset(273)] public int InputState;
        [FieldOffset(277)] public byte FaceTrackingActiveState;
        [FieldOffset(278)] public byte CameraTrackingActiveState;

        public static StreamBufferData Create(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (StreamBufferData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(StreamBufferData));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
