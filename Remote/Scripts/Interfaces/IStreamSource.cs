using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for common values of a stream source.
    /// </summary>
    public interface IStreamSource
    {
        /// <summary>
        /// Is this stream source active.
        /// </summary>
        bool streamActive { get; }
        /// <summary>
        /// Is the stream buffer thread active.
        /// </summary>
        bool streamThreadActive { get; set; }
        /// <summary>
        /// Is this stream source the active stream source in the Stream Reader.
        /// </summary>
        Func<bool> IsStreamSource { get; set; }
        /// <summary>
        /// Use to get the Stream Reader the stream source is connected to.
        /// </summary>
        Func<StreamReader> getStreamReader { get; set; }

        /// <summary>
        /// Allows the stream source access to the playback data referenced on the Stream Reader.
        /// </summary>
        Func<PlaybackData> getPlaybackData { get; set; }
        /// <summary>
        /// Is extra debugging logging active in the Stream Reader.
        /// </summary>
        Func<bool> getUseDebug { get; set; }

        /// <summary>
        /// Method for starting the Stream Processing Thread on a Stream Reader.
        /// </summary>
        void StartStreamThread();
        /// <summary>
        /// Sets this stream source to being the active stream data provider in the Stream Reader.
        /// </summary>
        void ActivateStreamSource();
        /// <summary>
        /// Deactivates the stream source from being the active stream data provider in the Stream Reader.
        /// </summary>
        void DeactivateStreamSource();
        /// <summary>
        /// Sets the active stream settings to be the referenced stream settings in the Stream Reader.
        /// </summary>
        void SetReaderStreamSettings();
    }
}
