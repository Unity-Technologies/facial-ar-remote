using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class Server : StreamSource, IServerSettings
    {
        const int k_BufferPrewarm = 16;
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames

        public Func<int> getPortNumber { get; set; }
        public Func<int> getFrameCatchupSize { get; set; }
        public Func<int> getFrameCatchupThreshold { get; set; }

        int portNumber {get { return getPortNumber(); } }
        int catchupSize {get { return getFrameCatchupSize(); } }
        int catchupThreshold { get { return getFrameCatchupThreshold(); } }

        Socket m_Socket;
        int m_LastFrameNum;
        int m_TakeNumber;
        int m_CurrentBufferSize = -1;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

        public bool useRecorder
        {
            get { return Application.isEditor && Application.isPlaying && playbackData != null; }
        }

        public bool isRecording { get; private set; }

        protected override bool IsStreamActive()
        {
            return isSource && deviceConnected;
        }

        public bool deviceConnected
        {
            get { return m_Socket != null && m_Socket.Connected; }
        }

        public override void StartStreamThread()
        {
            m_TakeNumber = 0;
            Debug.Log("Possible IP addresses:");
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                var connectionAddress = address;
                Debug.Log(connectionAddress);
                try
                {
                    var endPoint = new IPEndPoint(connectionAddress, portNumber);
                    m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    m_Socket.Bind(endPoint);
                    m_Socket.Listen(100);
                    m_LastFrameNum = -1;
                    streamThreadActive = true;
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Error on address {0} : {1}", connectionAddress, e);
                }

                new Thread(() =>
                {
                    m_Socket = m_Socket.Accept();
                    Debug.Log(string.Format("Client connected on {0}", connectionAddress));

                    var frameNumArray = new int[1];

                    while (streamThreadActive)
                    {
                        if (m_Socket.Connected)
                        {
                            try
                            {
                                if (streamSettings == null || streamSettings.BufferSize < 1)
                                {
                                    Debug.LogError("Abort!");
                                    break;
                                }

                                var buffer = m_UnusedBuffers.Count == 0 ? new byte[streamSettings.BufferSize]
                                : m_UnusedBuffers.Dequeue();
                                for (var i = 0; i < streamSettings.BufferSize; i++)
                                {
                                    buffer[i] = 0;
                                }

                                m_Socket.Receive(buffer);
                                if (buffer[0] == streamSettings.ErrorCheck)
                                {
                                    if (streamActive)
                                    {
                                        m_BufferQueue.Enqueue(buffer);

                                        if (isRecording)
                                        {
                                            playbackData.AddToActiveBuffer(buffer);
                                        }

                                        Buffer.BlockCopy(buffer, streamSettings.FrameNumberOffset, frameNumArray, 0,
                                            streamSettings.FrameNumberSize);

                                        var frameNum = frameNumArray[0];
                                        if (useDebug)
                                        {
                                            if (m_LastFrameNum != frameNum - 1)
                                                Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum,
                                                    m_LastFrameNum);
                                        }

                                        m_LastFrameNum = frameNum;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.Message);
                            }
                        }

                        if (m_BufferQueue.Count > k_MaxBufferQueue)
                            m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue());

                        Thread.Sleep(1);
                    }
                }).Start();
            }
        }

        public override void StartPlaybackDataUsage()
        {
            if (isSource && ! isRecording)
            {
                SetReaderStreamSettings();
                playbackData.CreatePlaybackBuffer(streamSettings, m_TakeNumber);
                isRecording = true;

                m_TakeNumber++;
            }
        }

        public override void SetReaderStreamSettings()
        {
            streamReader.UseStreamReaderSettings();
        }

        public override void StopPlaybackDataUsage()
        {
            isRecording = false;
            if (playbackData != null)
                playbackData.StopActivePlaybackBuffer();
        }

        public override void UpdateCurrentFrameBuffer(bool force = false)
        {
            if (m_BufferQueue.Count == 0)
                return;

            if (m_BufferQueue.Count > catchupThreshold)
            {
                for (var i = 0; i < catchupSize; i++)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue()); // Throw out an old frame
                }
            }

            var buffer = m_BufferQueue.Dequeue();

            if (force || isSource)
                streamReader.UpdateStreamData(ref buffer, 0);

            m_UnusedBuffers.Enqueue(buffer);
        }

        public override void StreamSourceUpdate()
        {
            if (!isSource && isRecording)
                StopPlaybackDataUsage();

            if (!streamActive || !isSource)
                return;

            if (useDebug)
            {
                if (m_BufferQueue.Count > catchupSize)
                    Debug.LogWarning(string.Format("{0} is larger than Catchup Size of {1} Dropping Frames!",
                        m_BufferQueue.Count, catchupSize));
            }

            UpdateCurrentFrameBuffer();
        }

        public override void OnStreamSettingsChange()
        {
            StopPlaybackDataUsage();
            if (m_CurrentBufferSize != streamSettings.BufferSize)
            {
                m_UnusedBuffers.Clear();
                m_BufferQueue.Clear();
            }
            else
            {
                while (m_BufferQueue.Count > 0)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue());
                }
            }

            if(playbackData != null)
                playbackData.WarmUpPlaybackBuffer(streamSettings);

            var current = m_UnusedBuffers.Count;
            if (current >= k_BufferPrewarm)
                return;

            for (var i = current; i < k_BufferPrewarm; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[streamSettings.BufferSize]);
            }
        }
    }
}
