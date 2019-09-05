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
        GameObject m_CameraRigManagerGO;

        [SerializeField, Range(1,100)]
        float m_InputScale = 3f;

        Pose m_CameraPoseOnFreeze = Pose.identity;
        Pose m_CachedCameraOffset = Pose.identity;
        Pose m_LastPose = Pose.identity;
        
        VirtualCameraStateData m_CachedStateData;

        IUsesCameraRigData m_CameraRigManager;

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

        public void SetCameraPose(Pose pose)
        {
            if (m_StateData.frozen)
                return;

            m_LastPose = pose;

            //var localInputVector = transform.TransformDirection(m_HorizontalMoveInput);
            //Translate(m_InputScale * Time.deltaTime * (localInputVector + Vector3.up * m_VerticalMoveInput));

            pose.position = pose.position + m_CachedCameraOffset.position;
            pose.rotation = pose.rotation * m_CachedCameraOffset.rotation;

            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
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
                m_CachedCameraOffset = new Pose(m_LastPose.position - m_CameraPoseOnFreeze.position, 
                    Quaternion.Inverse(m_CameraPoseOnFreeze.rotation));
            }
        }
        
        void Translate(Vector3 translateBy)
        {
            m_CachedCameraOffset.position += translateBy;
        }

        void UpdateCameraRig()
        {
            //if (m_StateData.cameraRig == m_CachedStateData.cameraRig)
            //    return;
            
            m_CameraRigManager.SetActive(m_StateData.cameraRig);
        }

        void UpdateFocalLength()
        {
            if (Math.Abs(m_StateData.focalLength - m_CachedStateData.focalLength) < Mathf.Epsilon)
                return;

            m_CameraRigManager.SetFocalLength(m_StateData.focalLength);
        }

        void ConnectInterfaces()
        {
            m_CameraRigManager = m_CameraRigManagerGO.GetComponent<IUsesCameraRigData>();
        }

        void OnValidate()
        {
            ConnectInterfaces();
            
            UpdateCameraRig();
            UpdateFocalLength();
        }
    }
}
