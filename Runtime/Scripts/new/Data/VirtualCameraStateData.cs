using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PerformanceRecorder
{
    public enum CameraRigType
    {
        HardLock,
        LightDamping,
        MediumDamping,
        HeavyDamping,
        Steadicam
    }

    [Flags]
    public enum AxisLock
    {
        None        = 0,
        Truck       = (1 << 0),
        Dolly       = (1 << 1),
        Pedestal    = (1 << 2),
        Pan         = (1 << 3),
        Tilt        = (1 << 4),
        Dutch       = (1 << 5),
        DutchZero   = (1 << 6)
    }

    public static class AxisLockUtilities
    {
        /// <summary>
        /// Checks if flag has only a single bit set.
        /// </summary>
        /// <param name="flag"></param>
        /// <returns>True if flag has only a single bit set.</returns>
        public static bool IsSingleFlag(AxisLock flag)
        {
            var value = (int)flag;
            return value != 0 && (value & (value-1)) == 0;
        }

        /// <summary>
        /// Checks if flag is set.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="flag"></param>
        /// <returns>True if flags has flag set.</returns>
        public static bool HasFlag(AxisLock flags, AxisLock flag)
        {
            if (flag == AxisLock.None)
                return (int)flags == 0;

            if (!IsSingleFlag(flag))
                throw new ArgumentOutOfRangeException(nameof(flag), flag, null);

            return flags.HasFlag(flag);
        }

        /// <summary>
        /// Sets a flag.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="flag"></param>
        /// <returns>True if changed.</returns>
        public static bool SetFlag(ref AxisLock flags, AxisLock flag)
        {
            var hasFlag = HasFlag(flags, flag);
            flags |= flag;
            return !hasFlag;
        }

        /// <summary>
        /// Clears a flag.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="flag"></param>
        /// <returns>True if changed.</returns>
        public static bool ClearFlag(ref AxisLock flags, AxisLock flag)
        {
            var hasFlag = HasFlag(flags, flag);
            flags &= ~flag;
            return hasFlag;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct VirtualCameraStateData
    {
        public CameraRigType cameraRig;
        public AxisLock axisLock;
        public float focalLength; 
        public bool frozen;
        
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})",
                (object)this.cameraRig.ToString(),
                (object)this.axisLock.ToString(),
                (object)this.focalLength.ToString(),
                (object)this.frozen.ToString());
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", 
                (object) this.cameraRig.ToString(format), 
                (object) this.axisLock.ToString(format),
                (object) this.focalLength.ToString(format),
                (object) this.frozen.ToString());
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is VirtualCameraStateData))
                return false;
            return this.Equals((VirtualCameraStateData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)cameraRig;
                hashCode = (hashCode * 397) ^ (int)axisLock;
                hashCode = (hashCode * 397) ^ (int)focalLength;
                hashCode = (hashCode * 397) ^ frozen.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VirtualCameraStateData other)
        {
            return
                this.cameraRig == other.cameraRig &&
                this.axisLock == other.axisLock &&
                Math.Abs(this.focalLength - other.focalLength) < Mathf.Epsilon &&
                this.frozen == other.frozen;
        }

        public static bool operator ==(VirtualCameraStateData a, VirtualCameraStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(VirtualCameraStateData a, VirtualCameraStateData b)
        {
            return !(a == b);
        }
    }
}
