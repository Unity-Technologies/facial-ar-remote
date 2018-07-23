using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// APIs provided by a stream reader
    /// </summary>
    public interface IStreamReader
    {
        /// <summary>
        /// The currently active stream source
        /// </summary>
        IStreamSource streamSource { get; }

        /// <summary>
        /// If true, log debug information to the console
        /// </summary>
        bool verboseLogging { get; }

        /// <summary>
        /// True when currently getting valid pose tracking data
        /// </summary>
        bool trackingActive { get; }

        /// <summary>
        /// Current blendshape weight values
        /// </summary>
        float[] blendShapesBuffer { get; }

        /// <summary>
        /// Current head pose
        /// </summary>
        Pose headPose { get; }

        /// <summary>
        /// Current camera pose
        /// </summary>
        Pose cameraPose { get; }

        /// <summary>
        /// Called by a StreamSource to when new data is available
        /// </summary>
        /// <param name="buffer">Data for this frame</param>
        /// <param name="offset">Offset into this buffer where the data starts</param>
        void UpdateStreamData(byte[] buffer, int offset = 0);
    }
}
