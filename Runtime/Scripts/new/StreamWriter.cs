using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class StreamWriter
    {
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        public int packetCount
        {
            get { return m_Queue.Count; }
        }

        /// <summary>
        /// Enqueues packet's bytes and adds header. Can be called from main thread.
        /// </summary>
        public void Write(FaceData faceData)
        {
            Write<FaceData>(PacketDescriptor.DescriptorFace, faceData);
        }

        public void Write(byte[] bytes, int count)
        {
            var stream = m_Manager.GetStream();
            stream.Write(bytes, 0, count);
            m_Queue.Enqueue(stream);
        }

        /// <summary>
        /// Flushes queue into the stream. Can be called from a separate thread.
        /// </summary>
        public void Send(Stream stream)
        {
            if (stream == null)
                throw new NullReferenceException();
            
            var memoryStream = default(MemoryStream);

            while (m_Queue.TryDequeue(out memoryStream))
            {
                var count = (int)memoryStream.Position;

                stream.Write(memoryStream.GetBuffer(), 0 , count);
                
                memoryStream.Dispose();
            }

            stream.Flush();
        }

        void Write<T>(PacketDescriptor descriptor, T packet) where T : struct
        {
            var stream = m_Manager.GetStream();
            int size = Marshal.SizeOf<T>();

            stream.Write(descriptor.ToBytes(), 0, PacketDescriptor.DescriptorSize);
            stream.Write(packet.ToBytes(), 0, size);
            
            m_Queue.Enqueue(stream);
        }
    }
}
