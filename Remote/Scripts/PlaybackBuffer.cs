using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class PlaybackBuffer: IStreamSettings
    {
        [SerializeField]
        string m_Name;

        [SerializeField]
        [HideInInspector]
        byte[] m_RecordStream = { };

        [SerializeField]
        [HideInInspector]
        int m_BufferSize = 266;

        [SerializeField]
        [HideInInspector]
        bool m_Initialized;

        [SerializeField]
        [HideInInspector]
        byte m_ErrorCheck = 42;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeCount = 51;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeSize = 51 * sizeof(float);

        [SerializeField]
        [HideInInspector]
        int m_PoseSize = 7 * sizeof(float);

        [SerializeField]
        [HideInInspector]
        int m_HeadPoseOffset = 205;

        [SerializeField]
        [HideInInspector]
        int m_CameraPoseOffset = 233;

        [SerializeField]
        [HideInInspector]
        int m_FrameNumberOffset = 233;

        public bool Initialized { get { return m_Initialized; } }
        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int PoseSize { get { return m_PoseSize; } }
        public int PoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset { get { return m_CameraPoseOffset; } }
        public int BufferSize { get { return m_BufferSize; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public byte[] recordStream { get { return m_RecordStream; } set { m_RecordStream = value; } }

        Queue<byte[]> m_RecordQueue;

        public Queue<byte[]> recordQueue
        {
            get
            {
                if (m_RecordQueue == null)
                {
                    m_RecordQueue = QueueRecordStream(recordStream, m_BufferSize);
                }
                return m_RecordQueue;
            }
        }

        PlaybackBuffer() {}

        public PlaybackBuffer(IStreamSettings streamSettings)
        {
            SetStreamSettings(streamSettings);
        }

        public void SetStreamSettings(IStreamSettings streamSettings)
        {
            if (!streamSettings.Initialized)
                streamSettings.Initialize();

            m_BufferSize = streamSettings.BufferSize;
            m_ErrorCheck = streamSettings.ErrorCheck;
            m_BlendShapeCount = streamSettings.BlendShapeCount;
            m_BlendShapeSize = streamSettings.BlendShapeSize;
            m_PoseSize = streamSettings.PoseOffset;
            m_HeadPoseOffset = streamSettings.PoseOffset;
            m_CameraPoseOffset = streamSettings.CameraPoseOffset;
            m_FrameNumberOffset = streamSettings.FrameNumberOffset;
        }

        public void Initialize()
        {
            m_Initialized = true;
        }

        static Queue<byte[]> QueueRecordStream(byte[] stream, int bufferSize = 266)
        {
            var queue = new Queue<byte[]>();
            var empty = new byte();
            for (var i = 0; i < stream.Length;)
            {
                var bytes = new byte[bufferSize];
                for (var b = 0; b < bytes.Length; b++)
                {
                    bytes[b] = i + b < stream.Length ? stream[i + b] : empty;
                }

                queue.Enqueue(bytes);
                i += bufferSize;
            }
            return queue;
        }
    }
}
