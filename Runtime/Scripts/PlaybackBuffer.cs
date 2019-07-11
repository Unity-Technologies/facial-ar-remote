using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc />
    /// <summary>
    /// Contains settings and stream data for a recorded session.
    /// </summary>
    [Serializable]
    public class PlaybackBuffer : IStreamSettings
    {
        [SerializeField]
        [Tooltip("Name of the recorded stream.")]
        string m_Name;

        [SerializeField]
        [HideInInspector]
        byte[] m_RecordStream = { };

        [SerializeField]
        [HideInInspector]
        byte m_ErrorCheck;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeCount;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeSize;

        [SerializeField]
        [HideInInspector]
        int m_FrameNumberSize;

        [SerializeField]
        [HideInInspector]
        int m_FrameTimeSize;

        [SerializeField]
        [HideInInspector]
        int m_HeadPoseOffset;

        [SerializeField]
        [HideInInspector]
        int m_CameraPoseOffset;

        [SerializeField]
        [HideInInspector]
        int m_FrameNumberOffset;

        [SerializeField]
        [HideInInspector]
        int m_FrameTimeOffset;

        [SerializeField]
        [HideInInspector]
        int m_BufferSize;

        [SerializeField]
        string[] m_Locations = {};

        IStreamSettings m_StreamSettings;

        BlendShapeMappings m_BlendShapeMappings;
        int m_TouchInputCount;
        int m_InputStateCount;
        int m_InputStateSize;
        int m_InputScreenPositionSize;
        int m_InputStateOffset;
        int m_InputScreenPositionOffset;

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int FrameNumberSize { get { return m_FrameNumberSize; } }
        public int FrameTimeSize { get { return m_FrameTimeSize; } }
        public int HeadPoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset { get { return m_CameraPoseOffset; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }
        public int FrameTimeOffset { get { return m_FrameTimeOffset; } }
        public int bufferSize { get { return m_BufferSize; } }

        public string[] locations
        {
            get
            {
                if (m_Locations.Length != m_BlendShapeCount)
                {
                    UseDefaultLocations();
                }
                return m_Locations;
            }
        }

        public int inputStateOffset => m_InputStateOffset;

        public int inputStateSize => m_InputStateSize;

        public int inputScreenPositionOffset => m_InputScreenPositionOffset;

        public int inputScreenPositionSize => m_InputScreenPositionSize;

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public byte[] recordStream
        {
            get { return m_RecordStream; }
            set { m_RecordStream = value; }
        }

        PlaybackBuffer() {}

        /// <summary>
        /// Create a new playback buffer.
        /// </summary>
        /// <param name="streamSettings">The stream settings to define the playback buffer.</param>
        public PlaybackBuffer(IStreamSettings streamSettings)
        {
            m_StreamSettings = streamSettings;
            
            m_BufferSize = streamSettings.bufferSize;
            m_ErrorCheck = streamSettings.ErrorCheck;
            m_BlendShapeCount = streamSettings.BlendShapeCount;
            m_BlendShapeSize = streamSettings.BlendShapeSize;
            m_HeadPoseOffset = streamSettings.HeadPoseOffset;
            m_CameraPoseOffset = streamSettings.CameraPoseOffset;
            m_FrameNumberOffset = streamSettings.FrameNumberOffset;
            m_FrameTimeOffset = streamSettings.FrameTimeOffset;

            m_FrameNumberSize = streamSettings.FrameNumberSize;
            m_FrameTimeSize = streamSettings.FrameTimeSize;

            m_Locations = streamSettings.locations;
        }

        public void UseDefaultLocations()
        {
            m_Locations = m_StreamSettings.locations;
        }
    }
}
