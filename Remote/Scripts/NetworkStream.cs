using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IStreamSource" />
    /// <summary>
    /// A network-based stream source
    /// Sets up a listen server on the given port to which Clients connect
    /// </summary>
    public class NetworkStream : MonoBehaviour, IStreamSource
    {
        /// <summary>
        /// Maximum buffer queue size, after which old frames are discarded
        /// </summary>
        const int k_MaxBufferQueue = 512;

        /// <summary>
        /// Value for "backlog" argument in Socket.Listen
        /// </summary>
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

        [SerializeField]
        [Tooltip("(Optional) Manual override to use a specific stream recorder. Default behavior is to use GetComponentInChildren on this object")]
        GameObject m_StreamRecorderOverride;

        int m_LastFrameNum;

        bool m_Running;

        Socket m_Socket;
        int m_TakeNumber;
        IStreamRecorder m_StreamRecorder;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>();
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>();

        public bool recording { get; private set; }

        public IStreamReader streamReader { private get; set; }

        public bool active
        {
            get { return m_Socket != null && m_Socket.Connected; }
        }

        public IStreamSettings streamSettings { get { return m_StreamSettings; } }

        void Start()
        {
            if (m_StreamSettings == null)
            {
                Debug.LogErrorFormat("No Stream Setting set on {0}. Unable to run Server.", this);
                enabled = false;
                return;
            }

            m_StreamRecorder = m_StreamRecorderOverride
                ? m_StreamRecorderOverride.GetComponentInChildren<IStreamRecorder>()
                : GetComponentInChildren<IStreamRecorder>();

            if (m_StreamRecorder == null)
                Debug.LogWarningFormat("No Stream Recorder found in {0}. You will not be able to record anything.", this);

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
                    Debug.LogErrorFormat("Error creating listen socket on address {0} : {1}", connectionAddress, e);
                }

                new Thread(() =>
                {
                    // Block until timeout or successful connection
                    m_Socket = m_Socket.Accept();
                    Debug.Log(string.Format("Client connected on {0}", connectionAddress));

                    var frameNumArray = new int[1];
                    var bufferSize = m_StreamSettings.BufferSize;

                    while (m_Running)
                    {
                        var source = streamReader.streamSource;
                        if (m_Socket.Connected && source != null && source.Equals(this))
                        {
                            try
                            {
                                if (m_StreamSettings == null || m_StreamSettings.BufferSize != bufferSize)
                                {
                                    Debug.LogError("Settings changed while connnected. Please exit play mode before changing settings");
                                    break;
                                }

                                var buffer = m_UnusedBuffers.Count == 0 ? new byte[bufferSize] : m_UnusedBuffers.Dequeue();

                                for (var i = 0; i < bufferSize; i++)
                                {
                                    buffer[i] = 0;
                                }

                                m_Socket.Receive(buffer);
                                // Receive can fail and return an empty buffer
                                if (buffer[0] == m_StreamSettings.ErrorCheck)
                                {
                                    m_BufferQueue.Enqueue(buffer);

                                    if (recording)
                                        m_StreamRecorder.AddDataToRecording(buffer);

                                    Buffer.BlockCopy(buffer, m_StreamSettings.FrameNumberOffset, frameNumArray, 0,
                                        m_StreamSettings.FrameNumberSize);

                                    var frameNum = frameNumArray[0];
                                    if (streamReader.useDebug && m_LastFrameNum != frameNum - 1)
                                        Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum,  m_LastFrameNum);

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
            if (m_StreamRecorder == null)
                return;

            if (streamReader.streamSource.Equals(this) && !recording)
            {
                m_StreamRecorder.StartRecording(m_StreamSettings, m_TakeNumber);
                recording = true;

                m_TakeNumber++;
            }
        }

        public void StopRecording()
        {
            if (m_StreamRecorder == null)
                return;

            recording = false;
            m_StreamRecorder.FinishRecording();
        }

        void UpdateCurrentFrameBuffer()
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

            if (streamReader.streamSource.Equals(this))
                streamReader.UpdateStreamData(buffer);

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

        void OnDestroy()
        {
            m_Running = false;
        }
    }
}
