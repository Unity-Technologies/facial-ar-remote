using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public interface IUseReaderCameraPose
    {
        Func<Pose> getCameraPose { get; set; }
    }
}