using System;
using System.Collections.Generic;
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

    [RequireComponent(typeof(BlendShapeReader))]
    public class Server : MonoBehaviour
    {
        const int k_BufferPrewarm = 16;
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        int m_CatchupSize = 2;

        [SerializeField]
        bool m_UseDebug;

        [SerializeField]
        PlaybackData m_PlaybackData;

        Socket m_Socket;

        [SerializeField]
        StreamSettings m_StreamSettings;

        BlendShapeReader m_BlendShapeReader;

        public bool running { get; private set; }
        int m_LastFrameNum;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

        [SerializeField]
        float m_BufferSize;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        public bool useRecorder
        {
            get { return Application.isEditor && Application.isPlaying && m_PlaybackData != null; }
        }

        public bool isRecording { get; private set; }

        void Awake()
        {
            Application.targetFrameRate = 60;
            for (var i = 0; i < k_BufferPrewarm; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[m_StreamSettings.BufferSize]);
            }

            if (m_PlaybackData != null)
            {
                m_PlaybackData.CreatePlaybackBuffer();
            }

            m_BlendShapeReader = GetComponent<BlendShapeReader>();
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
                running = true;
                m_LastFrameNum = -1;
                var connectionAddress = address;
                new Thread(() =>
                {
                    m_Socket = m_Socket.Accept();
                    Debug.Log(string.Format("Client connected on {0}", connectionAddress));

                    var frameNumArray = new int[1];

                    while (running)
                    {
                        if (m_Socket.Connected)
                        {
                            try
                            {
                                var buffer = m_UnusedBuffers.Count == 0 ? new byte[m_StreamSettings.BufferSize] : m_UnusedBuffers.Dequeue();
                                for (var i = 0; i < m_StreamSettings.BufferSize; i++)
                                {
                                    buffer[i] = 0;
                                }

                                m_Socket.Receive(buffer);
                                if (buffer[0] == m_StreamSettings.errorCheck)
                                {
                                    m_BufferQueue.Enqueue(buffer);

                                    if (m_PlaybackData != null)
                                    {
                                        if (isRecording)
                                        {
                                            m_PlaybackData.activeByteQueue.Enqueue(buffer);
                                        }
                                    }

                                    Buffer.BlockCopy(buffer, m_StreamSettings.FrameNumberOffset, frameNumArray, 0, sizeof(int));

                                    var frameNum = frameNumArray[0];
                                    if (m_UseDebug)
                                    {
                                        if (m_LastFrameNum != frameNum - 1)
                                            Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum, m_LastFrameNum);
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
                }).Start();
            }
        }

        void OnDisable()
        {
            if (isRecording)
                StopRecording();
        }

        public void StartRecording()
        {
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

            m_BlendShapeReader.UpdateStreamData(ref buffer, 0);
            m_UnusedBuffers.Enqueue(buffer);
        }

        void Update()
        {
            m_BufferSize = m_BufferQueue.Count;
            if (m_UseDebug)
            {
                if (m_BufferSize > m_CatchupSize)
                    Debug.LogWarning(string.Format("{0} is larger than Catchup Size of {1} Dropping Frames!", m_BufferSize, m_CatchupSize));
            }

            DequeueBuffer();
        }

        void OnDestroy()
        {
            running = false;
        }
    }

}
