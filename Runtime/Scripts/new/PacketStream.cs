using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace PerformanceRecorder
{
    public class PacketStream
    {
        Thread m_ReadThread;
        Thread m_WriteThread;
        PacketReader m_Reader = new PacketReader();
        PacketWriter m_Writer = new PacketWriter();

        public IStreamSource streamSource { get; set; }

        Stream stream
        {
            get
            {
                if (streamSource != null)
                    return streamSource.stream;
                
                return null;
            }
        }

        public PacketReader reader
        {
            get { return m_Reader; }
        }

        public PacketWriter writer
        {
            get { return m_Writer; }
        }

        public void Start()
        {
            SetupThreads();
        }

        public void Stop()
        {
            Dispose();
        }

        void SetupThreads()
        {
            m_ReadThread = new Thread(() =>
            {
                while (true)
                {
                    if (stream != null)
                        reader.Read(stream);
                    
                    Thread.Sleep(1);
                };
            });
            m_ReadThread.Start();

            m_WriteThread = new Thread(() =>
            {
                while (true)
                {
                    if (stream != null)
                        writer.Send(stream);
                    
                    Thread.Sleep(1);
                };
            });
            m_WriteThread.Start();
        }

        void Dispose()
        {
            AbortThread(ref m_ReadThread);
            AbortThread(ref m_WriteThread);

            m_Reader.Clear();
            m_Writer.Clear();
        }

        void AbortThread(ref Thread thread)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }
    }
}
