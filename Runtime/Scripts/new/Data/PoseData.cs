using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;
using UnityEngine;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PoseData
    {
        public float timeStamp;
        public Pose pose;
    }
}
