using System;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamPlayback : MonoBehaviour, IStreamSource
    {
        const float k_TimeStep = 0.016f;

        [SerializeField]
        [Tooltip("Contains the individual streams recorded from a capture session.")]
        PlaybackData m_PlaybackData;

        float m_PlaybackStartTime;

        int m_BufferPosition;
        byte[] m_CurrentFrameBuffer;
        byte[] m_NextFrameBuffer;

        float m_NextFrameTime;
        float m_CurrentTime;

        readonly float[] m_FrameTimes = new float[2];
        float m_LocalDeltaTime;
        float m_FirstFrameTime;
        float m_PlayBackCurrentTime;

        bool m_Running;
        bool m_LastFrame;

        public IStreamReader streamReader { private get; set; }
        public bool active { get; private set; }
        public PlaybackBuffer activePlaybackBuffer { get; private set; }
        public byte[] currentFrameBuffer { get { return m_CurrentFrameBuffer; } }

        public IStreamSettings streamSettings
        {
            get { return activePlaybackBuffer; }
        }

        public PlaybackData playbackData
        {
            get { return m_PlaybackData; }
        }

        void Start()
        {
            m_Running = true;
            new Thread(() =>
            {
                while (m_Running)
                {
                    if (active)
                    {
                        try
                        {
                            if(!PlayBackLoop())
                                StopPlayback();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                            active = false;
                        }
                    }

                    Thread.Sleep(4);
                }
            }).Start();
        }

        void OnDestroy()
        {
            m_Running = false;
        }

        public void StreamSourceUpdate()
        {
            var source = streamReader.streamSource;
            if (source != null && !source.Equals(this) && active)
                active = false;

            if (!active)
                return;

            UpdateCurrentFrameBuffer();
        }

        public void UpdateTimes()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = m_CurrentTime - m_PlaybackStartTime;

            m_PlayBackCurrentTime = m_FirstFrameTime + m_LocalDeltaTime;
        }

        public void SetPlaybackBuffer(PlaybackBuffer buffer)
        {
            if (active)
                StopPlayback();

            activePlaybackBuffer = buffer;
            if (activePlaybackBuffer == null || activePlaybackBuffer.recordStream.Length < activePlaybackBuffer.BufferSize)
                return;

            streamReader.streamSettings = activePlaybackBuffer;
        }

        public void StartPlayback()
        {
            if (activePlaybackBuffer == null)
            {
                Debug.Log("No Playback Buffer Set.");
                SetPlaybackBuffer(streamReader.playbackData.playbackBuffers[0]);
            }

            if (streamReader.streamSettings != activePlaybackBuffer)
                streamReader.streamSettings = activePlaybackBuffer;

            Buffer.BlockCopy(activePlaybackBuffer.recordStream, streamSettings.FrameTimeOffset, m_FrameTimes, 0,
                streamSettings.FrameTimeSize);
            Buffer.BlockCopy(activePlaybackBuffer.recordStream, streamSettings.BufferSize + streamSettings.FrameTimeOffset,
                m_FrameTimes, streamSettings.FrameTimeSize, streamSettings.FrameTimeSize);

            Buffer.BlockCopy(activePlaybackBuffer.recordStream, 0, m_CurrentFrameBuffer, 0, streamSettings.BufferSize);
            Buffer.BlockCopy(activePlaybackBuffer.recordStream, 0, m_NextFrameBuffer, 0, streamSettings.BufferSize);

            m_CurrentTime = Time.timeSinceLevelLoad;
            m_PlaybackStartTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = 0f;
            m_FirstFrameTime = m_FrameTimes[0];
            m_PlayBackCurrentTime = m_FrameTimes[0];
            m_NextFrameTime = m_FrameTimes[0];
            m_BufferPosition = 0;

            m_LastFrame = false;
            active = true;
        }

        public void StopPlayback()
        {
            m_NextFrameTime = float.PositiveInfinity;
            active = false;
        }

        public bool PlayBackLoop(bool forceNext = false)
        {
            if (forceNext || m_PlayBackCurrentTime >= m_NextFrameTime)
            {
                Buffer.BlockCopy(m_NextFrameBuffer, 0, m_CurrentFrameBuffer, 0, streamSettings.BufferSize);
                Buffer.BlockCopy(m_FrameTimes, streamSettings.FrameTimeSize, m_FrameTimes, 0, streamSettings.FrameTimeSize);

                if (!m_LastFrame)
                {
                    if (m_BufferPosition + streamSettings.BufferSize > activePlaybackBuffer.recordStream.Length)
                    {
                        m_LastFrame = true;
                        m_NextFrameTime += k_TimeStep;
                    }
                    else
                    {
                        Buffer.BlockCopy(activePlaybackBuffer.recordStream, m_BufferPosition,
                            m_NextFrameBuffer, 0, streamSettings.BufferSize);
                        Buffer.BlockCopy(m_NextFrameBuffer, streamSettings.FrameTimeOffset, m_FrameTimes,
                            streamSettings.FrameTimeSize, streamSettings.FrameTimeSize);

                        m_BufferPosition += streamSettings.BufferSize;
                        m_NextFrameTime = m_FrameTimes[1];
                    }
                }
                else
                {
                    //StopPlayBack();
                    return false;
                }
            }
            return true;
        }

        public void UpdateCurrentFrameBuffer(bool force = false)
        {
            if (force || streamReader.streamSource.Equals(this) && active)
                streamReader.UpdateStreamData(ref m_CurrentFrameBuffer, 0);
        }

        public void OnStreamSettingsChanged(IStreamSettings settings)
        {
            m_BufferPosition = 0;
            m_CurrentFrameBuffer = new byte[settings.BufferSize];
            m_NextFrameBuffer = new byte[settings.BufferSize];
            for (var i = 0; i < settings.BufferSize; i++)
            {
                m_CurrentFrameBuffer[i] = 0;
                m_NextFrameBuffer[i] = 0;
            }
        }
    }
}
