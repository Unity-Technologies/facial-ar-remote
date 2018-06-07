using System;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamPlayback : StreamSource
    {
        [SerializeField]
        PlaybackData m_PlaybackData;

        [SerializeField]
        BlendShapesController m_BlendShapesController;

        [SerializeField]
        Transform m_RootBone;

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
        float[] m_FrameTimes = new float[2];
        float m_LocalDeltaTime;
        float m_FirstFrameTime;
        float m_PlayBackCurrentTime;

        public PlaybackData playbackData { get { return m_PlaybackData; } }
        public PlaybackBuffer activePlaybackBuffer { get { return m_ActivePlaybackBuffer; } }
        public byte[] currentFrameBuffer { get { return m_CurrentFrameBuffer; } }
        public BlendShapesController blendShapesController { get { return m_BlendShapesController; } }
        public Transform rootBone {get { return m_RootBone; }}

        protected override void Awake()
        {
            if (!SetPlaybackBuffer(m_PlaybackData.playbackBuffers[0], false))
                return;

            base.Awake();

            RefreshStreamSettings();
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
                            if(!PlayBackLoop())
                                StopPlayBack();
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

        void UpdateTimes()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = m_CurrentTime - m_PlaybackStartTime;

            m_PlayBackCurrentTime = m_FirstFrameTime + m_LocalDeltaTime;
        }

        public bool SetPlaybackBuffer(PlaybackBuffer buffer, bool refreshStream = true)
        {
            if (playing)
                StopPlayBack();

            m_ActivePlaybackBuffer = buffer;
            if (m_ActivePlaybackBuffer == null || m_ActivePlaybackBuffer.recordStream.Length < m_ActivePlaybackBuffer.BufferSize)
            {
                enabled = false;
                return false;
            }

            m_ActivePlaybackBuffer.Initialize();
            if (refreshStream)
                RefreshStreamSettings();

            return true;
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

        public bool PlayBackLoop(bool forceNext = false)
        {
            if (forceNext || m_PlayBackCurrentTime >= m_NextFrameTime)
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
                    //StopPlayBack();
                    return false;
                }
            }
            return true;
        }

        public void UpdateReader(bool force = false)
        {
            if (force || m_StreamReader.streamSource == this && playing)
                m_StreamReader.UpdateStreamData(this, ref m_CurrentFrameBuffer, 0);
        }

        public void RefreshStreamSettings()
        {
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
    }
}
