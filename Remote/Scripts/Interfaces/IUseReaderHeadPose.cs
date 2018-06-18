using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public interface IUseReaderHeadPose
    {
        Func<Pose> getHeadPose { get; set; }
    }
}