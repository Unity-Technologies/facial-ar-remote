using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StreamBufferData
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
