using System;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc />
    /// <summary>
    /// Interface for getting access to the stream reader and getting settings changed callbacks
    /// </summary>
    public interface IUsesStreamSettings : IUsesStreamReader
    {
        void OnStreamSettingsChanged(IStreamSettings settings);
    }
}
