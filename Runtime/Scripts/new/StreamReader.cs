using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Microsoft.IO;

namespace PerformanceRecorder
{
    [Serializable]
    public class StreamReader : IDisposable
    {
        IStreamSource m_StreamSource;
        Thread m_Thread;
        byte[] m_Buffer = new byte[1024];
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        MemoryStream m_RecordStream = null;
        bool m_Recording = false;

        public byte checkByte;

        public IStreamSource streamSource
        {
            get { return m_StreamSource; }
            set { m_StreamSource = value; }
        }

        public void StartLiveStream()
        {
            StopLiveStream();

            if (streamSource == null || streamSource.stream == null)
                return;
            
            if (streamSource.stream.CanSeek)
                throw new Exception("Error: stream not compatible with live streamming");

            m_Thread = new Thread(() =>
            {
                while (true)
                {
                    var readByteCount = 0;
                    readByteCount = Read();

                    if (readByteCount > 0)
                    {
                        checkByte = m_Buffer[0];

                        if (m_Recording)
                            m_RecordStream.Write(m_Buffer, 0, readByteCount);
                    }

                    Thread.Sleep(1);
                };
            });

            m_Thread.Start();
        }

        public void StopLiveStream()
        {
            DisposeThread();
        }

        public int Read()
        {
            if (streamSource == null)
                return 0;

            var stream = streamSource.stream;

            if (stream == null || !stream.CanRead)
                return 0;

            var readByteCount = 0;

            try
            {
                readByteCount = stream.Read(m_Buffer, 0, 1);
            }
            catch (Exception) {}

            return readByteCount;
        }

        public void Dispose()
        {
            DisposeThread();
            StopRecording();
            DisposeRecording();
        }

        void DisposeThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        void DisposeRecording()
        {
            if (m_RecordStream != null)
            {
                m_RecordStream.Dispose();
                m_RecordStream = null;
            }
        }

        public void StartRecording()
        {
            DisposeRecording();
            m_RecordStream = m_Manager.GetStream();
            m_Recording = true;
        }

        public void StopRecording()
        {
            m_Recording = false;
        }

        public void SaveRecording(string path)
        {
            if (m_RecordStream != null)
            {
                using (var fileStream = File.Create(path))
                {
                    m_RecordStream.WriteTo(fileStream);
                }
            }
        }
    }
}
