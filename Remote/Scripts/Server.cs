using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public interface IStreamSource
    {
        bool streamActive { get;}
        bool streamThreadActive { get; set;}
        Func<bool> isStreamSource { get; set; }
        Func<StreamReader> getStreamReader { get; set; }

        Func<PlaybackData> getPlaybackData { get; set; }
        Func<bool> getUseDebug { get; set; }

        Func<IStreamSettings> getStreamSettings { get; set; }

        void StartStreamThread();
        void ActivateStreamSource();
        void DeactivateStreamSource();

//        Action StreamSettingsChangeCallback { get; }
        void OnStreamSettingsChangeChange();
        void SetStreamSettings();
    }

    public interface IServerSettings
    {
        Func<int> getPortNumber { get; set; }
        Func<int> getFrameCatchupSize { get; set; }
    }

    public abstract class StreamSource : IStreamSource
    {
        public Func<bool> isStreamSource { get; set; }
        public Func<IStreamSettings> getStreamSettings { get; set; }
        public Func<PlaybackData> getPlaybackData { get; set; }
        public Func<bool> getUseDebug { get; set; }
        public Func<StreamReader> getStreamReader { get; set; }

        public bool streamActive { get { return isStreamSource(); } }
        public bool streamThreadActive { get; set; }

//        public Action StreamSettingsChangeCallback { get; private set; }

        protected StreamReader streamReader { get { return getStreamReader(); } }
        protected IStreamSettings streamSettings { get { return getStreamSettings(); } }
        protected PlaybackData playbackData { get { return getPlaybackData(); } }
        protected bool useDebug { get { return getUseDebug(); } }

        public virtual void Initialize()
        {
//            StreamSettingsChangeCallback += OnStreamSettingsChangeChange;
        }

        public abstract void StreamSourceUpdate();
        public abstract void OnStreamSettingsChangeChange();
        public abstract void SetStreamSettings();

        public virtual void ActivateStreamSource()
        {
            if (!isStreamSource())
            {
                streamReader.UnSetStreamSource();
                streamReader.SetStreamSource(this);
            }
        }

        public virtual void DeactivateStreamSource()
        {
            if (isStreamSource())
            {
                streamReader.UnSetStreamSource();
            }
        }

        public abstract void StartStreamThread();
        public abstract void StartPlaybackDataUsage();
        public abstract void StopPlaybackDataUsage();
        public abstract void UpdateCurrentFrameBuffer(bool force = false);
//        protected abstract IStreamSettings GetStreamSettings();
    }

    public class Server : StreamSource, IServerSettings
    {
        const int k_BufferPrewarm = 16;
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames

        public Func<int> getPortNumber { get; set; }
        public Func<int> getFrameCatchupSize { get; set; }

//        IStreamSettings streamSettings { get { return getStreamSettings(); }  }
        int portNumber {get { return getPortNumber(); } }
        int catchupSize {get { return getFrameCatchupSize(); } }

        Socket m_Socket;
        int m_LastFrameNum;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

        public bool useRecorder
        {
            get { return streamThreadActive && Application.isEditor && Application.isPlaying && playbackData != null; }
        }

        public bool isRecording { get; private set; }

//        public override void Initialize()
//        {
//            base.Initialize();
//
//            for (var i = 0; i < k_BufferPrewarm; i++)
//            {
//                m_UnusedBuffers.Enqueue(new byte[streamSettings.BufferSize]);
//            }
//
//            if (playbackData != null)
//            {
//                playbackData.CreatePlaybackBuffer(streamSettings);
//            }
//        }

//        protected override IStreamSettings GetStreamSettings()
//        {
//            return streamSettings;
//        }

        public override void StartStreamThread()
        {
            Debug.Log("Possible IP addresses:");
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Debug.Log(address);

                var endPoint = new IPEndPoint(address, portNumber);
                m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(100);
                m_LastFrameNum = -1;
                streamThreadActive = true;
                var connectionAddress = address;
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
                                            playbackData.activeByteRecord.Add(buffer.ToArray());
                                        }

                                        Buffer.BlockCopy(buffer, streamSettings.FrameNumberOffset, frameNumArray, 0, streamSettings.FrameNumberSize);

                                        var frameNum = frameNumArray[0];
                                        if (useDebug)
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

        public override void StartPlaybackDataUsage()
        {
            if (isStreamSource())
            {
                SetStreamSettings();
                isRecording = true;
            }
        }

        public override void SetStreamSettings()
        {
            streamReader.UseStreamReaderSettings();
        }

        public override void StopPlaybackDataUsage()
        {
            isRecording = false;
        }

        public override void UpdateCurrentFrameBuffer(bool force = false)
        {
            if (m_BufferQueue.Count == 0)
                return;

            if (m_BufferQueue.Count > catchupSize)
            {
                for (var i = 0; i < catchupSize; i++)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue()); // Throw out an old frame
                }
            }

            var buffer = m_BufferQueue.Dequeue();

            if (force || isStreamSource())
                streamReader.UpdateStreamData(ref buffer, 0);

            m_UnusedBuffers.Enqueue(buffer);
        }

        public override void StreamSourceUpdate()
        {
            if (!isStreamSource() && isRecording)
                StopPlaybackDataUsage();

            if (!streamActive || !isStreamSource())
                return;

//            m_BufferSize = m_BufferQueue.Count;
//            if (useDebug)
//            {
//                if (m_BufferSize > catchupSize)
//                    Debug.LogWarning(string.Format("{0} is larger than Catchup Size of {1} Dropping Frames!", m_BufferSize, catchupSize));
//            }

            UpdateCurrentFrameBuffer();
        }

        public override void OnStreamSettingsChangeChange()
        {
            StopPlaybackDataUsage();

            m_UnusedBuffers.Clear();
            m_BufferQueue.Clear();

            for (var i = 0; i < k_BufferPrewarm; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[streamSettings.BufferSize]);
            }

            if (playbackData != null)
            {
                playbackData.CreatePlaybackBuffer(streamSettings);
            }
        }
    }
}
