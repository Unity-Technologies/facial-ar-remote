using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for getting access to the stream reader
    /// </summary>
    public interface IUsesStreamReader
    {
        IStreamReader streamReader { set; }
    }
}
