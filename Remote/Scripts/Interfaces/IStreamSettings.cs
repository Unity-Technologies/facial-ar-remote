namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for common stream settings values. That contain the settings for the byte stream and decoding information.
    /// </summary>
    public interface IStreamSettings
    {
        /// <summary>
        /// Error check byte value.
        /// </summary>
        byte ErrorCheck { get; }

        /// <summary>
        /// Number of blend shapes in the stream.
        /// </summary>
        int BlendShapeCount { get; }

        /// <summary>
        /// Size of blend shapes in the byte array.
        /// </summary>
        int BlendShapeSize { get; }

        /// <summary>
        /// Size of frame number value in byte array.
        /// </summary>
        int FrameNumberSize { get; }

        /// <summary>
        /// Size of frame time value in byte array.
        /// </summary>
        int FrameTimeSize { get; }

        /// <summary>
        /// Location of head pose in byte array.
        /// </summary>
        int HeadPoseOffset { get; }

        /// <summary>
        /// Location of camera pose in byte array.
        /// </summary>
        int CameraPoseOffset { get; }

        /// <summary>
        /// Location of frame number value in byte array.
        /// </summary>
        int FrameNumberOffset { get; }

        /// <summary>
        /// Location of frame time value in byte array.
        /// </summary>
        int FrameTimeOffset { get; }

        /// <summary>
        /// Total size of buffer of byte array for single same of data.
        /// </summary>
        int bufferSize { get; }

        /// <summary>
        /// String names of the blend shapes in the stream with their index in the array being their relative location.
        /// </summary>
        string[] locations { get; }

        /// <summary>
        /// Rename mapping values to apply blend shape locations to a blend shape controller.
        /// </summary>
        Mapping[] mappings { get; }
    }
}
