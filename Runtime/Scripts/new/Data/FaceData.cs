using System.IO;
using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FaceData
    {
        [FieldOffset(0)] public BlendShapeValues blendShapeValues;

        public static int Size
        {
            get { return Marshal.SizeOf<FaceData>(); }
        }
    }
}
