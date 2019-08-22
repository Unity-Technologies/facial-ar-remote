using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public enum BlendShapeLocation
    {
        BrowDownLeft = 0,
        BrowDownRight = 1,
        BrowInnerUp = 2,
        BrowOuterUpLeft = 3,
        BrowOuterUpRight = 4,
        CheekPuff = 5,
        CheekSquintLeft = 6,
        CheekSquintRight = 7,
        EyeBlinkLeft = 8,
        EyeBlinkRight = 9,
        EyeLookDownLeft = 10,
        EyeLookDownRight = 11,
        EyeLookInLeft = 12,
        EyeLookInRight = 13,
        EyeLookOutLeft = 14,
        EyeLookOutRight = 15,
        EyeLookUpLeft = 16,
        EyeLookUpRight = 17,
        EyeSquintLeft = 18,
        EyeSquintRight = 19,
        EyeWideLeft = 20,
        EyeWideRight = 21,
        JawForward = 22,
        JawLeft = 23,
        JawOpen = 24,
        JawRight = 25,
        MouthClose = 26,
        MouthDimpleLeft = 27,
        MouthDimpleRight = 28,
        MouthFrownLeft = 29,
        MouthFrownRight = 30,
        MouthFunnel = 31,
        MouthLeft = 32,
        MouthLowerDownLeft = 33,
        MouthLowerDownRight = 34,
        MouthPressLeft = 35,
        MouthPressRight = 36,
        MouthPucker = 37,
        MouthRight = 38,
        MouthRollLower = 39,
        MouthRollUpper = 40,
        MouthShrugLower = 41,
        MouthShrugUpper = 42,
        MouthSmileLeft = 43,
        MouthSmileRight = 44,
        MouthStretchLeft = 45,
        MouthStretchRight = 46,
        MouthUpperUpLeft = 47,
        MouthUpperUpRight = 48,
        NoseSneerLeft = 49,
        NoseSneerRight = 50,
        TongueOut = 51,
        Invalid = 0x7FFFFFFF
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlendShapeValues
    {
        public float BrowDownLeft;
        public float BrowDownRight;
        public float BrowInnerUp;
        public float BrowOuterUpLeft;
        public float BrowOuterUpRight;
        public float CheekPuff;
        public float CheekSquintLeft;
        public float CheekSquintRight;
        public float EyeBlinkLeft;
        public float EyeBlinkRight;
        public float EyeLookDownLeft;
        public float EyeLookDownRight;
        public float EyeLookInLeft;
        public float EyeLookInRight;
        public float EyeLookOutLeft;
        public float EyeLookOutRight;
        public float EyeLookUpLeft;
        public float EyeLookUpRight;
        public float EyeSquintLeft;
        public float EyeSquintRight;
        public float EyeWideLeft;
        public float EyeWideRight;
        public float JawForward;
        public float JawLeft;
        public float JawOpen;
        public float JawRight;
        public float MouthClose;
        public float MouthDimpleLeft;
        public float MouthDimpleRight;
        public float MouthFrownLeft;
        public float MouthFrownRight;
        public float MouthFunnel;
        public float MouthLeft;
        public float MouthLowerDownLeft;
        public float MouthLowerDownRight;
        public float MouthPressLeft;
        public float MouthPressRight;
        public float MouthPucker;
        public float MouthRight;
        public float MouthRollLower;
        public float MouthRollUpper;
        public float MouthShrugLower;
        public float MouthShrugUpper;
        public float MouthSmileLeft;
        public float MouthSmileRight;
        public float MouthStretchLeft;
        public float MouthStretchRight;
        public float MouthUpperUpLeft;
        public float MouthUpperUpRight;
        public float NoseSneerLeft;
        public float NoseSneerRight;
        public float TongueOut;

        public static int count
        {
            get { return 52; }
        }

        public float this[int index]
        {
            get { return GetValue(index); }
            set { SetValue(index, value); }
        }

        public float GetValue(BlendShapeLocation location)
        {
            return GetValue((int)location);
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

        public void SetValue(BlendShapeLocation location, float value)
        {
            SetValue((int)location, value);
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

        public static BlendShapeValues Lerp(ref BlendShapeValues values1, ref BlendShapeValues values2, float t)
        {
            var values = new BlendShapeValues();

            for (var i = 0; i < count; ++i)
                values[i] = Mathf.Lerp(values1[i], values2[i], t);
            
            return values;
        }
    }
}
