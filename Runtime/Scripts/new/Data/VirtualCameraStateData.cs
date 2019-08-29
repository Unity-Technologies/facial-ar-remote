using System.Runtime.InteropServices;
using UnityEditor;
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

    public enum AxisLock
    {
        Truck       = (1 << 0), //1
        Dolly       = (1 << 1), //2
        Pedestal    = (1 << 2), //4
        Pan         = (1 << 3), //8
        Tilt        = (1 << 4), //16
        Dutch       = (1 << 5), //32
        DutchZero   = (1 << 6)  //64
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualCameraStateData
    {
        public CameraRig cameraRig;
        public AxisLock axisLock;
        public int focalLength; 
        public bool frozen;
        public bool recording;
        
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
                hashCode = (hashCode * 397) ^ recording.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VirtualCameraStateData other)
        {
            return
                this.cameraRig == other.cameraRig &&
                this.axisLock == other.axisLock &&
                this.focalLength == other.focalLength &&
                this.frozen == other.frozen &&
                this.recording == other.recording;
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
