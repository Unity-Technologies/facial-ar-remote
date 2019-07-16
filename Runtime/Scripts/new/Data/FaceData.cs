using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FaceData : IPackageable
    {
        [FieldOffset(0)] public float timeStamp;
        [FieldOffset(4)] public int id;
        [FieldOffset(8)] public BlendShapeValues blendShapeValues;

        PacketDescriptor IPackageable.descriptor
        {
            get
            {
                return new PacketDescriptor()
                {
                    type = PacketType.Face,
                    version = 0
                };
            }
        }

        float IPackageable.timeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }
    }
}
