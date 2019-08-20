using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public class PacketReader
    {
        public delegate void PoseDataCallback(PoseData data);
        public delegate void FaceDataCallback(FaceData data);
        public delegate void CommandCallback(Command data);
        public event PoseDataCallback poseDataChanged;
        public event FaceDataCallback faceDataChanged;
        public event CommandCallback commandChanged;

        byte[] m_Buffer = new byte[1024];
        
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        ConcurrentQueue<MemoryStream> m_Queue = new ConcurrentQueue<MemoryStream>();

        /// <summary>
        /// Disposes enqueued packets.
        /// </summary>
        public void Clear()
        {
            var memoryStream = default(MemoryStream);
            while (m_Queue.TryDequeue(out memoryStream))
                memoryStream.Dispose();
        }

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

                var descriptor = memoryStream.Read<PacketDescriptor>(GetBuffer(PacketDescriptor.DescriptorSize));

                switch(descriptor.type)
                {
                    case PacketType.Pose:
                        ReadPoseData(memoryStream, descriptor);
                        break;
                    case PacketType.Face:
                        ReadFaceData(memoryStream, descriptor);
                        break;
                    case PacketType.Command:
                        ReadCommand(memoryStream, descriptor);
                        break;
                }

                memoryStream.Dispose();
            }
        }

        void ReadPacket(Stream input, Stream output)
        {
            var bytes = GetBuffer(PacketDescriptor.DescriptorSize);
            input.Read(bytes, PacketDescriptor.DescriptorSize);
            output.Write(bytes, 0, PacketDescriptor.DescriptorSize);

            var descriptor = bytes.ToStruct<PacketDescriptor>();
            var payloadSize = descriptor.GetPayloadSize();
            bytes = GetBuffer(payloadSize);
            input.Read(bytes, payloadSize);
            output.Write(bytes, 0, payloadSize);
        }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }

        void ReadPoseData(Stream stream, PacketDescriptor descriptor)
        {
            var data = default(PoseData);
            if (stream.TryReadPoseData(descriptor.version, out data, GetBuffer(descriptor.GetPayloadSize())))
                poseDataChanged(data);
        }

        void ReadFaceData(Stream stream, PacketDescriptor descriptor)
        {
            var data = default(FaceData);
            if (stream.TryReadFaceData(descriptor.version, out data, GetBuffer(descriptor.GetPayloadSize())))
                faceDataChanged(data);
        }

        void ReadCommand(Stream stream, PacketDescriptor descriptor)
        {
            var data = default(Command);
            if (stream.TryReadCommand(descriptor.version, out data, GetBuffer(descriptor.GetPayloadSize())))
                commandChanged(data);
        }
    }
}
