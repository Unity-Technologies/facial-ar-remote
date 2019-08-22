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
        bool m_Running = false;

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
            if (!m_Running)
            {
                m_Running = true;
                SetupThreads();
            }
        }

        public void Stop()
        {
            if (m_Running)
            {
                m_Running = false;

                m_Reader.Clear();
                m_Writer.Clear();

                m_ReadThread.Join();
                m_WriteThread.Join();
                
                m_ReadThread = null;
                m_WriteThread = null;
            }
        }

        void SetupThreads()
        {
            m_ReadThread = new Thread(() =>
            {
                while (m_Running)
                {
                    if (stream != null && stream.CanRead)
                        reader.Read(stream);
                    
                    Thread.Sleep(1);
                };
            });
            m_ReadThread.Start();

            m_WriteThread = new Thread(() =>
            {
                while (m_Running)
                {
                    if (stream != null && stream.CanWrite)
                        writer.Send(stream);
                    
                    Thread.Sleep(1);
                };
            });
            m_WriteThread.Start();
        }
    }
}
