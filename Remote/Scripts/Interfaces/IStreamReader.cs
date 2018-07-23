using System;
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
        PlaybackData playbackData { get; }
        bool useDebug { get; }
        bool active { get; }
        bool trackingActive { get; }
        float[] blendShapesBuffer { get; }
        Pose headPose { get; }
        Pose cameraPose { get; }
        void UpdateStreamData(ref byte[] buffer, int i);
    }
}
