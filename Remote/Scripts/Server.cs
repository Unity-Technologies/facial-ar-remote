using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class Server : MonoBehaviour, IStreamSource
    {
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames
        const int k_MaxConnections = 64;

        [SerializeField]
        [Tooltip("Contains the buffer layout and blend shape name and mapping information for interpreting the data stream from a connected device.")]
        StreamSettings m_StreamSettings;

        [SerializeField]
        [Tooltip("Port number that the device will connect to, be sure to have this match the port set on the device.")]
        int m_Port = 9000;

        [SerializeField]
        [Tooltip("Threshold for number of missed frames before trying to skip frames with catchup size.")]
        int m_CatchupThreshold = 16;

        [SerializeField]
        [Tooltip("How many frames should be processed at once if the editor falls behind processing the device stream. In an active recording these frames are still captured even if they are skipped in editor.")]
        int m_CatchupSize = 3;

        Socket m_Socket;
        bool m_Running;
        int m_LastFrameNum;
        int m_TakeNumber;
        int m_CurrentBufferSize = -1;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>();
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>();

        public IStreamReader streamReader { private get; set; }

        public bool recording { get; private set; }

        public bool active
        {
            get { return m_Socket != null && m_Socket.Connected; }
        }

        public IStreamSettings streamSettings
        {
            get { return m_StreamSettings; }
        }

        void Start()
        {
            if (m_StreamSettings == null)
            {
                Debug.LogErrorFormat("No Stream Setting set on {0}! Unable to run Server!", this);
                enabled = false;
                return;
            }

            m_TakeNumber = 0;
            Debug.Log("Possible IP addresses:");
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                var connectionAddress = address;
                Debug.Log(connectionAddress);
                try
                {
                    var endPoint = new IPEndPoint(connectionAddress, m_Port);
                    m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    m_Socket.Bind(endPoint);
                    m_Socket.Listen(k_MaxConnections);
                    m_LastFrameNum = -1;
                    m_Running = true;
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

                    while (m_Running)
                    {
                        var source = streamReader.streamSource;
                        if (m_Socket.Connected && source != null && source.Equals(this))
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
                                    m_BufferQueue.Enqueue(buffer);

                                    if (recording)
                                        streamReader.playbackData.AddToActiveBuffer(buffer);

                                    Buffer.BlockCopy(buffer, streamSettings.FrameNumberOffset, frameNumArray, 0,
                                        streamSettings.FrameNumberSize);

                                    var frameNum = frameNumArray[0];
                                    if (streamReader.useDebug)
                                    {
                                        if (m_LastFrameNum != frameNum - 1)
                                            Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum,
                                                m_LastFrameNum);
                                    }

                                    m_LastFrameNum = frameNum;
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

                    if (m_Socket != null)
                        m_Socket.Disconnect(false);
                }).Start();
            }
        }

        public void StartRecording()
        {
            if (streamReader.streamSource.Equals(this) && !recording)
            {
                streamReader.streamSettings = m_StreamSettings;
                streamReader.playbackData.CreatePlaybackBuffer(m_StreamSettings, m_TakeNumber);
                recording = true;

                m_TakeNumber++;
            }
        }

        public void StopRecording()
        {
            recording = false;
            var playbackData = streamReader.playbackData;
            if (playbackData != null)
                playbackData.FinishRecording();
        }

        void UpdateCurrentFrameBuffer(bool force = false)
        {
            if (m_BufferQueue.Count == 0)
                return;

            // Throw out some old frames if we are too far behind
            if (m_BufferQueue.Count > m_CatchupThreshold)
            {
                for (var i = 0; i < m_CatchupSize; i++)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue());
                }
            }

            var buffer = m_BufferQueue.Dequeue();

            if (force || streamReader.streamSource.Equals(this))
                streamReader.UpdateStreamData(ref buffer, 0);

            m_UnusedBuffers.Enqueue(buffer);
        }

        public void StreamSourceUpdate()
        {
            var source = streamReader.streamSource;
            var notSource = source == null || !source.Equals(this);
            if (notSource && recording)
                StopRecording();

            if (notSource || !active)
                return;

            if (streamReader.useDebug)
            {
                if (m_BufferQueue.Count > m_CatchupSize)
                    Debug.LogWarning(string.Format("{0} is larger than Catchup Size of {1} Dropping Frames!",
                        m_BufferQueue.Count, m_CatchupSize));
            }

            UpdateCurrentFrameBuffer();
        }

        public void OnStreamSettingsChanged(IStreamSettings settings)
        {
            StopRecording();
            var bufferSize = settings.BufferSize;
            if (m_CurrentBufferSize != bufferSize)
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

            m_CurrentBufferSize = bufferSize;

            var current = m_UnusedBuffers.Count;
            if (current >= m_CatchupThreshold)
                return;

            for (var i = current; i < m_CatchupThreshold; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[bufferSize]);
            }
        }

        void OnDestroy()
        {
            m_Running = false;
        }
    }
}
