using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PerformanceRecorder
{
    public enum CameraRig
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
        Nothing     = (1 << 0),
        Truck       = (1 << 1),
        Dolly       = (1 << 2),
        Pedestal    = (1 << 3),
        Pan         = (1 << 4),
        Tilt        = (1 << 5),
        Dutch       = (1 << 6),
        DutchZero   = (1 << 7)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualCameraStateData
    {
        public CameraRig cameraRig;
        public AxisLock axisLock;
        public int focalLength; 
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
                hashCode = (hashCode * 397) ^ focalLength;
                hashCode = (hashCode * 397) ^ frozen.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VirtualCameraStateData other)
        {
            return
                this.cameraRig == other.cameraRig &&
                this.axisLock == other.axisLock &&
                this.focalLength == other.focalLength &&
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
