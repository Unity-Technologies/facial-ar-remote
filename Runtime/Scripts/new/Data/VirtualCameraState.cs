using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;
using UnityEngine;

namespace PerformanceRecorder
{
    public enum CameraRig
    {
        HardLock,
        LightDamping,
        MediumDamping,
        HeavyDamping,
        Steadicam
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualCameraState
    {
        public CameraRig cameraRig;
    }
}
