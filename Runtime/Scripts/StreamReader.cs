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
        [Tooltip("(Optional) Manually add stream sources which aren't on this GameObject or its children.")]
        GameObject[] m_StreamSourceOverrides = { };

        bool m_FaceTrackingLost;

        IStreamSource m_ActiveStreamSource;

        int m_TrackingLossCount;

        bool m_FaceTrackingEnabled;
        bool m_CameraTrackingEnabled;
        Pose m_CameraPose;
        Pose m_HeadPose;
        Vector3 m_LastHeadPosition;

        float[] m_CameraPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        float[] m_HeadPoseArray = new float[BlendShapeUtils.PoseFloatCount];
        int[] m_FrameNumArray = new int[1];
        float[] m_FrameTimeArray = new float[1];
        int[] m_TouchPhaseArray = new int[1];
        float[] m_TouchPositionArray = new float[2];

        HashSet<IStreamSource> m_Sources = new HashSet<IStreamSource>();
        HashSet<IUsesStreamReader> m_Consumers = new HashSet<IUsesStreamReader>();
        TouchPhase m_TouchPhase;
        Vector2 m_TouchPosition;

        public float[] blendShapesBuffer { get; private set; }

        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }
        public bool verboseLogging { get { return m_VerboseLogging; } }
        public HashSet<IStreamSource> sources { get { return m_Sources; } }
        public HashSet<IUsesStreamReader> consumers => m_Consumers;
        
        public TouchPhase touchPhase => m_TouchPhase;

        public Vector2 touchPosition => m_TouchPosition;
        
        public void SetInitialHeadPose(Pose pose)
        {
            m_HeadPose = pose;
        }

        public void SetInitialCameraPose(Pose pose)
        {
            m_CameraPose = pose;
        }

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

        public GameObject character
        {
            get { return m_Character; }
        }

        public bool faceTrackingLost
        {
            get { return m_FaceTrackingLost; }
            set { m_FaceTrackingLost = value; }
        }

        public void UpdateStreamData(byte[] buffer, int offset = 0)
        {
            var settings = streamSource.streamSettings;

            Buffer.BlockCopy(buffer, offset + 1, blendShapesBuffer, 0, settings.BlendShapeSize);
            m_FaceTrackingEnabled = buffer[offset + settings.bufferSize - 2] == 1;
            m_CameraTrackingEnabled = buffer[offset + settings.bufferSize - 1] == 1;

            if (verboseLogging)
            {
                Buffer.BlockCopy(buffer, offset + settings.FrameNumberOffset, m_FrameNumArray, 0, settings.FrameNumberSize);
                Buffer.BlockCopy(buffer, offset + settings.FrameTimeOffset, m_FrameTimeArray, 0, settings.FrameTimeSize);
                Debug.Log($"{m_FrameNumArray[0]} : {m_FrameTimeArray[0]}");
            }

            if (m_FaceTrackingEnabled)
            {
                Buffer.BlockCopy(buffer, offset + settings.HeadPoseOffset, m_HeadPoseArray, 0, BlendShapeUtils.PoseSize);
                BlendShapeUtils.ArrayToPose(m_HeadPoseArray, ref m_HeadPose);
            }

            if (m_CameraTrackingEnabled)
            {
                Buffer.BlockCopy(buffer, offset + settings.CameraPoseOffset, m_CameraPoseArray, 0, BlendShapeUtils.PoseSize);
                BlendShapeUtils.ArrayToPose(m_CameraPoseArray, ref m_CameraPose);
            }
            
            Buffer.BlockCopy(buffer, offset + settings.inputStateOffset, m_TouchPhaseArray, 0, settings.inputStateSize);
            m_TouchPhase = (TouchPhase)m_TouchPhaseArray[0];
            
            Buffer.BlockCopy(buffer, offset + settings.inputScreenPositionOffset, m_TouchPositionArray, 0, settings.inputScreenPositionSize);
            m_TouchPosition.x = m_TouchPositionArray[0];
            m_TouchPosition.y = m_TouchPositionArray[1];
        }

        public void ConnectDependencies()
        {
            sources.UnionWith(GetComponentsInChildren<IStreamSource>());
            foreach (var go in m_StreamSourceOverrides)
            {
                if (go != null)
                    sources.UnionWith(go.GetComponentsInChildren<IStreamSource>());
            }

            var alreadyContainsNetworkStream = false;
            foreach (var source in sources)
            {
                ConnectInterfaces(source);
                if (source is NetworkStream)
                {
                    if (!alreadyContainsNetworkStream)
                    {
                        streamSource = source;
                        alreadyContainsNetworkStream = true;
                    }
                    else
                    {
                        Debug.LogWarning("Can only have one Network Source. Removing the last one.");
                        sources.Remove(source);
                    }
                }
            }

            if (character != null)
            {
                consumers.UnionWith(character.GetComponentsInChildren<IUsesStreamReader>());
                foreach (var consumer in consumers)
                    ConnectInterfaces(consumer);
            }
        }

        void Awake()
        {
            Application.targetFrameRate = 60;

            ConnectDependencies();
        }

        void Update()
        {
            UpdateHeadTrackingState();

            foreach (var source in sources)
            {
                source.StreamSourceUpdate();
            }
        }

        void UpdateHeadTrackingState()
        {
            var headPosition = m_HeadPose.position;
            if (headPosition == m_LastHeadPosition)
            {
                m_TrackingLossCount++;
                faceTrackingLost = m_TrackingLossCount >= m_TrackingLossPadding;
            }
            else
            {
                m_TrackingLossCount = 0;
            }

            m_LastHeadPosition = headPosition;
        }

        void ConnectInterfaces(object obj)
        {
            var usesStreamReader = obj as IUsesStreamReader;
            if (usesStreamReader != null)
                usesStreamReader.streamReader = this;

            var ss = obj as IStreamSource;
            if (ss != null && !ss.streamReaders.Contains(this))
            {
                ss.streamReaders.Add(this);
            }
        }
    }
}
