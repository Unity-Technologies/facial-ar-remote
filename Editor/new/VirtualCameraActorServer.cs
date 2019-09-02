using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class VirtualCameraActorServer : ActorServer
    {
        PoseDataRecorder m_PoseDataRecorder = new PoseDataRecorder();

        public VirtualCameraActorServer() : base()
        {
            AddRecorder(m_PoseDataRecorder);
            
            GetReader().poseDataChanged += PoseDataChanged;
        }

        void PoseDataChanged(PoseData data)
        {
            var virtualCamera = actor as VirtualCameraActor;

            if (virtualCamera == null)
                return;

            if (state != PreviewState.LiveStream)
                return;

            virtualCamera.SetCameraPose(data.pose);

            if (IsRecording())
                m_PoseDataRecorder.Record(data);
        }
    }
}
