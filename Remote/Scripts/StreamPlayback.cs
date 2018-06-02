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

        float m_StartTime;

        [SerializeField]
        float m_TimeStep = 0.016f;

        int m_CurrentFrame = 0;
        int m_NextFrame;
        int m_BufferPosition;
        byte[] m_CurrentFrameBuffer;
        byte[] m_NextFrameBuffer;

        float m_CurrentFrameTime;
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

            base.Awake();

//            m_BufferPosition = 0;
//            m_CurrentFrameBuffer = new byte[m_StreamReader.streamSettings.BufferSize];
//            m_NextFrameBuffer = new byte[m_StreamReader.streamSettings.BufferSize];
//            for (var i = 0; i < m_StreamReader.streamSettings.BufferSize; i++)
//            {
//                m_CurrentFrameBuffer[i] = 0;
//                m_NextFrameBuffer[i] = 0;
//            }
//
//            if (m_PlaybackData.playbackBuffers.Length == 0)
//            {
//                enabled = false;
//                return;
//            }

//            m_ActivePlaybackBuffer = m_PlaybackData.playbackBuffers[m_PlaybackData.playbackBuffers.Length-1];
//            m_ActivePlaybackBuffer = m_PlaybackData.playbackBuffers[0];
//            if (m_ActivePlaybackBuffer == null || m_ActivePlaybackBuffer.recordStream.Length < m_StreamReader.streamSettings.BufferSize)
//            {
//                enabled = false;
//                return;
//            }

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
                return;
            }
        }

        void Start()
        {
            m_StreamerActive = true;
            new Thread(() =>
            {
                var frameNumArray = new int[1];

                while (m_StreamerActive)
                {
                    if (playing)
                    {
                        Debug.Log("is playing");
                        try
                        {
                            if (m_CurrentTime >= m_NextFrameTime)
                            {
                                var streamSettings = GetStreamSettings();

                                m_CurrentFrame = m_NextFrame;
                                Debug.Log(m_CurrentFrame);
                                Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, m_BufferPosition,
                                    m_CurrentFrameBuffer, 0, streamSettings.BufferSize);

                                Buffer.BlockCopy(m_CurrentFrameBuffer, streamSettings.FrameNumberOffset,
                                    frameNumArray, 0, sizeof(int));
                                m_CurrentFrame = frameNumArray[0];

                                m_BufferPosition += streamSettings.BufferSize;
                                Debug.Log(string.Format("buffer position: {0}", m_BufferPosition));

                                Buffer.BlockCopy(m_ActivePlaybackBuffer.recordStream, m_BufferPosition,
                                    m_NextFrameBuffer, 0, streamSettings.BufferSize);

                                Buffer.BlockCopy(m_NextFrameBuffer, streamSettings.FrameNumberOffset,
                                    frameNumArray, 0, sizeof(int));
                                m_NextFrame = frameNumArray[0];

                                m_NextFrameTime = m_StartTime + m_NextFrame * m_TimeStep;
                                Debug.Log(string.Format("c: {0} : {1} n: {2} : {3}", m_CurrentFrame, m_CurrentTime, m_NextFrame, m_NextFrameTime));
                            }
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
            m_CurrentTime = Time.timeSinceLevelLoad;
        }

        protected override void Update()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            if (m_StreamReader.streamSource != this && playing)
                playing = false;

            base.Update();

            UpdateReader();
        }

        void LateUpdate()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            playing = false;
            m_StreamerActive = false;
        }

        public override IStreamSettings GetStreamSettings()
        {
            return m_ActivePlaybackBuffer == null ? null : m_ActivePlaybackBuffer;
        }

        public void StartPlayBack()
        {
            m_CurrentTime = Time.timeSinceLevelLoad;
            m_StartTime = m_CurrentTime;
            m_NextFrameTime = m_StartTime;
            m_BufferPosition = 0;

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
