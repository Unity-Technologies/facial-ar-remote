using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
using UnityEngine.XR;
#endif

namespace Unity.Labs.FacialRemote
{
    public static class BlendShapeUtils
    {
        public const int PoseFloatCount = 7;
        public const int PoseSize = sizeof(float) * PoseFloatCount;
        public enum BlendShapeLocation
        {
#if UNITY_IOS
            BrowDownLeft        =   ARKitBlendShapeLocation.BrowDownLeft,
            BrowDownRight       =   ARKitBlendShapeLocation.BrowDownRight,
            BrowInnerUp         =   ARKitBlendShapeLocation.BrowInnerUp,
            BrowOuterUpLeft     =   ARKitBlendShapeLocation.BrowOuterUpLeft,
            BrowOuterUpRight    =   ARKitBlendShapeLocation.BrowOuterUpRight,
            CheekPuff           =   ARKitBlendShapeLocation.CheekPuff,
            CheekSquintLeft     =   ARKitBlendShapeLocation.CheekSquintLeft,
            CheekSquintRight    =   ARKitBlendShapeLocation.CheekSquintRight,
            EyeBlinkLeft        =   ARKitBlendShapeLocation.EyeBlinkLeft,
            EyeBlinkRight       =   ARKitBlendShapeLocation.EyeBlinkRight,
            EyeLookDownLeft     =   ARKitBlendShapeLocation.EyeLookDownLeft,
            EyeLookDownRight    =   ARKitBlendShapeLocation.EyeLookDownRight,
            EyeLookInLeft       =   ARKitBlendShapeLocation.EyeLookInLeft,
            EyeLookInRight      =   ARKitBlendShapeLocation.EyeLookInRight,
            EyeLookOutLeft      =   ARKitBlendShapeLocation.EyeLookOutLeft,
            EyeLookOutRight     =   ARKitBlendShapeLocation.EyeLookOutRight,
            EyeLookUpLeft       =   ARKitBlendShapeLocation.EyeLookUpLeft,
            EyeLookUpRight      =   ARKitBlendShapeLocation.EyeLookUpRight,
            EyeSquintLeft       =   ARKitBlendShapeLocation.EyeSquintLeft,
            EyeSquintRight      =   ARKitBlendShapeLocation.EyeSquintRight,
            EyeWideLeft         =   ARKitBlendShapeLocation.EyeWideLeft,
            EyeWideRight        =   ARKitBlendShapeLocation.EyeWideRight,
            JawForward          =   ARKitBlendShapeLocation.JawForward,
            JawLeft             =   ARKitBlendShapeLocation.JawLeft,
            JawOpen             =   ARKitBlendShapeLocation.JawOpen,
            JawRight            =   ARKitBlendShapeLocation.JawRight,
            MouthClose          =   ARKitBlendShapeLocation.MouthClose,
            MouthDimpleLeft     =   ARKitBlendShapeLocation.MouthDimpleLeft,
            MouthDimpleRight    =   ARKitBlendShapeLocation.MouthDimpleRight,
            MouthFrownLeft      =   ARKitBlendShapeLocation.MouthFrownLeft,
            MouthFrownRight     =   ARKitBlendShapeLocation.MouthFrownRight,
            MouthFunnel         =   ARKitBlendShapeLocation.MouthFunnel,
            MouthLeft           =   ARKitBlendShapeLocation.MouthLeft,
            MouthLowerDownLeft  =   ARKitBlendShapeLocation.MouthLowerDownLeft,
            MouthLowerDownRight =   ARKitBlendShapeLocation.MouthLowerDownRight,
            MouthPressLeft      =   ARKitBlendShapeLocation.MouthPressLeft,
            MouthPressRight     =   ARKitBlendShapeLocation.MouthPressRight,
            MouthPucker         =   ARKitBlendShapeLocation.MouthPucker,
            MouthRight          =   ARKitBlendShapeLocation.MouthRight,
            MouthRollLower      =   ARKitBlendShapeLocation.MouthRollLower,
            MouthRollUpper      =   ARKitBlendShapeLocation.MouthRollUpper,
            MouthShrugLower     =   ARKitBlendShapeLocation.MouthShrugLower,
            MouthShrugUpper     =   ARKitBlendShapeLocation.MouthShrugUpper,
            MouthSmileLeft      =   ARKitBlendShapeLocation.MouthSmileLeft,
            MouthSmileRight     =   ARKitBlendShapeLocation.MouthSmileRight,
            MouthStretchLeft    =   ARKitBlendShapeLocation.MouthStretchLeft,
            MouthStretchRight   =   ARKitBlendShapeLocation.MouthStretchRight,
            MouthUpperUpLeft    =   ARKitBlendShapeLocation.MouthUpperUpLeft,
            MouthUpperUpRight   =   ARKitBlendShapeLocation.MouthUpperUpRight,
            NoseSneerLeft       =   ARKitBlendShapeLocation.NoseSneerLeft,
            NoseSneerRight      =   ARKitBlendShapeLocation.NoseSneerRight,
            TongueOut           =   ARKitBlendShapeLocation.TongueOut
    #else
            BrowDownLeft        ,
            BrowDownRight       ,
            BrowInnerUp         ,
            BrowOuterUpLeft     ,
            BrowOuterUpRight    ,
            CheekPuff           ,
            CheekSquintLeft     ,
            CheekSquintRight    ,
            EyeBlinkLeft        ,
            EyeBlinkRight       ,
            EyeLookDownLeft     ,
            EyeLookDownRight    ,
            EyeLookInLeft       ,
            EyeLookInRight      ,
            EyeLookOutLeft      ,
            EyeLookOutRight     ,
            EyeLookUpLeft       ,
            EyeLookUpRight      ,
            EyeSquintLeft       ,
            EyeSquintRight      ,
            EyeWideLeft         ,
            EyeWideRight        ,
            JawForward          ,
            JawLeft             ,
            JawOpen             ,
            JawRight            ,
            MouthClose          ,
            MouthDimpleLeft     ,
            MouthDimpleRight    ,
            MouthFrownLeft      ,
            MouthFrownRight     ,
            MouthFunnel         ,
            MouthLeft           ,
            MouthLowerDownLeft  ,
            MouthLowerDownRight ,
            MouthPressLeft      ,
            MouthPressRight     ,
            MouthPucker         ,
            MouthRight          ,
            MouthRollLower      ,
            MouthRollUpper      ,
            MouthShrugLower     ,
            MouthShrugUpper     ,
            MouthSmileLeft      ,
            MouthSmileRight     ,
            MouthStretchLeft    ,
            MouthStretchRight   ,
            MouthUpperUpLeft    ,
            MouthUpperUpRight   ,
            NoseSneerLeft       ,
            NoseSneerRight      ,
            TongueOut
#endif
        }

