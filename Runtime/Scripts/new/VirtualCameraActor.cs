using System;
using System.Collections.Generic;
using PerformanceRecorder;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class VirtualCameraActor : Actor
    {
        [SerializeField]
        VirtualCameraStateData m_StateData;
        
        [SerializeField]
        List<GameObject> m_CameraRigs = new List<GameObject>();

        [SerializeField, Range(1,100)]
        float m_MovementScale = 1f;
        
        [SerializeField, Range(1,100)]
        float m_InputScale = 3f;

        Pose m_CameraPoseOnFreeze = Pose.identity;
        Pose m_CachedCameraOffset = Pose.identity;
        Pose m_LastRemoteCameraPose = Pose.identity;
        
        VirtualCameraStateData m_CachedStateData;

        Vector3 m_HorizontalMoveInput;
        float m_VerticalMoveInput;
        
        List<IUsesCameraRigData> m_ICameraRigs = new List<IUsesCameraRigData>();

        public void SetVirtualCameraState(VirtualCameraStateData data)
        {
            if (m_StateData == data)
                return;

            m_StateData = data;
            
            UpdateFrozenState();
            UpdateCameraRig();
            UpdateFocalLength();

            m_CachedStateData = m_StateData;
        }

        public void SetCameraPose(Pose remoteCameraPose)
        {
            if (m_StateData.frozen)
                return;

            var localInputVector = transform.TransformDirection(m_HorizontalMoveInput);
            Translate(m_InputScale * Time.deltaTime * (localInputVector + Vector3.up * m_VerticalMoveInput));

            Pose trackedPose; 
            trackedPose.position = remoteCameraPose.position * m_MovementScale + m_CachedCameraOffset.position;
            trackedPose.rotation = remoteCameraPose.rotation * m_CachedCameraOffset.rotation;

            transform.localPosition = trackedPose.position;
            transform.localRotation = trackedPose.rotation;

            m_LastRemoteCameraPose = remoteCameraPose;
        }

        /// <summary>
        /// Not to be done while recording.
        /// </summary>
        void UpdateFrozenState()
        {
            if (m_StateData.frozen == m_CachedStateData.frozen)
                return;

            if (m_StateData.frozen)
            {
                m_CameraPoseOnFreeze = new Pose(transform.localPosition, 
                    transform.localRotation);
            }
            else
            {
                m_CachedCameraOffset = new Pose(m_LastRemoteCameraPose.position - m_CameraPoseOnFreeze.position, 
                    Quaternion.Inverse(m_CameraPoseOnFreeze.rotation));
            }
        }
        
        void Translate(Vector3 translateBy)
        {
            m_CachedCameraOffset.position += translateBy;
        }

        void UpdateCameraRig()
        {
            if (m_StateData.cameraRig == m_CachedStateData.cameraRig)
                return;
            
            if (m_CameraRigs.Count == 0)
                return;
            
            if (m_ICameraRigs.Count != m_CameraRigs.Count)
            {
                m_ICameraRigs.Clear();
                foreach (var rig in m_CameraRigs)
                {
                    var cR = rig.GetComponent<IUsesCameraRigData>();
                    m_ICameraRigs.Add(cR);
                }
            }
            
            for (var i = 0; i < m_CameraRigs.Count; i++)
            {
                m_CameraRigs[i].SetActive(m_ICameraRigs[i].cameraRigType == m_StateData.cameraRig);
            }
        }

        void UpdateFocalLength()
        {
            if (Math.Abs(m_StateData.focalLength - m_CachedStateData.focalLength) < Mathf.Epsilon)
                return;
            
            if (m_CameraRigs.Count == 0)
                return;
            
            if (m_ICameraRigs.Count != m_CameraRigs.Count)
            {
                m_ICameraRigs.Clear();
                foreach (var rig in m_CameraRigs)
                {
                    var cR = rig.GetComponent<IUsesCameraRigData>();
                    m_ICameraRigs.Add(cR);
                }
            }
            
            foreach (var rig in m_ICameraRigs)
            {
                rig.focalLength = m_StateData.focalLength;
            }
        }

        void OnValidate()
        {
            UpdateCameraRig();
        }

        void SetAxisLock(AxisLock axisLock)
        {
            /*
            switch (axisLock)
            {
                case AxisLock.Truck:
                    if (on && (data.axisLock & AxisLock.Truck) == 0)
                        data.axisLock |= AxisLock.Truck;
                    else if (!on && (data.axisLock & AxisLock.Truck) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Truck;
                    else
                        noChange = true;
                    break;
                case AxisLock.Dolly:
                    if (on && (data.axisLock & AxisLock.Dolly) == 0)
                        data.axisLock |= AxisLock.Dolly;
                    else if (!on && (data.axisLock & AxisLock.Dolly) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Dolly;
                    else
                        noChange = true;
                    break;
                case AxisLock.Pedestal:
                    if (on && (data.axisLock & AxisLock.Pedestal) == 0)
                        data.axisLock |= AxisLock.Pedestal;
                    else if (!on && (data.axisLock & AxisLock.Pedestal) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Pedestal;
                    else
                        noChange = true;
                    break;
                case AxisLock.Pan:
                    if (on && (data.axisLock & AxisLock.Pedestal) == 0)
                        data.axisLock |= AxisLock.Pedestal;
                    else if (!on && (data.axisLock & AxisLock.Pedestal) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Pedestal;
                    else
                        noChange = true;
                    break;
                case AxisLock.Tilt:
                    if (on && (data.axisLock & AxisLock.Pedestal) == 0)
                        data.axisLock |= AxisLock.Pedestal;
                    else if (!on && (data.axisLock & AxisLock.Pedestal) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Pedestal;
                    else
                        noChange = true;
                    break;
                case AxisLock.Dutch:
                    if (on && (data.axisLock & AxisLock.Pedestal) == 0)
                        data.axisLock |= AxisLock.Pedestal;
                    else if (!on && (data.axisLock & AxisLock.Pedestal) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Pedestal;
                    else
                        noChange = true;
                    break;
                case AxisLock.DutchZero:
                    if (on && (data.axisLock & AxisLock.Pedestal) == 0)
                        data.axisLock |= AxisLock.Pedestal;
                    else if (!on && (data.axisLock & AxisLock.Pedestal) != 0)
                        m_VirtualCameraStateData.axisLock &= ~AxisLock.Pedestal;
                    else
                        noChange = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axisLock), axisLock, null);
            }

            m_CachedAxisLock = axisLock;
            */
        }
    }
}
