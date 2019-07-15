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
        public static readonly int Size = Marshal.SizeOf<PacketDescriptor>();

        [FieldOffset(0)] public PacketType type;
        [FieldOffset(4)] public int version;
        [FieldOffset(8)] public float time;
    }

    public interface IPackageable
    {
        PacketDescriptor descriptor { get; }
    }

    public static class PacketDescriptorExtensions
    {
        public static int GetPayloadSize(this PacketDescriptor packet)
        {
            switch (packet.type)
            {
                case PacketType.Face:
                    return GetFacePayloadSize(packet.version);
            }
            return 0;
        }

        static int GetFacePayloadSize(int version)
        {
            switch (version)
            {
                case 0:
                    return Marshal.SizeOf<FaceData>();
            }

            return 0;
        }
    }
}
