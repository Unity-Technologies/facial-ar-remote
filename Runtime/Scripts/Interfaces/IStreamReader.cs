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
        /// True when the head position is static between frames
        /// </summary>
        bool faceTrackingLost { get; }

        /// <summary>
        /// Current blend shape weight values
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
        /// Called by a StreamSource when new data is available
        /// </summary>
        /// <param name="buffer">Data for this frame</param>
        /// <param name="offset">Offset into this buffer where the data starts</param>
        void UpdateStreamData(byte[] buffer, int offset = 0);

        /// <summary>
        /// The current touch phase 
        /// </summary>
        TouchPhase touchPhase { get; }
        
        /// <summary>
        /// The current touch position. Be sure to read the touch phase first.
        /// </summary>
        Vector2 touchPosition  { get; }

        /// <summary>
        /// Called from an actor to set the initial head pose
        /// </summary>
        /// <param name="pose">The initial pose</param>
        void SetInitialHeadPose(Pose pose);
        
        /// <summary>
        /// Called from an actor to set the initial camera pose
        /// </summary>
        /// <param name="pose">The initial pose</param>
        void SetInitialCameraPose(Pose pose);
    }
}
