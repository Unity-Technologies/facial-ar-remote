using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for getting stream and tracking active bool state from the Stream Reader.
    /// </summary>
    public interface IUseReaderActive
    {
        /// <summary>
        /// Is a stream active on the Stream Reader.
        /// </summary>
        Func<bool> isStreamActive { get; set; }
        /// <summary>
        /// Is tracking active on the Stream Reader.
        /// </summary>
        Func<bool> isTrackingActive { get; set; }
    }
}
