using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FaceData
    {
        public static readonly PacketDescriptor Descriptor = new PacketDescriptor() { type = PacketType.Face, version = 0 };
        
        [FieldOffset(0)] public float timeStamp;
        [FieldOffset(4)] public int id;
        [FieldOffset(8)] public BlendShapeValues blendShapeValues;
    }
}