        /// <summary>
        /// Array of the blend shape locations supported by the unity ARKit plugin.
        /// </summary>
        /// <remarks>
        /// ARKIT_2_0 is a custom scripting define symbol and will need to be enabled in
        /// 'PlayerSettings>platform>Scripting Define Symbols' for use in build
        /// </remarks>
        public static readonly string[] Locations =
        {
            BrowDownLeft,
            BrowDownRight,
            BrowInnerUp,
            BrowOuterUpLeft,
            BrowOuterUpRight,
            CheekPuff,
            CheekSquintLeft,
            CheekSquintRight,
            EyeBlinkLeft,
            EyeBlinkRight,
            EyeLookDownLeft,
            EyeLookDownRight,
            EyeLookInLeft,
            EyeLookInRight,
            EyeLookOutLeft,
            EyeLookOutRight,
            EyeLookUpLeft,
            EyeLookUpRight,
            EyeSquintLeft,
            EyeSquintRight,
            EyeWideLeft,
            EyeWideRight,
            JawForward,
            JawLeft,
            JawOpen,
            JawRight,
            MouthClose,
            MouthDimpleLeft,
            MouthDimpleRight,
            MouthFrownLeft,
            MouthFrownRight,
            MouthFunnel,
            MouthLeft,
            MouthLowerDownLeft,
            MouthLowerDownRight,
            MouthPressLeft,
            MouthPressRight,
            MouthPucker,
            MouthRight,
            MouthRollLower,
            MouthRollUpper,
            MouthShrugLower,
            MouthShrugUpper,
            MouthSmileLeft,
            MouthSmileRight,
            MouthStretchLeft,
            MouthStretchRight,
            MouthUpperUpLeft,
            MouthUpperUpRight,
            NoseSneerLeft,
            NoseSneerRight,
            TongueOut
        };

        /// <summary>
        /// Used for mapping the the blendshape locations this returns the index of the string in the Locations array.
        /// </summary>
        /// <param name="streamSettings">Stream Setting that contains the Locations array.</param>
        /// <param name="location">Name of blendshape location you want to find.</param>
        /// <returns>Index of string in Locations array.</returns>
        public static int GetLocationIndex(this IStreamSettings streamSettings, string location)
        {
            return Array.IndexOf(streamSettings.locations, location);
        }

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
