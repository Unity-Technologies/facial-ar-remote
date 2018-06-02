using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class Mapping
    {
        public string from;
        public string to;
    }

    [RequireComponent(typeof(StreamReader))]
    public abstract class StreamSource : MonoBehaviour
    {
        public bool streamActive { get; protected set; }
        protected StreamReader m_StreamReader;

        protected virtual void Awake()
        {
            m_StreamReader = GetComponent<StreamReader>();
            if (m_StreamReader == null || GetStreamSettings() == null)
            {
                enabled = false;
                return;
            }
        }

        protected virtual void Update()
        {
            if (!streamActive)
                return;

            if (m_StreamReader.streamSource != this)
            {
                streamActive = false;
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            streamActive = false;
        }

        public virtual void ActivateStreamSource()
        {
            if (m_StreamReader == null)
                return;

            if (m_StreamReader.streamSource != this)
            {
                m_StreamReader.UnSetStreamSource();
                m_StreamReader.SetStreamSource(this);
                streamActive = true;
            }
        }

        public virtual void DeactivateStreamSource()
        {
            if (m_StreamReader == null || m_StreamReader.streamSource == null)
                return;

            if (m_StreamReader.streamSource == this)
            {
                m_StreamReader.UnSetStreamSource();
                streamActive = false;
            }
        }

        public abstract IStreamSettings GetStreamSettings();
    }

    public class Server : StreamSource
    {
        const int k_BufferPrewarm = 16;
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames

        [SerializeField]
        StreamSettings m_StreamSettings;

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        int m_CatchupSize = 2;

        [SerializeField]
        bool m_UseDebug;

        [SerializeField]
        PlaybackData m_PlaybackData;

        Socket m_Socket;

        bool m_ServerActive;

        int m_LastFrameNum;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

        public StreamSettings streamSettings
        {
            get
            {
                if (m_StreamSettings == null)
                    return null;

                if (!m_StreamSettings.Initialized)
                    m_StreamSettings.Initialize();

                return m_StreamSettings;
            }
        }

        [SerializeField]
        float m_BufferSize;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        public bool useRecorder
        {
            get { return m_ServerActive && Application.isEditor && Application.isPlaying && m_PlaybackData != null; }
        }

        public bool isRecording { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;
            for (var i = 0; i < k_BufferPrewarm; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[streamSettings.BufferSize]);
            }

            if (m_PlaybackData != null)
            {
                m_PlaybackData.CreatePlaybackBuffer(streamSettings);
            }
        }

        void Start()
        {
            Debug.Log("Possible IP addresses:");
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Debug.Log(address);

                var endPoint = new IPEndPoint(address, m_Port);
                m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(100);
                m_ServerActive = true;
                m_LastFrameNum = -1;
                var connectionAddress = address;
                new Thread(() =>
                {
                    m_Socket = m_Socket.Accept();
                    Debug.Log(string.Format("Client connected on {0}", connectionAddress));

                    var frameNumArray = new int[1];

                    while (m_ServerActive)
                    {
                        if (m_Socket.Connected)
                        {
                            try
                            {
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
                                            // TODO better data copy
                                            m_PlaybackData.activeByteRecord.Add(buffer.ToArray());
                                        }

                                        Buffer.BlockCopy(buffer, streamSettings.FrameNumberOffset, frameNumArray, 0, streamSettings.FrameNumberSize);

                                        var frameNum = frameNumArray[0];
                                        if (m_UseDebug)
                                        {
                                            if (m_LastFrameNum != frameNum - 1)
                                                Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum, m_LastFrameNum);
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

        void OnDisable()
        {
            if (isRecording)
                StopRecording();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_ServerActive = false;
        }

        public override IStreamSettings GetStreamSettings()
        {
            return streamSettings;
        }

        public void StartRecording()
        {
            if (m_StreamReader.streamSource == this)
                isRecording = true;
        }

        public void StopRecording()
        {
            isRecording = false;
        }

        void DequeueBuffer()
        {
            if (m_BufferQueue.Count == 0)
                return;

            if (m_BufferQueue.Count > m_CatchupSize)
            {
                for (var i = 0; i < m_CatchupSize; i++)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue()); // Throw out an old frame
                }
            }

            var buffer = m_BufferQueue.Dequeue();

            if (m_StreamReader.streamSource == this)
                m_StreamReader.UpdateStreamData(this, ref buffer, 0);

            m_UnusedBuffers.Enqueue(buffer);
        }

        protected override void Update()
        {
            if (m_StreamReader.streamSource != this && isRecording)
                StopRecording();

            base.Update();

            m_BufferSize = m_BufferQueue.Count;
            if (m_UseDebug)
            {
                if (m_BufferSize > m_CatchupSize)
                    Debug.LogWarning(string.Format("{0} is larger than Catchup Size of {1} Dropping Frames!", m_BufferSize, m_CatchupSize));
            }

            DequeueBuffer();
        }
    }
}
