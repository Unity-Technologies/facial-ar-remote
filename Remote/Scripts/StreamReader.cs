using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// This component acts as the hub for using the Facial AR Remote in editor. It is responsible for processing the
    /// stream data from the stream source(s) to be used by connected controllers. It allows you to control the device
    /// connection, and record & playback captured streams to a character.
    /// </summary>
    public class StreamReader : MonoBehaviour, IStreamReader
    {
        [SerializeField]
        [Tooltip("Root of character to be be driven.")]
        GameObject m_Character;

        [SerializeField]
        [Tooltip(" Shows extra debug logging in the console.")]
        bool m_UseDebug;

        [SerializeField]
        [Range(1, 512)]
        [Tooltip("Number of frames of same head tracking data before tracking is considered lost.")]
        int m_TrackingLossPadding = 64;

        [SerializeField]
        [Tooltip("Manually override the blend shape controller found in the Character.")]
        BlendShapesController m_BlendShapesControllerOverride;

        [SerializeField]
        [Tooltip("Manually override the character rig controller found in the Character.")]
        CharacterRigController m_CharacterRigControllerOverride;

        [SerializeField]
        [Tooltip("Manually override the head bone set from the character rig controller. Used for determining the start pose of the character.")]
        Transform m_HeadBoneOverride;

        [SerializeField]
        [Tooltip("Manually override the main camera found by the stream reader. Used for determining the starting pose of the camera.")]
        Camera m_CameraOverride;

        [SerializeField]
        MonoBehaviour[] m_StreamSources = { };

        IStreamSettings m_ActiveStreamSettings;
        IStreamSource m_ActiveStreamSource;

        Transform m_CameraTransform;
        Transform m_HeadBone;

        int m_TrackingLossCount;

        bool m_FaceActive;
        Pose m_CameraPose;
        Pose m_HeadPose;
        Vector3 m_LastHeadPose;

        float[] m_CameraPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        float[] m_HeadPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];

        HashSet<IStreamSource> m_Sources = new HashSet<IStreamSource>();

        event Action<IStreamSettings> streamSettingsChanged;

        public Server server { get; private set; }
        public StreamPlayback streamPlayback { get; private set; }
        public BlendShapesController blendShapesController { get; private set; }
        public CharacterRigController characterRigController { get; private set; }
        public float[] blendShapesBuffer { get; private set; }
        public bool trackingActive { get; private set; }

        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }
        public PlaybackData playbackData { get { return streamPlayback.playbackData; } }

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

                streamSettings = value.streamSettings;

                var blendShapeCount = m_ActiveStreamSettings.BlendShapeCount;
                if (blendShapesBuffer == null || blendShapesBuffer.Length != blendShapeCount)
                    blendShapesBuffer = new float[blendShapeCount];
            }
        }

        public IStreamSettings streamSettings
        {
            get { return m_ActiveStreamSettings; }
            set
            {
                if (value == null)
                    return;

                m_ActiveStreamSettings = value;

                if (m_UseDebug)
                    Debug.Log("StreamSettings Changed");

                if (streamSettingsChanged != null)
                    streamSettingsChanged(value);
            }
        }

        public bool useDebug
        {
            get { return m_UseDebug; }
        }

        public bool active
        {
            get { return streamSource != null && streamSource.active; }
        }

        void OnValidate()
        {
            m_Sources.UnionWith(GetComponentsInChildren<IStreamSource>());
            foreach (var behavior in m_StreamSources)
            {
                var source = behavior as IStreamSource;
                if (source != null)
                    m_Sources.Add(source);
                else
                    Debug.LogWarningFormat("{0} is not a stream source", behavior);
            }

            foreach (var source in m_Sources)
            {
                ConnectInterfaces(source);
                var svr = source as Server;
                if (svr != null)
                    server = svr;

                var playback = source as StreamPlayback;
                if (playback != null)
                    streamPlayback = playback;
            }
        }

        public void UpdateStreamData(ref byte[] buffer, int position)
        {
            var settings = m_ActiveStreamSettings;

            Buffer.BlockCopy(buffer, position + 1, blendShapesBuffer, 0, settings.BlendShapeSize);
            m_FaceActive = buffer[position + settings.BufferSize - 1] == 1;

            Buffer.BlockCopy(buffer, settings.FrameNumberOffset, m_FrameNumArray, 0, settings.FrameNumberSize);
            Buffer.BlockCopy(buffer, settings.FrameTimeOffset, m_FrameTimeArray, 0, settings.FrameTimeSize);

            if (m_UseDebug)
                Debug.Log(string.Format("{0} : {1}", m_FrameNumArray[0], m_FrameTimeArray[0]));

            if (m_FaceActive)
            {
                Buffer.BlockCopy(buffer, position + settings.HeadPoseOffset, m_HeadPoseArray, 0, BlendShapeUtils.PoseSize);
                Buffer.BlockCopy(buffer, position + settings.CameraPoseOffset, m_CameraPoseArray, 0, BlendShapeUtils.PoseSize);
                BlendShapeUtils.ArrayToPose(m_HeadPoseArray, ref m_HeadPose);
                BlendShapeUtils.ArrayToPose(m_CameraPoseArray, ref m_CameraPose);
            }
        }

        public void InitializeStreamReader()
        {
            if (streamPlayback.playbackData == null)
            {
                Debug.LogWarningFormat("No Playback Data set on {0}. You will be unable to record, playback or bake any stream data.",
                    gameObject.name);
            }

            if (m_Character != null)
            {
                blendShapesController = m_BlendShapesControllerOverride != null
                    ? m_BlendShapesControllerOverride
                    : m_Character.GetComponentInChildren<BlendShapesController>();

                characterRigController = m_CharacterRigControllerOverride != null
                    ? m_CharacterRigControllerOverride
                    : m_Character.GetComponentInChildren<CharacterRigController>();

                if (m_HeadBoneOverride == null)
                {
                    if (characterRigController != null)
                        m_HeadBone = characterRigController.headBone;
                }
                else
                {
                    m_HeadBone = m_HeadBoneOverride;
                }
            }
            else
            {
                Debug.Log("Character is not set. Trying to set controllers from overrides.");
                blendShapesController = m_BlendShapesControllerOverride;
                characterRigController = m_CharacterRigControllerOverride;
                m_HeadBone = m_HeadBoneOverride;
            }

            m_CameraTransform = m_CameraOverride == null ? Camera.main.transform : m_CameraOverride.transform;

            if (blendShapesController == null)
                Debug.LogWarning("No Blend Shape Controller has been set or found. Note this data can still be recorded in the stream.");

            if (characterRigController == null)
                Debug.LogWarning("No Character Rig Controller has been set or found. Note this data can still be recorded in the stream.");

            if (m_HeadBone == null)
                Debug.LogWarning("No Head Bone Transform has been set or found. Note this data can still be recorded in the stream.");

            if (m_CameraTransform == null)
                Debug.LogWarning("No Camera has been set or found. Note this data can still be recorded in the stream.");

            if (blendShapesController != null)
                ConnectInterfaces(blendShapesController);

            if (characterRigController != null)
                ConnectInterfaces(characterRigController);
        }

        void OnEnable()
        {
            InitializeStreamReader();
        }

        void Start()
        {
            Application.targetFrameRate = 60;

            if (m_HeadBone == null)
            {
                m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Character head bone set. Using default pose.");
            }
            else
            {
                m_HeadPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
            }

            if (m_CameraTransform == null)
            {
                m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Camera set. Using default pose.");
            }
            else
            {
                m_CameraPose = new Pose(m_CameraTransform.position, m_CameraTransform.rotation);
            }

            streamSource = server;
        }

        void Update()
        {
            if (m_HeadPose.position == m_LastHeadPose)
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

            m_LastHeadPose = m_HeadPose.position;

            streamPlayback.UpdateTimes();

            foreach (var source in m_Sources)
            {
                source.StreamSourceUpdate();
            }
        }

        void FixedUpdate()
        {
            streamPlayback.UpdateTimes();
        }

        void LateUpdate()
        {
            streamPlayback.UpdateTimes();
        }

        void ConnectInterfaces(object obj)
        {
            var usesStreamReader = obj as IUsesStreamReader;
            if (usesStreamReader != null)
            {
                usesStreamReader.streamReader = this;
                var useStreamSettings = obj as IUsesStreamSettings;
                if (useStreamSettings != null)
                    streamSettingsChanged += useStreamSettings.OnStreamSettingsChanged;
            }
        }
    }
}
