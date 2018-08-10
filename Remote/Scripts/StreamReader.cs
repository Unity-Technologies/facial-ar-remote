using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IStreamReader" />
    /// <summary>
    /// This component acts as the hub for using the Facial AR Remote in editor. It is responsible for processing the
    /// stream data from the stream source(s) to be used by connected controllers. It allows you to control the device
    /// connection, and record and playback captured streams to a character.
    /// </summary>
    public class StreamReader : MonoBehaviour, IStreamReader
    {
        [SerializeField]
        [Tooltip("Root of character to be be driven.")]
        GameObject m_Character;

        [SerializeField]
        [Tooltip("Shows extra logging information in the console.")]
        bool m_VerboseLogging;

        [SerializeField]
        [Range(1, 512)]
        [Tooltip("Number of frames of same head tracking data before tracking is considered lost.")]
        int m_TrackingLossPadding = 64;

        [SerializeField]
        [Tooltip("(Optional) Manually override the blend shape controller found in the Character.")]
        BlendShapesController m_BlendShapesControllerOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the character rig controller found in the Character.")]
        CharacterRigController m_CharacterRigControllerOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the head bone set from the character rig controller. Used for determining the start pose of the character.")]
        Transform m_HeadBoneOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the main camera found by the stream reader. Used for determining the starting pose of the camera.")]
        Camera m_CameraOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually add stream sources which aren't on this GameObject or its children.")]
        GameObject[] m_StreamSourceOverrides = { };

        IStreamSource m_ActiveStreamSource;

        int m_TrackingLossCount;

        bool m_FaceActive;
        Pose m_CameraPose;
        Pose m_HeadPose;
        Vector3 m_LastHeadPosition;

        float[] m_CameraPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        float[] m_HeadPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];

        HashSet<IStreamSource> m_Sources = new HashSet<IStreamSource>();

        public float[] blendShapesBuffer { get; private set; }
        public bool trackingActive { get; private set; }

        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }
        public bool verboseLogging { get { return m_VerboseLogging; } }
        public Transform headBone { get; private set; }
        public Transform cameraTransform { get; private set; }
        public HashSet<IStreamSource> sources { get { return m_Sources; } }
        public BlendShapesController blendShapesController { get; private set; }
        public CharacterRigController characterRigController { get; private set; }

        public IStreamSource streamSource
        {
            get { return m_ActiveStreamSource; }
            set
            {
                if (m_ActiveStreamSource == value)
                    return;

                m_ActiveStreamSource = value;

                if (value == null)
                    return;

                var blendShapeCount = value.streamSettings.BlendShapeCount;
                if (blendShapesBuffer == null || blendShapesBuffer.Length != blendShapeCount)
                    blendShapesBuffer = new float[blendShapeCount];
            }
        }

        public void UpdateStreamData(byte[] buffer, int offset = 0)
        {
            var settings = streamSource.streamSettings;

            Buffer.BlockCopy(buffer, offset + 1, blendShapesBuffer, 0, settings.BlendShapeSize);
            m_FaceActive = buffer[offset + settings.bufferSize - 1] == 1;

            Buffer.BlockCopy(buffer, settings.FrameNumberOffset, m_FrameNumArray, 0, settings.FrameNumberSize);
            Buffer.BlockCopy(buffer, settings.FrameTimeOffset, m_FrameTimeArray, 0, settings.FrameTimeSize);

            if (m_VerboseLogging)
                Debug.Log(string.Format("{0} : {1}", m_FrameNumArray[0], m_FrameTimeArray[0]));

            if (m_FaceActive)
            {
                Buffer.BlockCopy(buffer, offset + settings.HeadPoseOffset, m_HeadPoseArray, 0, BlendShapeUtils.PoseSize);
                Buffer.BlockCopy(buffer, offset + settings.CameraPoseOffset, m_CameraPoseArray, 0, BlendShapeUtils.PoseSize);
                BlendShapeUtils.ArrayToPose(m_HeadPoseArray, ref m_HeadPose);
                BlendShapeUtils.ArrayToPose(m_CameraPoseArray, ref m_CameraPose);
            }
        }

        public void ConnectDependencies()
        {
            m_Sources.UnionWith(GetComponentsInChildren<IStreamSource>());
            foreach (var go in m_StreamSourceOverrides)
            {
                m_Sources.UnionWith(go.GetComponentsInChildren<IStreamSource>());
            }

            foreach (var source in m_Sources)
            {
                ConnectInterfaces(source);
                if (source is NetworkStream)
                    streamSource = source;
            }

            if (m_Character != null)
            {
                blendShapesController = m_BlendShapesControllerOverride != null
                    ? m_BlendShapesControllerOverride
                    : m_Character.GetComponentInChildren<BlendShapesController>();

                characterRigController = m_CharacterRigControllerOverride != null
                    ? m_CharacterRigControllerOverride
                    : m_Character.GetComponentInChildren<CharacterRigController>();

                headBone = m_HeadBoneOverride != null
                    ? m_HeadBoneOverride
                    : characterRigController != null
                        ? characterRigController.headBone
                        : null;
            }
            else
            {
                blendShapesController = m_BlendShapesControllerOverride;
                characterRigController = m_CharacterRigControllerOverride;
                headBone = m_HeadBoneOverride;
            }

            cameraTransform = m_CameraOverride == null ? Camera.main.transform : m_CameraOverride.transform;

            if (blendShapesController != null)
                ConnectInterfaces(blendShapesController);

            if (characterRigController != null)
                ConnectInterfaces(characterRigController);
        }

        void Start()
        {
            Application.targetFrameRate = 60;

            if (headBone == null)
            {
                m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Character head bone set. Using default pose.");
            }
            else
            {
                m_HeadPose = new Pose(headBone.position, headBone.rotation);
            }

            if (cameraTransform == null)
            {
                m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Camera set. Using default pose.");
            }
            else
            {
                m_CameraPose = new Pose(cameraTransform.position, cameraTransform.rotation);
            }

            ConnectDependencies();
        }

        void Update()
        {
            var headPosition = m_HeadPose.position;
            if (headPosition == m_LastHeadPosition)
            {
                m_TrackingLossCount++;
                if (!m_FaceActive && m_TrackingLossCount > m_TrackingLossPadding)
                    trackingActive = false;
                else
                    trackingActive = true;
            }
            else
            {
                m_TrackingLossCount = 0;
            }

            m_LastHeadPosition = headPosition;

            foreach (var source in m_Sources)
            {
                source.StreamSourceUpdate();
            }
        }

        void ConnectInterfaces(object obj)
        {
            var usesStreamReader = obj as IUsesStreamReader;
            if (usesStreamReader != null)
                usesStreamReader.streamReader = this;
        }
    }
}
