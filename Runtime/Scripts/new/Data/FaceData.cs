using System.IO;
using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FaceData : IPackageable
    {
        [FieldOffset(0)] public BlendShapeValues blendShapeValues;

        public PacketDescriptor descriptor
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
    }
}
