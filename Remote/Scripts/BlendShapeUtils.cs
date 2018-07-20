using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#endif

namespace Unity.Labs.FacialRemote
{
    public static class BlendShapeUtils
    {
        public const int PoseFloatCount = 7;
        public const int PoseSize = sizeof(float) * PoseFloatCount;

#if UNITY_IOS
        public const string  BrowDownLeft        =   ARBlendShapeLocation.BrowDownLeft;
        public const string  BrowDownRight       =   ARBlendShapeLocation.BrowDownRight;
        public const string  BrowInnerUp         =   ARBlendShapeLocation.BrowInnerUp;
        public const string  BrowOuterUpLeft     =   ARBlendShapeLocation.BrowOuterUpLeft;
        public const string  BrowOuterUpRight    =   ARBlendShapeLocation.BrowOuterUpRight;
        public const string  CheekPuff           =   ARBlendShapeLocation.CheekPuff;
        public const string  CheekSquintLeft     =   ARBlendShapeLocation.CheekSquintLeft;
        public const string  CheekSquintRight    =   ARBlendShapeLocation.CheekSquintRight;
        public const string  EyeBlinkLeft        =   ARBlendShapeLocation.EyeBlinkLeft;
        public const string  EyeBlinkRight       =   ARBlendShapeLocation.EyeBlinkRight;
        public const string  EyeLookDownLeft     =   ARBlendShapeLocation.EyeLookDownLeft;
        public const string  EyeLookDownRight    =   ARBlendShapeLocation.EyeLookDownRight;
        public const string  EyeLookInLeft       =   ARBlendShapeLocation.EyeLookInLeft;
        public const string  EyeLookInRight      =   ARBlendShapeLocation.EyeLookInRight;
        public const string  EyeLookOutLeft      =   ARBlendShapeLocation.EyeLookOutLeft;
        public const string  EyeLookOutRight     =   ARBlendShapeLocation.EyeLookOutRight;
        public const string  EyeLookUpLeft       =   ARBlendShapeLocation.EyeLookUpLeft;
        public const string  EyeLookUpRight      =   ARBlendShapeLocation.EyeLookUpRight;
        public const string  EyeSquintLeft       =   ARBlendShapeLocation.EyeSquintLeft;
        public const string  EyeSquintRight      =   ARBlendShapeLocation.EyeSquintRight;
        public const string  EyeWideLeft         =   ARBlendShapeLocation.EyeWideLeft;
        public const string  EyeWideRight        =   ARBlendShapeLocation.EyeWideRight;
        public const string  JawForward          =   ARBlendShapeLocation.JawForward;
        public const string  JawLeft             =   ARBlendShapeLocation.JawLeft;
        public const string  JawOpen             =   ARBlendShapeLocation.JawOpen;
        public const string  JawRight            =   ARBlendShapeLocation.JawRight;
        public const string  MouthClose          =   ARBlendShapeLocation.MouthClose;
        public const string  MouthDimpleLeft     =   ARBlendShapeLocation.MouthDimpleLeft;
        public const string  MouthDimpleRight    =   ARBlendShapeLocation.MouthDimpleRight;
        public const string  MouthFrownLeft      =   ARBlendShapeLocation.MouthFrownLeft;
        public const string  MouthFrownRight     =   ARBlendShapeLocation.MouthFrownRight;
        public const string  MouthFunnel         =   ARBlendShapeLocation.MouthFunnel;
        public const string  MouthLeft           =   ARBlendShapeLocation.MouthLeft;
        public const string  MouthLowerDownLeft  =   ARBlendShapeLocation.MouthLowerDownLeft;
        public const string  MouthLowerDownRight =   ARBlendShapeLocation.MouthLowerDownRight;
        public const string  MouthPressLeft      =   ARBlendShapeLocation.MouthPressLeft;
        public const string  MouthPressRight     =   ARBlendShapeLocation.MouthPressRight;
        public const string  MouthPucker         =   ARBlendShapeLocation.MouthPucker;
        public const string  MouthRight          =   ARBlendShapeLocation.MouthRight;
        public const string  MouthRollLower      =   ARBlendShapeLocation.MouthRollLower;
        public const string  MouthRollUpper      =   ARBlendShapeLocation.MouthRollUpper;
        public const string  MouthShrugLower     =   ARBlendShapeLocation.MouthShrugLower;
        public const string  MouthShrugUpper     =   ARBlendShapeLocation.MouthShrugUpper;
        public const string  MouthSmileLeft      =   ARBlendShapeLocation.MouthSmileLeft;
        public const string  MouthSmileRight     =   ARBlendShapeLocation.MouthSmileRight;
        public const string  MouthStretchLeft    =   ARBlendShapeLocation.MouthStretchLeft;
        public const string  MouthStretchRight   =   ARBlendShapeLocation.MouthStretchRight;
        public const string  MouthUpperUpLeft    =   ARBlendShapeLocation.MouthUpperUpLeft;
        public const string  MouthUpperUpRight   =   ARBlendShapeLocation.MouthUpperUpRight;
        public const string  NoseSneerLeft       =   ARBlendShapeLocation.NoseSneerLeft;
        public const string  NoseSneerRight      =   ARBlendShapeLocation.NoseSneerRight;
#if ARKIT_2_0
        public const string  TongueOut           =   ARBlendShapeLocation.TongueOut;
#endif //ARKIT_2_0
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
#if ARKIT_2_0
        public const string  TongueOut           =   "tongueOut";
#endif //ARKIT_2_0
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
#if ARKIT_2_0
            TongueOut,
#endif
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
