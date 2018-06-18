using System;

namespace Unity.Labs.FacialRemote
{
    public interface IServerSettings
    {
        Func<int> getPortNumber { get; set; }
        Func<int> getFrameCatchupSize { get; set; }
    }
}