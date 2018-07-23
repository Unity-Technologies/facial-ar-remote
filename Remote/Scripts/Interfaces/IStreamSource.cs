using System;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc />
    /// <summary>
    /// Interface for common values of a stream source.
    /// </summary>
    public interface IStreamSource : IUsesStreamReader
    {
        IStreamSettings streamSettings { get; }
        bool active { get; }
        void StreamSourceUpdate();
    }
}
