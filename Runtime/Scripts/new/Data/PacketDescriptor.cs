using System.IO;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public enum PacketType
    {
        Pose = 0,
        FaceRig
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PacketDescriptor
    {
        [FieldOffset(0)] public PacketType type;
        [FieldOffset(4)] public int version;

        public static int Size
        {
            get { return 8; }
        }
    }

    public static class PacketDescriptorExtensions
    {
        public static int GetPayloadSize(this PacketDescriptor packet)
        {
            return 0;
        }
    }
}
