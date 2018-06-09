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
        bool m_UseDebug;

        [SerializeField]
        PlaybackData m_PlaybackData;

        [Header("Server Settings")]
        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        int m_CatchupSize = 2;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        [Header("Server Settings")]
        [SerializeField]
        BlendShapesController m_BlendShapesController;

        Pose m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
        Pose m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);

        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];

        public bool faceActive { get; private set; }
        public bool trackingActive { get; private set; }
        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }

        public PlaybackData playbackData { get { return m_PlaybackData; } }
        public BlendShapesController blendShapesController { get { return m_BlendShapesController; } }

        public bool streamActive
        {
            get { return enabled && activeStreamSource != null && activeStreamSource.streamActive; }
        }

        public IStreamSource activeStreamSource { get; private set; }
        public IStreamSettings ActiveStreamSettings { get; private set; }

        public void UseStreamReaderSettings()
        {
            if (ActiveStreamSettings.Equals(m_StreamSettings))
                return;

            ActiveStreamSettings = m_StreamSettings;
            m_OnStreamSettingsChange.Invoke();
        }

        public void SetActiveStreamSettings(IStreamSettings settings)
        {
            if (ActiveStreamSettings.Equals(settings))
                return;

            ActiveStreamSettings = settings;
            m_OnStreamSettingsChange.Invoke();
        }

        public void SetStreamSource(IStreamSource source)
        {
            if (source == null || source.getStreamSettings() == null)
                return;
            activeStreamSource = source;
            m_BlendShapesBuffer = new float[source.getStreamSettings().BlendShapeCount];

            UpdateStreamSettings(source.getStreamSettings());
        }

        public void UpdateStreamSettings(IStreamSettings streamSettings)
        {
            foreach (var controller in m_ConnectedControllers)
            {
                controller.SetStreamSettings(streamSettings);
            }
        }

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }
        public float[] headPoseArray { get { return m_HeadPoseArray; } }
        public float[] cameraPoseArray { get { return m_CameraPoseArray; } }

        float[] m_BlendShapesBuffer;
        float[] m_HeadPoseArray = new float[7];
        float[] m_CameraPoseArray = new float[7];

        List<IConnectedController> m_ConnectedControllers = new List<IConnectedController>();
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
            activeStreamSource = null;
        }

        public void UpdateStreamData(ref byte[] buffer, int position)
        {
            var streamSettings = activeStreamSource.getStreamSettings();

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

        public void AddConnectedController(IConnectedController connectedController)
        {
            if (!m_ConnectedControllers.Contains(connectedController))
                m_ConnectedControllers.Add(connectedController);
        }

        public void RemoveConnectedController(IConnectedController connectedController)
        {
            if (m_ConnectedControllers.Contains(connectedController))
                m_ConnectedControllers.Remove(connectedController);
        }

        Action m_OnStreamSettingsChange = () =>
        {
            Debug.Log("OnStreamSettingsChange" );
        };

        void Awake()
        {
            m_Server = new Server();
            ConnectInterfaces(m_Server);

            m_StreamPlayback = new StreamPlayback();
            ConnectInterfaces(m_StreamPlayback);

            m_StreamSources.Add(m_Server);
            m_StreamSources.Add(m_StreamPlayback);

            ActiveStreamSettings = m_StreamSettings;
            m_OnStreamSettingsChange.Invoke();
        }

        void ConnectInterfaces(object obj)
        {
            var streamSource = obj as IStreamSource;
            if (streamSource != null)
            {
                streamSource.getStreamReader = () => this;
                streamSource.isStreamSource = () => activeStreamSource == streamSource;
                streamSource.getPlaybackData = () => m_PlaybackData;
                streamSource.getUseDebug = () => m_UseDebug;
                streamSource.getStreamSettings = () => ActiveStreamSettings;
//                m_OnStreamSettingsChange += streamSource.StreamSettingsChangeCallback;
                m_OnStreamSettingsChange += streamSource.OnStreamSettingsChangeChange;
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

            foreach (var streamSource in m_StreamSources)
            {
                streamSource.StartStreamThread();
            }
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
