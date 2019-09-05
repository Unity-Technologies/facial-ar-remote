using System;
using PerformanceRecorder;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class VirtualCameraActor : Actor
    {
        [SerializeField]
        VirtualCameraStateData m_State;

        [SerializeField]
        GameObject m_CameraRigManagerGO;

        Pose m_CameraPoseOnFreeze = Pose.identity;
        Pose m_CachedCameraOffset = Pose.identity;
        Pose m_LastPose = Pose.identity;
        
        VirtualCameraStateData m_PrevState;

        IUsesCameraRigData m_CameraRigManager;

        public VirtualCameraStateData state
        {
            get { return m_State; }
        }

        public GameObject cameraRigManagerGo
        {
            get => m_CameraRigManagerGO;
            set
            {
                m_CameraRigManagerGO = value;
                ConnectInterfaces();
            }
        }

        public void SetState(VirtualCameraStateData data)
        {
            if (m_State == data)
                return;

            m_State = data;
            
            UpdateFrozenState();
            UpdateCameraRig();
            UpdateFocalLength();

            m_PrevState = m_State;
        }

        public void SetCameraPose(Pose pose)
        {
            if (m_State.frozen)
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
            if (m_State.frozen == m_PrevState.frozen)
                return;

            if (m_State.frozen)
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
            
            m_CameraRigManager.SetActive(m_State.cameraRig);
        }

        void UpdateFocalLength()
        {
            if (Math.Abs(m_State.focalLength - m_PrevState.focalLength) < Mathf.Epsilon)
                return;

            m_CameraRigManager.SetFocalLength(m_State.focalLength);
        }

        void ConnectInterfaces()
        {
            m_CameraRigManager = cameraRigManagerGo.GetComponent<IUsesCameraRigData>();
        }

        void OnValidate()
        {
            ConnectInterfaces();
            
            UpdateCameraRig();
            UpdateFocalLength();
        }
    }
}
