using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public enum BlendShapeLocation
    {
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
    }


    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct BlendShapeValues
    {
        [FieldOffset(0)] public float BrowDownLeft;
        [FieldOffset(4)] public float BrowDownRight;
        [FieldOffset(8)] public float BrowInnerUp;
        [FieldOffset(12)] public float BrowOuterUpLeft;
        [FieldOffset(16)] public float BrowOuterUpRight;
        [FieldOffset(20)] public float CheekPuff;
        [FieldOffset(24)] public float CheekSquintLeft;
        [FieldOffset(28)] public float CheekSquintRight;
        [FieldOffset(32)] public float EyeBlinkLeft;
        [FieldOffset(36)] public float EyeBlinkRight;
        [FieldOffset(40)] public float EyeLookDownLeft;
        [FieldOffset(44)] public float EyeLookDownRight;
        [FieldOffset(48)] public float EyeLookInLeft;
        [FieldOffset(52)] public float EyeLookInRight;
        [FieldOffset(56)] public float EyeLookOutLeft;
        [FieldOffset(60)] public float EyeLookOutRight;
        [FieldOffset(64)] public float EyeLookUpLeft;
        [FieldOffset(68)] public float EyeLookUpRight;
        [FieldOffset(72)] public float EyeSquintLeft;
        [FieldOffset(76)] public float EyeSquintRight;
        [FieldOffset(80)] public float EyeWideLeft;
        [FieldOffset(84)] public float EyeWideRight;
        [FieldOffset(88)] public float JawForward;
        [FieldOffset(92)] public float JawLeft;
        [FieldOffset(96)] public float JawOpen;
        [FieldOffset(100)] public float JawRight;
        [FieldOffset(104)] public float MouthClose;
        [FieldOffset(108)] public float MouthDimpleLeft;
        [FieldOffset(112)] public float MouthDimpleRight;
        [FieldOffset(116)] public float MouthFrownLeft;
        [FieldOffset(120)] public float MouthFrownRight;
        [FieldOffset(124)] public float MouthFunnel;
        [FieldOffset(128)] public float MouthLeft;
        [FieldOffset(132)] public float MouthLowerDownLeft;
        [FieldOffset(136)] public float MouthLowerDownRight;
        [FieldOffset(140)] public float MouthPressLeft;
        [FieldOffset(144)] public float MouthPressRight;
        [FieldOffset(148)] public float MouthPucker;
        [FieldOffset(152)] public float MouthRight;
        [FieldOffset(156)] public float MouthRollLower;
        [FieldOffset(160)] public float MouthRollUpper;
        [FieldOffset(164)] public float MouthShrugLower;
        [FieldOffset(168)] public float MouthShrugUpper;
        [FieldOffset(172)] public float MouthSmileLeft;
        [FieldOffset(176)] public float MouthSmileRight;
        [FieldOffset(180)] public float MouthStretchLeft;
        [FieldOffset(184)] public float MouthStretchRight;
        [FieldOffset(188)] public float MouthUpperUpLeft;
        [FieldOffset(192)] public float MouthUpperUpRight;
        [FieldOffset(196)] public float NoseSneerLeft;
        [FieldOffset(200)] public float NoseSneerRight;
        [FieldOffset(204)] public float TongueOut;

        public float this[int index]
        {
            get { return GetValue(index); }
            set { SetValue(index, value); }
        }

        float GetValue(int index)
        {
            switch (index)
            {
                case 0: return BrowDownLeft;
                case 1: return BrowDownRight;
                case 2: return BrowInnerUp;
                case 3: return BrowOuterUpLeft;
                case 4: return BrowOuterUpRight;
                case 5: return CheekPuff;
                case 6: return CheekSquintLeft;
                case 7: return CheekSquintRight;
                case 8: return EyeBlinkLeft;
                case 9: return EyeBlinkRight;
                case 10: return EyeLookDownLeft;
                case 11: return EyeLookDownRight;
                case 12: return EyeLookInLeft;
                case 13: return EyeLookInRight;
                case 14: return EyeLookOutLeft;
                case 15: return EyeLookOutRight;
                case 16: return EyeLookUpLeft;
                case 17: return EyeLookUpRight;
                case 18: return EyeSquintLeft;
                case 19: return EyeSquintRight;
                case 20: return EyeWideLeft;
                case 21: return EyeWideRight;
                case 22: return JawForward;
                case 23: return JawLeft;
                case 24: return JawOpen;
                case 25: return JawRight;
                case 26: return MouthClose;
                case 27: return MouthDimpleLeft;
                case 28: return MouthDimpleRight;
                case 29: return MouthFrownLeft;
                case 30: return MouthFrownRight;
                case 31: return MouthFunnel;
                case 32: return MouthLeft;
                case 33: return MouthLowerDownLeft;
                case 34: return MouthLowerDownRight;
                case 35: return MouthPressLeft;
                case 36: return MouthPressRight;
                case 37: return MouthPucker;
                case 38: return MouthRight;
                case 39: return MouthRollLower;
                case 40: return MouthRollUpper;
                case 41: return MouthShrugLower;
                case 42: return MouthShrugUpper;
                case 43: return MouthSmileLeft;
                case 44: return MouthSmileRight;
                case 45: return MouthStretchLeft;
                case 46: return MouthStretchRight;
                case 47: return MouthUpperUpLeft;
                case 48: return MouthUpperUpRight;
                case 49: return NoseSneerLeft;
                case 50: return NoseSneerRight;
                case 51: return TongueOut;
                default:
                    throw new IndexOutOfRangeException("Invalid index!");
            }
        }

        void SetValue(int index, float value)
        {
            switch (index)
            {
                case 0: BrowDownLeft = value; break;
                case 1: BrowDownRight = value; break;
                case 2: BrowInnerUp = value; break;
                case 3: BrowOuterUpLeft = value; break;
                case 4: BrowOuterUpRight = value; break;
                case 5: CheekPuff = value; break;
                case 6: CheekSquintLeft = value; break;
                case 7: CheekSquintRight = value; break;
                case 8: EyeBlinkLeft = value; break;
                case 9: EyeBlinkRight = value; break;
                case 10: EyeLookDownLeft = value; break;
                case 11: EyeLookDownRight = value; break;
                case 12: EyeLookInLeft = value; break;
                case 13: EyeLookInRight = value; break;
                case 14: EyeLookOutLeft = value; break;
                case 15: EyeLookOutRight = value; break;
                case 16: EyeLookUpLeft = value; break;
                case 17: EyeLookUpRight = value; break;
                case 18: EyeSquintLeft = value; break;
                case 19: EyeSquintRight = value; break;
                case 20: EyeWideLeft = value; break;
                case 21: EyeWideRight = value; break;
                case 22: JawForward = value; break;
                case 23: JawLeft = value; break;
                case 24: JawOpen = value; break;
                case 25: JawRight = value; break;
                case 26: MouthClose = value; break;
                case 27: MouthDimpleLeft = value; break;
                case 28: MouthDimpleRight = value; break;
                case 29: MouthFrownLeft = value; break;
                case 30: MouthFrownRight = value; break;
                case 31: MouthFunnel = value; break;
                case 32: MouthLeft = value; break;
                case 33: MouthLowerDownLeft = value; break;
                case 34: MouthLowerDownRight = value; break;
                case 35: MouthPressLeft = value; break;
                case 36: MouthPressRight = value; break;
                case 37: MouthPucker = value; break;
                case 38: MouthRight = value; break;
                case 39: MouthRollLower = value; break;
                case 40: MouthRollUpper = value; break;
                case 41: MouthShrugLower = value; break;
                case 42: MouthShrugUpper = value; break;
                case 43: MouthSmileLeft = value; break;
                case 44: MouthSmileRight = value; break;
                case 45: MouthStretchLeft = value; break;
                case 46: MouthStretchRight = value; break;
                case 47: MouthUpperUpLeft = value; break;
                case 48: MouthUpperUpRight = value; break;
                case 49: NoseSneerLeft = value; break;
                case 50: NoseSneerRight = value; break;
                case 51: TongueOut = value; break;
                default:
                    throw new IndexOutOfRangeException("Invalid index!");
            }
        }
    }
}
