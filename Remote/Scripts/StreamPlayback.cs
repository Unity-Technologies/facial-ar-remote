using System;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamPlayback : StreamSource
    {
        [SerializeField]
        PlaybackData m_PlaybackData;

        public bool playing { get; private set; }

        float m_PlaybackStartTime;

        [SerializeField]
        float m_TimeStep = 0.016f;

        int m_BufferPosition;
        byte[] m_CurrentFrameBuffer;
        byte[] m_NextFrameBuffer;

        float m_NextFrameTime;
        float m_CurrentTime;

        PlaybackBuffer m_ActivePlaybackBuffer;

        bool m_StreamerActive;

        protected override void Awake()
        {
            m_ActivePlaybackBuffer = m_PlaybackData.playbackBuffers[0];
            if (m_ActivePlaybackBuffer == null || m_ActivePlaybackBuffer.recordStream.Length < m_ActivePlaybackBuffer.BufferSize)
            {
                enabled = false;
                return;
            }

            m_ActivePlaybackBuffer.Initialize();

            base.Awake();

            var streamSettings = GetStreamSettings();

            m_BufferPosition = 0;
            m_CurrentFrameBuffer = new byte[streamSettings.BufferSize];
            m_NextFrameBuffer = new byte[streamSettings.BufferSize];
            for (var i = 0; i < streamSettings.BufferSize; i++)
            {
                m_CurrentFrameBuffer[i] = 0;
                m_NextFrameBuffer[i] = 0;
            }

            if (m_PlaybackData.playbackBuffers.Length == 0)
            {
                enabled = false;
            }
        }

        bool m_LastFrame;

        void Start()
        {
            m_StreamerActive = true;
            new Thread(() =>
            {
                while (m_StreamerActive)
                {
                    if (playing)
                    {
                        try
                        {
                            PlayBackLoop();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                            playing = false;
                        }
                    }

                    Thread.Sleep(4);
                }
            }).Start();
        }

        void PlayBackLoop()
        {
            if (m_PlayBackCurrentTime >= m_NextFrameTime)
            {
                var streamSettings = GetStreamSettings();

                Buffer.BlockCopy(m_NextFrameBuffer, 0, m_CurrentFrameBuffer, 0, streamSettings.BufferSize);
                Buffer.BlockCopy(m_FrameTimes, streamSettings.FrameTimeSize, m_FrameTimes, 0, streamSettings.FrameTimeSize);

                if (!m_LastFrame)
                {
                    if (m_BufferPosition + streamSettings.BufferSize > m_ActivePlaybackBuffer.recordStream.Length)
                    {
                        m_LastFrame = true;
                        m_NextFrameTime += m_TimeStep;
                    }
                    else
                    {
                        Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, m_BufferPosition,
                            m_NextFrameBuffer, 0, streamSettings.BufferSize);
                        Buffer.BlockCopy(m_NextFrameBuffer, streamSettings.FrameTimeOffset, m_FrameTimes,
                            streamSettings.FrameTimeSize, streamSettings.FrameTimeSize);

                        m_BufferPosition += streamSettings.BufferSize;
                        m_NextFrameTime = m_FrameTimes[1];
                    }
                }
                else
                {
                    StopPlayBack();
                }
            }
        }

        void FixedUpdate()
        {
            UpdateTimes();
        }

        protected override void Update()
        {
            UpdateTimes();

            if (m_StreamReader.streamSource != this && playing)
                playing = false;

            base.Update();

            UpdateReader();
        }

        void LateUpdate()
        {
            UpdateTimes();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            playing = false;
            m_StreamerActive = false;
        }

        public override IStreamSettings GetStreamSettings()
        {
            if (m_ActivePlaybackBuffer == null)
                Debug.LogError("Playback Buffer is Null!");

            return m_ActivePlaybackBuffer;
        }

        float[] m_FrameTimes = new float[2];
        float m_LocalDeltaTime;
        float m_FirstFrameTime;
        float m_PlayBackCurrentTime;

        void UpdateTimes()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = m_CurrentTime - m_PlaybackStartTime;

            m_PlayBackCurrentTime = m_FirstFrameTime + m_LocalDeltaTime;
        }

        public void StartPlayBack()
        {
            var streamSettings = GetStreamSettings();

            Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, streamSettings.FrameTimeOffset, m_FrameTimes, 0,
                streamSettings.FrameTimeSize);
            Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, streamSettings.BufferSize + streamSettings.FrameTimeOffset,
                m_FrameTimes, streamSettings.FrameTimeSize, streamSettings.FrameTimeSize);

            Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, 0, m_CurrentFrameBuffer, 0, streamSettings.BufferSize);
            Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, 0, m_NextFrameBuffer, 0, streamSettings.BufferSize);

            m_CurrentTime = Time.timeSinceLevelLoad;
            m_PlaybackStartTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = 0f;
            m_FirstFrameTime = m_FrameTimes[0];
            m_PlayBackCurrentTime = m_FrameTimes[0];
            m_NextFrameTime = m_FrameTimes[0];
            m_BufferPosition = 0;

            m_LastFrame = false;
            playing = true;
        }

        public void StopPlayBack()
        {
            m_NextFrameTime = float.PositiveInfinity;
            playing = false;
        }

        bool once = true;

        void UpdateReader()
        {
            if (m_StreamReader.streamSource == this && playing)
                m_StreamReader.UpdateStreamData(this, ref m_CurrentFrameBuffer, 0);

            if (once)
            {
                var streamSettings = GetStreamSettings();

                var temp = new byte[streamSettings.BufferSize];
                for (var i = 0; i < temp.Length; i++)
                {
                    temp[i] = 0;
                }

                var frameNumArray = new int[1];
                var frameTimeArray = new float[1];
                var foo = m_ActivePlaybackBuffer.recordQueue.ToArray();
                for (var i = 0; i < foo.Length; i++)
                {
                    Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, i * streamSettings.BufferSize, temp, 0, streamSettings.BufferSize);
                    Buffer.BlockCopy(temp, streamSettings.FrameNumberOffset, frameNumArray, 0, streamSettings.FrameNumberSize);
                    Buffer.BlockCopy(temp, streamSettings.FrameTimeOffset, frameTimeArray, 0, streamSettings.FrameTimeSize);

                    Debug.Log(string.Format("{0} : {1}", frameNumArray[0], frameTimeArray[0]));
                }

                once = false;
            }

        }
    }
}
