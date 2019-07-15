using System.IO;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public enum PacketType
    {
        Invalid = 0,
        Pose,
        Face,
        HeadPose
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PacketDescriptor
    {
        [FieldOffset(0)] public PacketType type;
        [FieldOffset(4)] public int version;

        public static int Size
        {
            get { return Marshal.SizeOf<PacketDescriptor>(); }
        }
    }

    public static class PacketDescriptorExtensions
    {
        public static int GetPayloadSize(this PacketDescriptor packet)
        {
            switch (packet.type)
            {
                case PacketType.Face:
                    return FaceData.Size;
            }
            return 0;
        }
    }
}
