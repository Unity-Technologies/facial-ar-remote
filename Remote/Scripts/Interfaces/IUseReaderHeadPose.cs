using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface that allows the accessing of the head pose form the Stream Reader.
    /// </summary>
    public interface IUseReaderHeadPose
    {
        /// <summary>
        /// Used to get the head pose from the Stream Settings.
        /// </summary>
        Func<Pose> getHeadPose { get; set; }
    }
}
