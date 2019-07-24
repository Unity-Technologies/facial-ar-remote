using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class StreamWriter
    {
        static readonly int PacketDescriptorSize = Marshal.SizeOf<PacketDescriptor>();
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        public IStreamSource streamSource { get; set; }
        public int packetCount
        {
            get { return m_Queue.Count; }
        }

        Stream outputStream
        {
            get
            {
                if (streamSource != null)
                    return streamSource.stream;
                
                return null;
            }
        }

        /// <summary>
        /// Enqueues packet's bytes and adds header. Can be called from main thread.
        /// </summary>
        public void Write(FaceData faceData)
        {
            Write<FaceData>(PacketDescriptor.DescriptorFace, faceData);
        }

        /// <summary>
        /// Flushes queue into the stream. Can be called from a separate thread.
        /// </summary>
        public void Send()
        {
            var stream = default(MemoryStream);

            while (m_Queue.TryDequeue(out stream))
            {
                var count = (int)stream.Position;

                if (outputStream != null)
                    outputStream.Write(stream.GetBuffer(), 0 , count);
                
                stream.Dispose();
            }

            if (outputStream != null)
                outputStream.Flush();
        }

        void Write<T>(PacketDescriptor descriptor, T packet) where T : struct
        {
            var stream = m_Manager.GetStream();
            int size = Marshal.SizeOf<T>();

            stream.Write(descriptor.ToBytes(), 0, PacketDescriptorSize);
            stream.Write(packet.ToBytes(), 0, size);
            
            m_Queue.Enqueue(stream);
        }
    }
}
