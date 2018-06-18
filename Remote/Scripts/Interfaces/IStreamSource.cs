using System;

namespace Unity.Labs.FacialRemote 
{
    public interface IStreamSource
    {
        bool streamActive { get; }
        bool streamThreadActive { get; set; }
        Func<bool> IsStreamSource { get; set; }
        Func<StreamReader> getStreamReader { get; set; }

        Func<PlaybackData> getPlaybackData { get; set; }
        Func<bool> getUseDebug { get; set; }

        void StartStreamThread();
        void ActivateStreamSource();
        void DeactivateStreamSource();
        void SetReaderStreamSettings();
    }
}