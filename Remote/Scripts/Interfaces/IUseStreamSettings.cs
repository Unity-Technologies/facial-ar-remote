using System;

namespace Unity.Labs.FacialRemote
{
    public interface IUseStreamSettings
    {
        Func<IStreamSettings> getStreamSettings { get; set; }
        Func<IStreamSettings> getReaderStreamSettings { get; set; } // TODO should try to always use active settings.
        void OnStreamSettingsChange();
    }
}