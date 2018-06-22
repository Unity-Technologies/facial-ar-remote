using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface that allows the accessing of the camera pose form the Stream Reader.
    /// </summary>
    public interface IUseReaderCameraPose
    {
        /// <summary>
        /// Used to get the camera pose from the Stream Settings.
        /// </summary>
        Func<Pose> getCameraPose { get; set; }
    }
}
