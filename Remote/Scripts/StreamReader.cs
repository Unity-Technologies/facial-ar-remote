using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamReader : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        StreamSettings m_StreamSettings;

        [SerializeField]
        PlaybackData m_PlaybackData;

        [SerializeField]
        bool m_UseDebug;

        [Header("Server Settings")]
        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        int m_CatchupSize = 2;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        [Header("Controller Settings")]
        [SerializeField]
        BlendShapesController m_BlendShapesController;

        [SerializeField]
        CharacterRigController m_CharacterRigController;

        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        Transform m_HeadBone;

        Pose m_HeadPose;
        Pose m_CameraPose;

        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];

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

        IStreamSource m_ActiveStreamSource;
        IStreamSettings m_ActiveStreamSettings;

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

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }
        public float[] headPoseArray { get { return m_HeadPoseArray; } }
        public float[] cameraPoseArray { get { return m_CameraPoseArray; } }

        float[] m_BlendShapesBuffer;
        float[] m_HeadPoseArray = new float[7];
        float[] m_CameraPoseArray = new float[7];

        HashSet<IStreamSource> m_StreamSources = new HashSet<IStreamSource>();

        Server m_Server;
        StreamPlayback m_StreamPlayback;

        public Server server
        {
            get
            {
                if (m_Server == null)
                    Awake();

                return m_Server;
            }
        }

        public StreamPlayback streamPlayback
        {
            get
            {
                if (m_StreamPlayback == null)
                    Awake();
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
                ArrayToPose(headPoseArray, ref m_HeadPose);
                ArrayToPose(cameraPoseArray, ref m_CameraPose);
            }
        }

        Action m_OnStreamSettingsChange;

        void Awake()
        {
            m_OnStreamSettingsChange = () =>
            {
                if (m_UseDebug)
                    Debug.Log("OnStreamSettingsChange" );
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

        void Start()
        {
            Application.targetFrameRate = 120;

            m_HeadPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
            m_CameraPose = new Pose(m_Camera.transform.position, m_Camera.transform.rotation);

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

        static void ArrayToPose(float[] poseArray, ref Pose pose)
        {
            pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
            pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
        }
    }
}
