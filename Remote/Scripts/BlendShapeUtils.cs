using System;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote 
{
    public static class BlendShapeUtils
    {
        public const string EyeLookDownLeft = ARBlendShapeLocation.EyeLookDownLeft;
        public const string EyeLookDownRight = ARBlendShapeLocation.EyeLookDownRight;
        public const string EyeLookInLeft = ARBlendShapeLocation.EyeLookInLeft;
        public const string EyeLookInRight = ARBlendShapeLocation.EyeLookInRight;
        public const string EyeLookOutLeft = ARBlendShapeLocation.EyeLookOutLeft;
        public const string EyeLookOutRight = ARBlendShapeLocation.EyeLookOutRight;
        public const string EyeLookUpLeft = ARBlendShapeLocation.EyeLookUpLeft;
        public const string EyeLookUpRight = ARBlendShapeLocation.EyeLookUpRight;
        
        
        /// <summary>
        /// Array of the blend shape locations supported by the unity arkit plugin.
        /// </summary>
        /// <remarks>
        /// ARKIT_2_0 is a custom scrtipting define symbol and will neeed to be enabled in
        /// 'PlayerSettings>platform>Scripting Define Symbols' for use in build
        /// </remarks>
        public static readonly string[] Locations = 
        {
            ARBlendShapeLocation.BrowDownLeft,
            ARBlendShapeLocation.BrowDownRight,
            ARBlendShapeLocation.BrowInnerUp,
            ARBlendShapeLocation.BrowOuterUpLeft,
            ARBlendShapeLocation.BrowOuterUpRight,
            ARBlendShapeLocation.CheekPuff,
            ARBlendShapeLocation.CheekSquintLeft,
            ARBlendShapeLocation.CheekSquintRight,
            ARBlendShapeLocation.EyeBlinkLeft,
            ARBlendShapeLocation.EyeBlinkRight,
            ARBlendShapeLocation.EyeLookDownLeft,
            ARBlendShapeLocation.EyeLookDownRight,
            ARBlendShapeLocation.EyeLookInLeft,
            ARBlendShapeLocation.EyeLookInRight,
            ARBlendShapeLocation.EyeLookOutLeft,
            ARBlendShapeLocation.EyeLookOutRight,
            ARBlendShapeLocation.EyeLookUpLeft,
            ARBlendShapeLocation.EyeLookUpRight,
            ARBlendShapeLocation.EyeSquintLeft,
            ARBlendShapeLocation.EyeSquintRight,
            ARBlendShapeLocation.EyeWideLeft,
            ARBlendShapeLocation.EyeWideRight,
            ARBlendShapeLocation.JawForward,
            ARBlendShapeLocation.JawLeft,
            ARBlendShapeLocation.JawOpen,
            ARBlendShapeLocation.JawRight,
            ARBlendShapeLocation.MouthClose,
            ARBlendShapeLocation.MouthDimpleLeft,
            ARBlendShapeLocation.MouthDimpleRight,
            ARBlendShapeLocation.MouthFrownLeft,
            ARBlendShapeLocation.MouthFrownRight,
            ARBlendShapeLocation.MouthFunnel,
            ARBlendShapeLocation.MouthLeft,
            ARBlendShapeLocation.MouthLowerDownLeft,
            ARBlendShapeLocation.MouthLowerDownRight,
            ARBlendShapeLocation.MouthPressLeft,
            ARBlendShapeLocation.MouthPressRight,
            ARBlendShapeLocation.MouthPucker,
            ARBlendShapeLocation.MouthRight,
            ARBlendShapeLocation.MouthRollLower,
            ARBlendShapeLocation.MouthRollUpper,
            ARBlendShapeLocation.MouthShrugLower,
            ARBlendShapeLocation.MouthShrugUpper,
            ARBlendShapeLocation.MouthSmileLeft,
            ARBlendShapeLocation.MouthSmileRight,
            ARBlendShapeLocation.MouthStretchLeft,
            ARBlendShapeLocation.MouthStretchRight,
            ARBlendShapeLocation.MouthUpperUpLeft,
            ARBlendShapeLocation.MouthUpperUpRight,
            ARBlendShapeLocation.NoseSneerLeft,
            ARBlendShapeLocation.NoseSneerRight,
#if ARKIT_2_0
            ARBlendShapeLocation.TongueOut,
#endif
        };
        
//        public static string Filter(string value)
//        {
//            return value.ToLower().Replace("_", "");
//        }
        
        public static int GetLocationIndex(this IStreamSettings streamSettings, string location)
        {
            return Array.IndexOf(streamSettings.locations, location);
        }
    }
}