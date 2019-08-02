using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public abstract class PacketRecorder<T> where T : struct
    {
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        MemoryStream m_MemoryStream;

        public bool isRecording { get; private set; }

        public PacketRecorder()
        {
            m_MemoryStream = m_Manager.GetStream();
        }

        public void StartRecording()
        {
            m_MemoryStream.Seek(0, SeekOrigin.Begin);
            m_MemoryStream.Write(PacketDescriptor.Get(GetPacketType()).ToBytes(), 0, PacketDescriptor.DescriptorSize);

            isRecording = true;
        }

        public void Record(T data)
        {
            if (!isRecording)
                throw new Exception("Can't record: Recoding hasn't started");
            
            m_MemoryStream.Write(data.ToBytes(), 0, Marshal.SizeOf<T>());
        }

        public void StopRecording()
        {
            isRecording = false;
        }

        public byte[] GetBuffer()
        {
            if (isRecording)
                throw new Exception("Can't get buffer while recoding");

            return m_MemoryStream.GetBuffer();
        }

        protected abstract PacketType GetPacketType();
    }

    public class FaceDataRecorder : PacketRecorder<FaceData>
    {
        protected override PacketType GetPacketType()
        {
            return PacketType.Face;
        }
    }
}
