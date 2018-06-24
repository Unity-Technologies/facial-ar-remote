using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    [CreateAssetMenu(fileName = "PlaybackData", menuName = "FacialRemote/PlaybackData")]
    public class PlaybackData : ScriptableObject
    {
        [SerializeField]
        PlaybackBuffer[] m_PlaybackBuffers;

        public PlaybackBuffer[] playbackBuffers { get { return m_PlaybackBuffers; } }

        public List<byte[]> activeByteRecord
        {
            get
            {
                return m_ActiveByteRecord;
            }
        }

        List<byte[]> m_ActiveByteRecord = new List<byte[]>();
        PlaybackBuffer m_ActivePlaybackBuffer;

        Queue<byte[]> m_BufferQueue = new Queue<byte[]>();
        int m_CurrentBufferSize = -1;
        Thread m_BufferCreateThread;

        const int k_PreWarmAmount = 240;
        const int k_MinBufferAmount = 20;
        const int k_BufferCreateAmount = 6;

        void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EditorStateChange;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EditorStateChange;
#endif
        }

#if UNITY_EDITOR
        void EditorStateChange(PlayModeStateChange state)
        {
            StopActivePlaybackBuffer();
        }
#endif

        public void WarmUpPlaybackBuffer(IStreamSettings streamSettings)
        {
            if (m_CurrentBufferSize != streamSettings.BufferSize)
            {
                m_BufferQueue.Clear();
                if (m_BufferCreateThread != null)
                {
                    m_BufferCreateThread.Abort();
                    m_BufferCreateThread = null;
                }
            }

            var current = m_BufferQueue.Count;
            m_CurrentBufferSize = streamSettings.BufferSize;
            if (current >= k_BufferCreateAmount)
                return;

            for (var i = current; i < k_PreWarmAmount; i++)
            {
                m_BufferQueue.Enqueue(new byte[streamSettings.BufferSize]);
            }

            if (m_BufferCreateThread == null)
            {
                m_BufferCreateThread = new Thread(() =>
                {
                    while (m_BufferQueue.Count < k_MinBufferAmount)
                    {
                        for (var i = 0; i < k_BufferCreateAmount; i++)
                        {
                            m_BufferQueue.Enqueue(new byte[m_CurrentBufferSize]);
                        }
                    }
                    Thread.Sleep(1);
                });
                m_BufferCreateThread.Start();
            }
        }

        public void CreatePlaybackBuffer(IStreamSettings streamSettings, int take)
        {
            var playbackBuffer = new PlaybackBuffer(streamSettings);
            playbackBuffer.name = String.Format("{0:yyyy_MM_dd_HH_mm}-Take{1:00}", DateTime.Now, take);
            Debug.Log(string.Format("Starting take: {0}", playbackBuffer.name));
            m_ActivePlaybackBuffer = playbackBuffer;
        }

        public void AddToActiveBuffer(byte[] buffer)
        {
            var copyBuffer = m_BufferQueue.Dequeue();
            Buffer.BlockCopy(buffer, 0, copyBuffer, 0, m_CurrentBufferSize);

            activeByteRecord.Add(copyBuffer);
        }

        public void StopActivePlaybackBuffer()
        {
            SaveActivePlaybackBuffer();
        }

        public void SaveActivePlaybackBuffer()
        {
            if (m_ActivePlaybackBuffer == null)
                return;

            var bufferCount = m_ActiveByteRecord.Count;
            m_ActivePlaybackBuffer.recordStream = new byte[bufferCount * m_CurrentBufferSize];
            for (var i = 0; i < bufferCount; i++)
            {
                var buffer = m_ActiveByteRecord[i];
                Buffer.BlockCopy(buffer, 0, m_ActivePlaybackBuffer.recordStream, i * m_CurrentBufferSize, m_CurrentBufferSize);
                m_BufferQueue.Enqueue(buffer);
            }

            var buffers = new PlaybackBuffer[m_PlaybackBuffers.Length + 1];
            for (var i = 0; i < m_PlaybackBuffers.Length; i++)
            {
                buffers[i] = m_PlaybackBuffers[i];
            }
            buffers[buffers.Length - 1] = m_ActivePlaybackBuffer;
            m_PlaybackBuffers = buffers;

            m_ActivePlaybackBuffer = null;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void OnValidate()
        {
            foreach (var playbackBuffer in m_PlaybackBuffers)
            {
                if (playbackBuffer.locations.Length == 0)
                {
                    playbackBuffer.UseDefaultLocations();
                }
            }
        }
    }
}
