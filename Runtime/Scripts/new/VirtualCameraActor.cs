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
        VirtualCameraStateData m_State = new VirtualCameraStateData();
        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        List<GameObject> m_CameraRigs = new List<GameObject>();

        [SerializeField, Range(1,100)]
        float m_MovementScale = 1f;
        
        [SerializeField, Range(1,100)]
        float m_InputScale = 3f;

        bool m_CameraFrozen;
        Pose m_CameraPoseOnFreeze = Pose.identity;
        Pose m_CachedCameraOffset = Pose.identity;
        Pose m_LastRemoteCameraPose = Pose.identity;

        /// <summary>
        /// Set forward/back and strafing from a joystick input
        /// </summary>
        public Vector3 horizontalMoveInput { get; set; }
        
        /// <summary>
        /// Set the vertical position from a joystick input 
        /// </summary>
        public float verticalMoveInput { get; set; }

        public float movementScale
        {
            get => m_MovementScale;
            set => m_MovementScale = value;
        }

        public void SetVirtualCameraState(VirtualCameraStateData data)
        {
            if (m_State == data)
                return;
            
            SetCameraFrozenState(data.frozen);
            SetCameraRig(data.cameraRig);
            SetFocalLength(data.focalLength);

            m_State = data;
        }

        public void SetCameraPose(Pose remoteCameraPose)
        {
            if (m_CameraFrozen)
                return;

            var localInputVector = transform.TransformDirection(horizontalMoveInput);
            Translate(m_InputScale * Time.deltaTime * (localInputVector + Vector3.up * verticalMoveInput));

            Pose trackedPose; 
            trackedPose.position = remoteCameraPose.position * movementScale + m_CachedCameraOffset.position;
            trackedPose.rotation = remoteCameraPose.rotation * m_CachedCameraOffset.rotation;

            transform.localPosition = trackedPose.position;
            transform.localRotation = trackedPose.rotation;

            m_LastRemoteCameraPose = remoteCameraPose;
        }

        /// <summary>
        /// Not to be done while recording.
        /// </summary>
        /// <param name="on"></param>
        void SetCameraFrozenState(bool on)
        {
            if (m_CameraFrozen == on)
                return;

            m_CameraFrozen = on;

            if (on)
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

        void SetCameraRig(CameraRigType cameraRig)
        {
            var cameraRigIndex = (int)cameraRig;
            Debug.Assert(cameraRigIndex < m_CameraRigs.Count);
            Debug.Assert(cameraRigIndex >= 0);

            for (var i = 0; i < m_CameraRigs.Count; i++)
            {
                m_CameraRigs[cameraRigIndex].SetActive(i == cameraRigIndex);
            }
        }

        void SetFocalLength(float focalLength)
        {
            //TODO: Set using ICameraRig interface
            /*
            foreach (var cameraRig in m_CameraRigs)
            {
                cameraRig.m_Lens.FieldOfView = i;
            }
            */
        }
    }
}
