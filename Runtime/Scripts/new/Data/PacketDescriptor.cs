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

    [StructLayout(LayoutKind.Sequential)]
    public struct PacketDescriptor
    {
        public static readonly int DescriptorSize = Marshal.SizeOf<PacketDescriptor>();
        static readonly PacketDescriptor DescriptorInvalid = new PacketDescriptor() { type = PacketType.Invalid, version = 0 };
        static readonly PacketDescriptor DescriptorPose = new PacketDescriptor() { type = PacketType.Pose, version = 0 };
        static readonly PacketDescriptor DescriptorFace = new PacketDescriptor() { type = PacketType.Face, version = 0 };
        static readonly PacketDescriptor DescriptorHeadPose = new PacketDescriptor() { type = PacketType.HeadPose, version = 0 };

        public static PacketDescriptor Get(PacketType type)
        {
            switch (type)
            {
                case PacketType.Invalid:
                    return DescriptorInvalid;
                case PacketType.Pose:
                    return DescriptorPose;
                case PacketType.Face:
                    return DescriptorFace;
                case PacketType.HeadPose:
                    return DescriptorHeadPose;
            }

            return DescriptorInvalid;
        }

        public PacketType type;
        public int version;
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
