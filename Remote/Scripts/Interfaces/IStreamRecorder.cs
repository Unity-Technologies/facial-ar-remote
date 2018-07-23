using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// APIs provided by a stream recorder
    /// </summary>
    public interface IStreamRecorder
    {
        /// <summary>
        /// Start recording the current stream
        /// </summary>
        /// <param name="streamSettings">Settings for the current stream</param>
        /// <param name="take">Current take number</param>
        void StartRecording(IStreamSettings streamSettings, int take);

        /// <summary>
        /// Adds data from the given buffer to the current recording
        /// </summary>
        /// <param name="buffer">Data to be added</param>
        /// <param name="offset">Offset into this buffer where the data starts</param>
        void AddDataToRecording(byte[] buffer, int offset = 0);

        /// <summary>
        /// Finish/finalize the current recording
        /// </summary>
        void FinishRecording();
    }
}
