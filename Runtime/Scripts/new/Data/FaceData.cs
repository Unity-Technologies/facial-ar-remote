using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FaceData
    {
        public float timeStamp;
        public int id;
        public BlendShapeValues blendShapeValues;
    }
}
