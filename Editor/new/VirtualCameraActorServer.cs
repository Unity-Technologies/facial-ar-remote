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
            GetReader().virtualCameraStateChanged += OnVirtualCameraStateChanged;
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

        void OnVirtualCameraStateChanged(VirtualCameraStateData data)
        {
            Debug.Log(data);

            var virtualCamera = actor as VirtualCameraActor;

            if (virtualCamera == null)
                return;

            virtualCamera.SetState(data);

            SendActorServerChanged();
        }

        public override void OnGUI()
        {
            var virtualCamera = actor as VirtualCameraActor;
            var state = virtualCamera.state;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                state.axisLock = (AxisLock)EditorGUILayout.EnumFlagsField("Axis Lock", state.axisLock);
                state.cameraRig = (CameraRigType)EditorGUILayout.EnumPopup("Camera Rig", state.cameraRig);
                state.focalLength = EditorGUILayout.FloatField("Focal Length", state.focalLength);
                state.frozen = EditorGUILayout.Toggle("Frozen", state.frozen);

                if (check.changed)
                {
                    virtualCamera.SetState(state);

                    if (IsClientConnected())
                        GetWriter().Write(state);
                }
            }
        }
    }
}
