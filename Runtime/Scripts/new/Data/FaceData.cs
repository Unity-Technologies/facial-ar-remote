using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FaceData
    {
        public static readonly PacketDescriptor Descriptor = new PacketDescriptor() { type = PacketType.Face, version = 0 };

        public float timeStamp;
        public int id;
        public BlendShapeValues blendShapeValues;
    }
}
