using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR;
#endif

namespace Unity.Labs.FacialRemote
{
    public static class PoseArrayUtils
    {
        public const int PoseFloatCount = 7;
        public const int PoseSize = sizeof(float) * PoseFloatCount;
       
        /// <summary>
        /// Takes a correctly formatted array and returns a pose from that array.
        /// </summary>
        /// <param name="poseArray">Array of floats that encodes a pose.</param>
        /// <param name="pose">Pose encoded in the float array.</param>
        public static void ArrayToPose(float[] poseArray, ref Pose pose)
        {
            pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
            pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
        }

        /// <summary>
        /// Takes a pose and encodes the values to the given correctly formatted pose array.
        /// </summary>
        /// <param name="pose">Pose to encode in the float array.</param>
        /// <param name="poseArray">Float array to that the pose is encoded to.</param>
        public static void PoseToArray(Pose pose, float[] poseArray)
        {
            var position = pose.position;
            var rotation = pose.rotation;
            poseArray[0] = position.x;
            poseArray[1] = position.y;
            poseArray[2] = position.z;
            poseArray[3] = rotation.x;
            poseArray[4] = rotation.y;
            poseArray[5] = rotation.z;
            poseArray[6] = rotation.w;
        }
    }
}
