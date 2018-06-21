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
    public class StreamReader : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        [Tooltip("Contains the buffer layout and blend shape name and mapping information for interpreting the data stream from a connected device.")]
        StreamSettings m_StreamSettings;

        [SerializeField]
        [Tooltip("Contains the individual streams recorded from a capture session.")]
        PlaybackData m_PlaybackData;

        [SerializeField]
        [Tooltip("Root of character to be be driven.")]
        GameObject m_Character;

        [SerializeField]
        [Tooltip(" Shows extra debug logging in the console.")]
        bool m_UseDebug;

        [Header("Server Settings")]
        [SerializeField]
        [Tooltip("Port number that the device will connect to, be sure to have this match the port set on the device.")]
        int m_Port = 9000;

        [SerializeField]
        [Tooltip("How many frames should be processed at once if the editor falls behind processing the device stream. In an active recording these frames are still captured even if they are skipped in editor.")]
        int m_CatchupSize = 2;

        [SerializeField]
        [Range(1, 512)]
        [Tooltip("Number of frames of same head tracking data before tracking is considered lost.")]
        int m_TrackingLossPadding = 64;

        [Header("Controller Settings")]
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

        BlendShapesController m_BlendShapesController;
        CharacterRigController m_CharacterRigController;
        Camera m_Camera;
        Transform m_HeadBone;

        Pose m_HeadPose;
        Pose m_CameraPose;

        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];

        IStreamSource m_ActiveStreamSource;
        IStreamSettings m_ActiveStreamSettings;

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        HashSet<IStreamSource> m_StreamSources = new HashSet<IStreamSource>();

        Server m_Server;
        StreamPlayback m_StreamPlayback;
        Action m_OnStreamSettingsChange;

        public bool faceActive { get; private set; }
        public bool trackingActive { get; private set; }
        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }

        public PlaybackData playbackData { get { return m_PlaybackData; } }
        public BlendShapesController blendShapesController { get { return m_BlendShapesController; } }
        public CharacterRigController characterRigController { get { return m_CharacterRigController; } }

        public bool streamActive
        {
            get { return enabled && m_ActiveStreamSource != null && m_ActiveStreamSource.streamActive; }
        }

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }
        public float[] headPoseArray { get { return m_HeadPoseArray; } }
        public float[] cameraPoseArray { get { return m_CameraPoseArray; } }

        float[] m_BlendShapesBuffer;
        float[] m_HeadPoseArray = new float[7];
        float[] m_CameraPoseArray = new float[7];

        public void UseStreamReaderSettings()
        {
            if (m_ActiveStreamSettings.Equals(m_StreamSettings))
                return;

            SetActiveStreamSettings(m_StreamSettings);
        }

        public void SetActiveStreamSettings(IStreamSettings settings)
        {
            if (settings == null)
                return;

            m_ActiveStreamSettings = settings;
            m_OnStreamSettingsChange.Invoke();
        }

        public void SetStreamSource(IStreamSource source)
        {
            if (source == null)
                return;
            m_ActiveStreamSource = source;
            source.SetReaderStreamSettings();
            m_BlendShapesBuffer = new float[m_ActiveStreamSettings.BlendShapeCount];
        }

        public Server server
        {
            get
            {
                if (m_Server == null)
                    InitializeStreamReader();

                return m_Server;
            }
        }

        public StreamPlayback streamPlayback
        {
            get
            {
                if (m_StreamPlayback == null)
                    InitializeStreamReader();
                return m_StreamPlayback;
            }
        }

        public void UnSetStreamSource()
        {
            m_ActiveStreamSource = null;
        }

        public void UpdateStreamData(ref byte[] buffer, int position)
        {
            var streamSettings = m_ActiveStreamSettings;

            Buffer.BlockCopy(buffer, position + 1, m_BlendShapesBuffer, 0, streamSettings.BlendShapeSize);
            Buffer.BlockCopy(buffer, position + streamSettings.HeadPoseOffset, m_HeadPoseArray, 0, streamSettings.PoseSize);
            Buffer.BlockCopy(buffer, position + streamSettings.CameraPoseOffset, m_CameraPoseArray, 0, streamSettings.PoseSize);
            faceActive = buffer[position + streamSettings.BufferSize - 1] == 1;

            Buffer.BlockCopy(buffer, streamSettings.FrameNumberOffset, m_FrameNumArray, 0, streamSettings.FrameNumberSize);
            Buffer.BlockCopy(buffer, streamSettings.FrameTimeOffset, m_FrameTimeArray, 0, streamSettings.FrameTimeSize);

            if (m_UseDebug)
                Debug.Log(string.Format("{0} : {1}", m_FrameNumArray[0], m_FrameTimeArray[0]));

            if (faceActive)
            {
                BlendShapeUtils.ArrayToPose(headPoseArray, ref m_HeadPose);
                BlendShapeUtils.ArrayToPose(cameraPoseArray, ref m_CameraPose);
            }
        }

        public void InitializeStreamReader()
        {
            if (m_StreamSettings == null)
            {
                Debug.LogErrorFormat("No Stream Setting set on {0}! Unable to run Stream Reader!", gameObject.name);
                enabled = false;
            }

            if (m_PlaybackData == null)
            {
                Debug.LogWarningFormat("No Playback Data set on {0}. You will be unable to record, playback or bake any stream data.",
                    gameObject.name);
            }

            if (m_Character != null)
            {
                if (m_BlendShapesControllerOverride == null)
                    m_BlendShapesController = m_Character.GetComponentInChildren<BlendShapesController>();
                else
                    m_BlendShapesController = m_BlendShapesControllerOverride;

                if (m_CharacterRigControllerOverride == null)
                    m_CharacterRigController = m_Character.GetComponentInChildren<CharacterRigController>();
                else
                    m_CharacterRigController = m_CharacterRigControllerOverride;

                if (m_HeadBoneOverride == null)
                {
                    if (m_CharacterRigController != null)
                        m_HeadBone = m_CharacterRigController.headBone;
                }
                else
                    m_HeadBone = m_HeadBoneOverride;
            }
            else
            {
                Debug.Log("Character is not set. Trying to set controllers from overrides.");
                m_BlendShapesController = m_BlendShapesControllerOverride;
                m_CharacterRigController = m_CharacterRigControllerOverride;
                m_HeadBone = m_HeadBoneOverride;
            }

            if (m_CameraOverride == null)
                m_Camera = Camera.main;
            else
                m_Camera = m_CameraOverride;

            if (m_BlendShapesController == null)
            {
                Debug.LogWarning("No Blend Shape Controller has been set or found. Note this data can still be recorded in the stream.");
            }

            if (m_CharacterRigController == null)
            {
                Debug.LogWarning("No Character Rig Controller has been set or found. Note this data can still be recorded in the stream.");
            }

            if (m_HeadBone == null)
            {
                Debug.LogWarning("No Head Bone Transform has been set or found. Note this data can still be recorded in the stream.");
            }

            if (m_Camera == null)
            {
                Debug.LogWarning("No Camera has been set or found. Note this data can still be recorded in the stream.");
            }

            m_OnStreamSettingsChange = () =>
            {
                if (m_UseDebug)
                    Debug.Log("OnStreamSettingsChange");
            };

            m_Server = new Server();
            ConnectInterfaces(m_Server);

            m_StreamPlayback = new StreamPlayback();
            ConnectInterfaces(m_StreamPlayback);

            // TODO switch this to a single character ref after Demo
            if (m_BlendShapesController != null)
            {
                ConnectInterfaces(m_BlendShapesController);
                m_BlendShapesController.connected = true;
            }

            if (m_CharacterRigController != null)
            {
                ConnectInterfaces(m_CharacterRigController);
                m_CharacterRigController.connected = true;
            }

            m_StreamSources.Add(m_Server);
            m_StreamSources.Add(m_StreamPlayback);

            SetActiveStreamSettings(m_StreamSettings);
        }

        void Awake()
        {
            InitializeStreamReader();
        }

        void Start()
        {
            Application.targetFrameRate = 120;

            if (m_HeadBone == null)
            {
                m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Character head bone set. Using default pose.");
            }
            else
            {
                m_HeadPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
            }

            if (m_Camera == null)
            {
                m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);
                Debug.LogWarning("No Camera set. Using default pose.");
            }
            else
            {
                m_CameraPose = new Pose(m_Camera.transform.position, m_Camera.transform.rotation);
            }

            foreach (var streamSource in m_StreamSources)
            {
                streamSource.StartStreamThread();
            }

            m_Server.ActivateStreamSource();
        }

        void Update()
        {
            if (m_HeadPose.position == m_LastPose)
            {
                m_TrackingLossCount++;
                if (!faceActive && m_TrackingLossCount > m_TrackingLossPadding)
                    trackingActive = false;
                else
                    trackingActive = true;
            }
            else
            {
                m_TrackingLossCount = 0;
            }
            m_LastPose = m_HeadPose.position;

            m_StreamPlayback.UpdateTimes();

            m_Server.StreamSourceUpdate();
            m_StreamPlayback.StreamSourceUpdate();
        }

        void FixedUpdate()
        {
            m_StreamPlayback.UpdateTimes();
        }

        void LateUpdate()
        {
            m_StreamPlayback.UpdateTimes();
        }

        void OnDisable()
        {
            foreach (var streamSource in m_StreamSources)
            {
                streamSource.DeactivateStreamSource();
            }
        }

        void OnDestroy()
        {
            foreach (var streamSource in m_StreamSources)
            {
                streamSource.streamThreadActive = false;
            }
            m_StreamSources.Clear();
        }

        void ConnectInterfaces(object obj)
        {
            var streamSource = obj as IStreamSource;
            if (streamSource != null)
            {
                streamSource.getStreamReader = () => this;
                streamSource.IsStreamSource = () => m_ActiveStreamSource == streamSource;
                streamSource.getPlaybackData = () => m_PlaybackData;
                streamSource.getUseDebug = () => m_UseDebug;
            }

            var useStreamSettings = obj as IUseStreamSettings;
            if (useStreamSettings != null)
            {
                useStreamSettings.getStreamSettings = () => m_ActiveStreamSettings;
                useStreamSettings.getReaderStreamSettings = () => m_StreamSettings;
                m_OnStreamSettingsChange += useStreamSettings.OnStreamSettingsChange;
            }

            var useReaderActive = obj as IUseReaderActive;
            if (useReaderActive != null)
            {
                useReaderActive.isStreamActive = () => streamActive;
                useReaderActive.isTrackingActive = () => trackingActive;
            }

            var useReaderHeadPose = obj as IUseReaderHeadPose;
            if (useReaderHeadPose != null)
            {
                useReaderHeadPose.getHeadPose = () => headPose;
            }

            var useReaderCameraPose = obj as IUseReaderCameraPose;
            if (useReaderCameraPose != null)
            {
                useReaderCameraPose.getCameraPose = () => cameraPose;
            }

            var useReaderBlendShapes = obj as IUseReaderBlendShapes;
            if (useReaderBlendShapes != null)
            {
                useReaderBlendShapes.getBlendShapesBuffer = () => blendShapesBuffer;
            }

            var serverSettings = obj as IServerSettings;
            if (serverSettings != null)
            {
                serverSettings.getPortNumber = () => m_Port;
                serverSettings.getFrameCatchupSize = () => m_CatchupSize;
            }
        }
    }
}
