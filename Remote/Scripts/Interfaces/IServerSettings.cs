using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for getting server settings from the Stream Reader.
    /// </summary>
    public interface IServerSettings
    {
        /// <summary>
        /// Used to get the Port number set in the Stream Reader.
        /// </summary>
        Func<int> getPortNumber { get; set; }
        /// <summary>
        /// Used to get the Frame Catchup Size set in the Stream Reader.
        /// </summary>
        Func<int> getFrameCatchupSize { get; set; }

        Func<int> getFrameCatchupThreshold { get; set; }
    }
}
