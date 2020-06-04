#if UNITY_IOS && !UNITY_EDITOR && INCLUDE_ARKIT_FACE_PLUGIN
#define FACETRACKING
#endif

using System;
using UnityEngine;
using UnityEngine.XR.ARKit;

#if FACETRACKING
using UnityEngine.XR.ARKit;
#endif

namespace Unity.Labs.FacialRemote
{
    public static class BlendShapeUtils
    {
        public const int PoseFloatCount = 7;
        public const int PoseSize = sizeof(float) * PoseFloatCount;

#if FACETRACKING
        public static readonly string  BrowDownLeft        =   ARKitBlendShapeLocation.BrowDownLeft.ToString();
        public static readonly string  BrowDownRight       =   ARKitBlendShapeLocation.BrowDownRight.ToString();
        public static readonly string  BrowInnerUp         =   ARKitBlendShapeLocation.BrowInnerUp.ToString();
        public static readonly string  BrowOuterUpLeft     =   ARKitBlendShapeLocation.BrowOuterUpLeft.ToString();
        public static readonly string  BrowOuterUpRight    =   ARKitBlendShapeLocation.BrowOuterUpRight.ToString();
        public static readonly string  CheekPuff           =   ARKitBlendShapeLocation.CheekPuff.ToString();
        public static readonly string  CheekSquintLeft     =   ARKitBlendShapeLocation.CheekSquintLeft.ToString();
        public static readonly string  CheekSquintRight    =   ARKitBlendShapeLocation.CheekSquintRight.ToString();
        public static readonly string  EyeBlinkLeft        =   ARKitBlendShapeLocation.EyeBlinkLeft.ToString();
        public static readonly string  EyeBlinkRight       =   ARKitBlendShapeLocation.EyeBlinkRight.ToString();
        public static readonly string  EyeLookDownLeft     =   ARKitBlendShapeLocation.EyeLookDownLeft.ToString();
        public static readonly string  EyeLookDownRight    =   ARKitBlendShapeLocation.EyeLookDownRight.ToString();
        public static readonly string  EyeLookInLeft       =   ARKitBlendShapeLocation.EyeLookInLeft.ToString();
        public static readonly string  EyeLookInRight      =   ARKitBlendShapeLocation.EyeLookInRight.ToString();
        public static readonly string  EyeLookOutLeft      =   ARKitBlendShapeLocation.EyeLookOutLeft.ToString();
        public static readonly string  EyeLookOutRight     =   ARKitBlendShapeLocation.EyeLookOutRight.ToString();
        public static readonly string  EyeLookUpLeft       =   ARKitBlendShapeLocation.EyeLookUpLeft.ToString();
        public static readonly string  EyeLookUpRight      =   ARKitBlendShapeLocation.EyeLookUpRight.ToString();
        public static readonly string  EyeSquintLeft       =   ARKitBlendShapeLocation.EyeSquintLeft.ToString();
        public static readonly string  EyeSquintRight      =   ARKitBlendShapeLocation.EyeSquintRight.ToString();
        public static readonly string  EyeWideLeft         =   ARKitBlendShapeLocation.EyeWideLeft.ToString();
        public static readonly string  EyeWideRight        =   ARKitBlendShapeLocation.EyeWideRight.ToString();
        public static readonly string  JawForward          =   ARKitBlendShapeLocation.JawForward.ToString();
        public static readonly string  JawLeft             =   ARKitBlendShapeLocation.JawLeft.ToString();
        public static readonly string  JawOpen             =   ARKitBlendShapeLocation.JawOpen.ToString();
        public static readonly string  JawRight            =   ARKitBlendShapeLocation.JawRight.ToString();
        public static readonly string  MouthClose          =   ARKitBlendShapeLocation.MouthClose.ToString();
        public static readonly string  MouthDimpleLeft     =   ARKitBlendShapeLocation.MouthDimpleLeft.ToString();
        public static readonly string  MouthDimpleRight    =   ARKitBlendShapeLocation.MouthDimpleRight.ToString();
        public static readonly string  MouthFrownLeft      =   ARKitBlendShapeLocation.MouthFrownLeft.ToString();
        public static readonly string  MouthFrownRight     =   ARKitBlendShapeLocation.MouthFrownRight.ToString();
        public static readonly string  MouthFunnel         =   ARKitBlendShapeLocation.MouthFunnel.ToString();
        public static readonly string  MouthLeft           =   ARKitBlendShapeLocation.MouthLeft.ToString();
        public static readonly string  MouthLowerDownLeft  =   ARKitBlendShapeLocation.MouthLowerDownLeft.ToString();
        public static readonly string  MouthLowerDownRight =   ARKitBlendShapeLocation.MouthLowerDownRight.ToString();
        public static readonly string  MouthPressLeft      =   ARKitBlendShapeLocation.MouthPressLeft.ToString();
        public static readonly string  MouthPressRight     =   ARKitBlendShapeLocation.MouthPressRight.ToString();
        public static readonly string  MouthPucker         =   ARKitBlendShapeLocation.MouthPucker.ToString();
        public static readonly string  MouthRight          =   ARKitBlendShapeLocation.MouthRight.ToString();
        public static readonly string  MouthRollLower      =   ARKitBlendShapeLocation.MouthRollLower.ToString();
        public static readonly string  MouthRollUpper      =   ARKitBlendShapeLocation.MouthRollUpper.ToString();
        public static readonly string  MouthShrugLower     =   ARKitBlendShapeLocation.MouthShrugLower.ToString();
        public static readonly string  MouthShrugUpper     =   ARKitBlendShapeLocation.MouthShrugUpper.ToString();
        public static readonly string  MouthSmileLeft      =   ARKitBlendShapeLocation.MouthSmileLeft.ToString();
        public static readonly string  MouthSmileRight     =   ARKitBlendShapeLocation.MouthSmileRight.ToString();
        public static readonly string  MouthStretchLeft    =   ARKitBlendShapeLocation.MouthStretchLeft.ToString();
        public static readonly string  MouthStretchRight   =   ARKitBlendShapeLocation.MouthStretchRight.ToString();
        public static readonly string  MouthUpperUpLeft    =   ARKitBlendShapeLocation.MouthUpperUpLeft.ToString();
        public static readonly string  MouthUpperUpRight   =   ARKitBlendShapeLocation.MouthUpperUpRight.ToString();
        public static readonly string  NoseSneerLeft       =   ARKitBlendShapeLocation.NoseSneerLeft.ToString();
        public static readonly string  NoseSneerRight      =   ARKitBlendShapeLocation.NoseSneerRight.ToString();
        public static readonly string  TongueOut           =   ARKitBlendShapeLocation.TongueOut.ToString();
#else
        public const string  BrowDownLeft        =   "browDown_L";
        public const string  BrowDownRight       =   "browDown_R";
        public const string  BrowInnerUp         =   "browInnerUp";
        public const string  BrowOuterUpLeft     =   "browOuterUp_L";
        public const string  BrowOuterUpRight    =   "browOuterUp_R";
        public const string  CheekPuff           =   "cheekPuff";
        public const string  CheekSquintLeft     =   "cheekSquint_L";
        public const string  CheekSquintRight    =   "cheekSquint_R";
        public const string  EyeBlinkLeft        =   "eyeBlink_L";
        public const string  EyeBlinkRight       =   "eyeBlink_R";
        public const string  EyeLookDownLeft     =   "eyeLookDown_L";
        public const string  EyeLookDownRight    =   "eyeLookDown_R";
        public const string  EyeLookInLeft       =   "eyeLookIn_L";
        public const string  EyeLookInRight      =   "eyeLookIn_R";
        public const string  EyeLookOutLeft      =   "eyeLookOut_L";
        public const string  EyeLookOutRight     =   "eyeLookOut_R";
        public const string  EyeLookUpLeft       =   "eyeLookUp_L";
        public const string  EyeLookUpRight      =   "eyeLookUp_R";
        public const string  EyeSquintLeft       =   "eyeSquint_L";
        public const string  EyeSquintRight      =   "eyeSquint_R";
        public const string  EyeWideLeft         =   "eyeWide_L";
        public const string  EyeWideRight        =   "eyeWide_R";
        public const string  JawForward          =   "jawForward";
        public const string  JawLeft             =   "jawLeft";
        public const string  JawOpen             =   "jawOpen";
        public const string  JawRight            =   "jawRight";
        public const string  MouthClose          =   "mouthClose";
        public const string  MouthDimpleLeft     =   "mouthDimple_L";
        public const string  MouthDimpleRight    =   "mouthDimple_R";
        public const string  MouthFrownLeft      =   "mouthFrown_L";
        public const string  MouthFrownRight     =   "mouthFrown_R";
        public const string  MouthFunnel         =   "mouthFunnel";
        public const string  MouthLeft           =   "mouthLeft";
        public const string  MouthLowerDownLeft  =   "mouthLowerDown_L";
        public const string  MouthLowerDownRight =   "mouthLowerDown_R";
        public const string  MouthPressLeft      =   "mouthPress_L";
        public const string  MouthPressRight     =   "mouthPress_R";
        public const string  MouthPucker         =   "mouthPucker";
        public const string  MouthRight          =   "mouthRight";
        public const string  MouthRollLower      =   "mouthRollLower";
        public const string  MouthRollUpper      =   "mouthRollUpper";
        public const string  MouthShrugLower     =   "mouthShrugLower";
        public const string  MouthShrugUpper     =   "mouthShrugUpper";
        public const string  MouthSmileLeft      =   "mouthSmile_L";
        public const string  MouthSmileRight     =   "mouthSmile_R";
        public const string  MouthStretchLeft    =   "mouthStretch_L";
        public const string  MouthStretchRight   =   "mouthStretch_R";
        public const string  MouthUpperUpLeft    =   "mouthUpperUp_L";
        public const string  MouthUpperUpRight   =   "mouthUpperUp_R";
        public const string  NoseSneerLeft       =   "noseSneer_L";
        public const string  NoseSneerRight      =   "noseSneer_R";
        public const string  TongueOut           =   "tongueOut";
#endif

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
            TongueOut,
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
