using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class StreamReader
    {
        public delegate void FaceDataCallback(FaceData faceData);
        public event FaceDataCallback faceDataChanged;

        byte[] m_Buffer = new byte[1024];
        
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        /// <summary>
        /// Reads stream source and enqueue packets. This can be called from a separate thread.
        /// </summary>
        public void Read(Stream stream)
        {
            if (stream == null)
                throw new NullReferenceException();

            var memoryStream = m_Manager.GetStream();

            try
            {
                ReadPacket(stream, memoryStream);
                m_Queue.Enqueue(memoryStream);
            }
            catch (Exception)
            {
                memoryStream.Dispose();
            }
        }

        /// <summary>
        /// Dequeue packets into outputs. Call this one from main thread.
        /// </summary>
        public void Receive()
        {
            var memoryStream = default(MemoryStream);

            while (m_Queue.TryDequeue(out memoryStream))
            {
                memoryStream.Position = 0;

                var packetDescriptor = Read<PacketDescriptor>(memoryStream);

                switch(packetDescriptor.type)
                {
                    case PacketType.Face:
                        ReadFaceData(memoryStream, packetDescriptor.version);
                        break;
                }

                memoryStream.Dispose();
            }
        }

        void ReadPacket(Stream input, Stream output)
        {
            var bytes = GetBuffer(PacketDescriptor.DescriptorSize);
            Read(input, bytes, PacketDescriptor.DescriptorSize);
            output.Write(bytes, 0, PacketDescriptor.DescriptorSize);

            var descriptor = bytes.ToStruct<PacketDescriptor>();
            var payloadSize = descriptor.GetPayloadSize();
            bytes = GetBuffer(payloadSize);
            Read(input, bytes, payloadSize);
            output.Write(bytes, 0, payloadSize);
        }

        void Read(Stream stream, byte[] bytes, int count)
        {
            if (bytes.Length < count)
                throw new Exception("Read buffer too small");
            
            var offset = 0;
            
            do {
                var readBytes = stream.Read(bytes, offset, count - offset);

                if (readBytes == 0)
                    throw new Exception("Invalid read byte count");

                offset += readBytes;

            } while(offset < count);
        }

        T Read<T>(Stream stream) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var bytes = GetBuffer(size);
            
            Read(stream, bytes, size);

            return bytes.ToStruct<T>();
        }

        void ReadFaceData(Stream stream, int version)
        {
            var data = default(FaceData);

            switch (version)
            {
                default:
                    data = Read<FaceData>(stream); break;
            }
            
            faceDataChanged.Invoke(data);
        }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }
    }
}
