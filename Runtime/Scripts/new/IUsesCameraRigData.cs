using UnityEngine;

namespace PerformanceRecorder
{
    public interface IUsesCameraRigData
    {
        void SetFocalLength(float focalLength);
        void SetActive(CameraRigType cameraRigType);
    }
}
