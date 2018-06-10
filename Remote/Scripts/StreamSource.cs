using System;

namespace Unity.Labs.FacialRemote
{
    public interface IStreamSource
    {
        bool streamActive { get;}
        bool streamThreadActive { get; set;}
        Func<bool> isStreamSource { get; set; }
        Func<StreamReader> getStreamReader { get; set; }

        Func<PlaybackData> getPlaybackData { get; set; }
        Func<bool> getUseDebug { get; set; }

        Func<IStreamSettings> getStreamSettings { get; set; }

        void StartStreamThread();
        void ActivateStreamSource();
        void DeactivateStreamSource();
        void OnStreamSettingsChangeChange();
        void SetReaderStreamSettings();
    }

    public interface IServerSettings
    {
        Func<int> getPortNumber { get; set; }
        Func<int> getFrameCatchupSize { get; set; }
    }


    public abstract class StreamSource : IStreamSource
    {
        public Func<bool> isStreamSource { get; set; }
        public Func<IStreamSettings> getStreamSettings { get; set; }
        public Func<PlaybackData> getPlaybackData { get; set; }
        public Func<bool> getUseDebug { get; set; }
        public Func<StreamReader> getStreamReader { get; set; }

        public bool streamActive { get { return isStreamSource(); } }
        public bool streamThreadActive { get; set; }

        protected StreamReader streamReader { get { return getStreamReader(); } }
        protected IStreamSettings streamSettings { get { return getStreamSettings(); } }
        protected PlaybackData playbackData { get { return getPlaybackData(); } }
        protected bool useDebug { get { return getUseDebug(); } }

        public abstract void StreamSourceUpdate();
        public abstract void OnStreamSettingsChangeChange();
        public abstract void SetReaderStreamSettings();

        public virtual void ActivateStreamSource()
        {
            if (!isStreamSource())
            {
                streamReader.UnSetStreamSource();
                streamReader.SetStreamSource(this);
            }
        }

        public virtual void DeactivateStreamSource()
        {
            if (isStreamSource())
            {
                streamReader.UnSetStreamSource();
            }
        }

        public abstract void StartStreamThread();
        public abstract void StartPlaybackDataUsage();
        public abstract void StopPlaybackDataUsage();
        public abstract void UpdateCurrentFrameBuffer(bool force = false);
    }
}
