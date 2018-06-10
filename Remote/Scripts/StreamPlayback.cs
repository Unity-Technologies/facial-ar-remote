using System;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamPlayback : StreamSource
    {
        public bool playing { get; private set; }

        float m_PlaybackStartTime;

        const float k_TimeStep = 0.016f;

        int m_BufferPosition;
        byte[] m_CurrentFrameBuffer;
        byte[] m_NextFrameBuffer;

        float m_NextFrameTime;
        float m_CurrentTime;

        PlaybackBuffer m_ActivePlaybackBuffer;

        float[] m_FrameTimes = new float[2];
        float m_LocalDeltaTime;
        float m_FirstFrameTime;
        float m_PlayBackCurrentTime;

        public PlaybackBuffer activePlaybackBuffer { get { return m_ActivePlaybackBuffer; } }
        public byte[] currentFrameBuffer { get { return m_CurrentFrameBuffer; } }

        bool m_LastFrame;

        public override void StartStreamThread()
        {
            streamThreadActive = true;
            new Thread(() =>
            {
                while (streamThreadActive)
                {
                    if (playing)
                    {
                        try
                        {
                            if(!PlayBackLoop())
                                StopPlaybackDataUsage();
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

        public override void StreamSourceUpdate()
        {
            if (!isStreamSource() && playing)
                playing = false;

            if (!streamActive || !isStreamSource())
                return;

            UpdateCurrentFrameBuffer();
        }

//        protected override IStreamSettings GetStreamSettings()
//        {
//            if (m_ActivePlaybackBuffer == null)
//                Debug.LogError("Playback Buffer is Null!");
//
//            return m_ActivePlaybackBuffer;
//        }

        public void UpdateTimes()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            m_LocalDeltaTime = m_CurrentTime - m_PlaybackStartTime;

            m_PlayBackCurrentTime = m_FirstFrameTime + m_LocalDeltaTime;
        }

        public bool SetPlaybackBuffer(PlaybackBuffer buffer)
        {
            if (playing)
                StopPlaybackDataUsage();

            m_ActivePlaybackBuffer = buffer;
            if (activePlaybackBuffer == null || activePlaybackBuffer.recordStream.Length < activePlaybackBuffer.BufferSize)
            {
//                enabled = false;
                return false;
            }

            activePlaybackBuffer.Initialize();

            SetReaderStreamSettings();

            return true;
        }

        public override void SetReaderStreamSettings()
        {
            streamReader.SetActiveStreamSettings(activePlaybackBuffer);
        }

        public override void StartPlaybackDataUsage()
        {
            if (activePlaybackBuffer == null)
            {
                Debug.Log("No Playback Buffer Set.");
                SetPlaybackBuffer(playbackData.playbackBuffers[0]);
            }

//            var streamSettings = GetStreamSettings();
            if (streamSettings != activePlaybackBuffer)
                SetReaderStreamSettings();

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
            playing = true;
        }

        public override void StopPlaybackDataUsage()
        {
            m_NextFrameTime = float.PositiveInfinity;
            playing = false;
        }

        public bool PlayBackLoop(bool forceNext = false)
        {
            if (forceNext || m_PlayBackCurrentTime >= m_NextFrameTime)
            {
//                var streamSettings = GetStreamSettings();

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

        public override void UpdateCurrentFrameBuffer(bool force = false)
        {
            if (force || isStreamSource() && playing)
                streamReader.UpdateStreamData(ref m_CurrentFrameBuffer, 0);
        }

        public override void OnStreamSettingsChangeChange()
        {
//            var streamSettings = GetStreamSettings();

            m_BufferPosition = 0;
            m_CurrentFrameBuffer = new byte[streamSettings.BufferSize];
            m_NextFrameBuffer = new byte[streamSettings.BufferSize];
            for (var i = 0; i < streamSettings.BufferSize; i++)
            {
                m_CurrentFrameBuffer[i] = 0;
                m_NextFrameBuffer[i] = 0;
            }
        }
    }
}
