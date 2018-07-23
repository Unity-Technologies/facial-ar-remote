namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc />
    /// <summary>
    /// Interface for common values of a stream source.
    /// </summary>
    public interface IStreamSource : IUsesStreamReader
    {
        /// <summary>
        /// The IStreamSettings used to describe the data from this stream source
        /// </summary>
        IStreamSettings streamSettings { get; }

        /// <summary>
        /// Whether this stream source is currently updating tracking data
        /// </summary>
        bool active { get; }

        /// <summary>
        /// Called after the StreamReader updates
        /// </summary>
        void StreamSourceUpdate();
    }
}
