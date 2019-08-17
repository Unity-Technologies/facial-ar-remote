using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public enum PacketType
    {
        Invalid = 0,
        Pose,
        Face,
        HeadPose,
        Command
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacketDescriptor
    {
        public static readonly int DescriptorSize = Marshal.SizeOf<PacketDescriptor>();
        static readonly PacketDescriptor InvalidDescriptor = new PacketDescriptor() { type = PacketType.Invalid, version = 0 };
        static readonly PacketDescriptor PoseDescriptor = new PacketDescriptor() { type = PacketType.Pose, version = 0 };
        static readonly PacketDescriptor FaceDescriptor = new PacketDescriptor() { type = PacketType.Face, version = 0 };
        static readonly PacketDescriptor HeadPoseDescriptor = new PacketDescriptor() { type = PacketType.HeadPose, version = 0 };
        static readonly PacketDescriptor CommandDescriptor = new PacketDescriptor() { type = PacketType.Command, version = 0 };

        public static PacketDescriptor Get(PacketType type)
        {
            switch (type)
            {
                case PacketType.Invalid:
                    return InvalidDescriptor;
                case PacketType.Pose:
                    return PoseDescriptor;
                case PacketType.Face:
                    return FaceDescriptor;
                case PacketType.HeadPose:
                    return HeadPoseDescriptor;
                case PacketType.Command:
                    return CommandDescriptor;
            }

            return InvalidDescriptor;
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
                    return GetFaceDataSize(packet.version);
                case PacketType.Command:
                    return GetCommandSize(packet.version);
            }
            return 0;
        }

        static int GetFaceDataSize(int version)
        {
            switch (version)
            {
                case 0:
                    return Marshal.SizeOf<FaceData>();
            }

            return 0;
        }

        static int GetCommandSize(int version)
        {
            switch (version)
            {
                case 0:
                    return Marshal.SizeOf<Command>();
            }

            return 0;
        }
    }
}
