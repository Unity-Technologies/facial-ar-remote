using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class StreamReader
    {
        byte[] m_Buffer = new byte[1024];
        public IStreamSource streamSource { get; set; }
        public IStreamRecorder recorder { get; set; }
        public IData<FaceData> faceDataOutput { get; set; }
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        /// <summary>
        /// Reads stream source and enqueue packets. This can be called from a separate thread.
        /// </summary>
        public void Read()
        {
            if (streamSource == null)
                return;

            var stream = streamSource.stream;

            if (stream == null)
                return;
            
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
        public void Dequeue()
        {
            var memoryStream = default(MemoryStream);

            while (m_Queue.TryDequeue(out memoryStream))
            {
                if (recorder != null && recorder.isRecording)
                {
                    var count = (int)memoryStream.Position;
                    var bytes = GetBuffer(count);
                    memoryStream.Position = 0;
                    Read(memoryStream, bytes, count);
                    recorder.Record(bytes, count);
                }

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

            var payloadSize = GetPayloadSize(bytes.ToStruct<PacketDescriptor>());
            bytes = GetBuffer(payloadSize);
            Read(input, bytes, payloadSize);
            output.Write(bytes, 0, payloadSize);
        }

        int GetPayloadSize(PacketDescriptor descriptor)
        {
            switch (descriptor.type)
            {
                case PacketType.Face:
                    return GetFacePayloadSize(descriptor.version);
            }

            return 0;
        }

        int GetFacePayloadSize(int version)
        {   
            switch (version)
            {
                default:
                    return Marshal.SizeOf<FaceData>();
            }
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
            
            if (faceDataOutput != null)
                faceDataOutput.data = data;
        }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }
    }
}
