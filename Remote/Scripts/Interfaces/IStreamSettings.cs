using System;

namespace Unity.Labs.FacialRemote
{
    public interface IStreamSettings
    {
        byte ErrorCheck { get; }
        int BlendShapeCount { get; }
        int BlendShapeSize { get; }
        int PoseSize { get; }
        int FrameNumberSize { get; }
        int FrameTimeSize { get; }
        int HeadPoseOffset { get; }
        int CameraPoseOffset { get; }
        int FrameNumberOffset  { get; }
        int FrameTimeOffset { get; }
        int BufferSize { get; }

        string[] locations { get; }
        Mapping[] mappings { get; }
    }
}