using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for using stream settings from the Stream Reader.
    /// </summary>
    public interface IUseStreamSettings
    {
        /// <summary>
        /// Gets the active stream settings from the Stream Reader.
        /// </summary>
        Func<IStreamSettings> getStreamSettings { get; set; }
        /// <summary>
        /// Gets the stream settings referenced in the Stream Reader.
        /// </summary>
        Func<IStreamSettings> getReaderStreamSettings { get; set; }
        /// <summary>
        /// Callback for when stream settings change.
        /// </summary>
        void OnStreamSettingsChange();
    }
}
