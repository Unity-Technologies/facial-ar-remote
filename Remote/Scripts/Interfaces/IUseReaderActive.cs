using System;

namespace Unity.Labs.FacialRemote 
{
    public interface IUseReaderActive
    {
        Func<bool> isStreamActive { get; set; }
        Func<bool> isTrackingActive { get; set; }
    }
}