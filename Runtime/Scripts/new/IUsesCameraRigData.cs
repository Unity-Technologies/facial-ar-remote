﻿using UnityEngine;

namespace PerformanceRecorder
{
    public interface IUsesCameraRigData
    {
        CameraRigType cameraRigType { get; set; }
        float focalLength { get; set; }
        Vector2 GetSensorSize();
        void SetActive(bool active);
    }
}