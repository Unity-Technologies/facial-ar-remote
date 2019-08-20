using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public abstract class PacketRecorder<T> : IPacketBuffer where T : struct
    {
        RecyclableMemoryStreamManager m_Manager = new RecyclableMemoryStreamManager();
        MemoryStream m_MemoryStream;

        public bool isRecording { get; private set; }
        public PacketType packetType { get { return GetPacketType(); } }

        public PacketRecorder()
        {
            m_MemoryStream = m_Manager.GetStream();
        }

        public void Clear()
        {
            m_MemoryStream.Seek(0, SeekOrigin.Begin);
        }

        public void StartRecording()
        {
            Clear();
            
            isRecording = true;
        }

        public void Record(T data)
        {
            if (!isRecording)
                throw new Exception("Can't record: Recoding hasn't started");
            
            m_MemoryStream.Write<T>(data);
        }

        public void StopRecording()
        {
            isRecording = false;
        }

        public byte[] GetBuffer(out long length)
        {
            if (isRecording)
                throw new Exception("Can't get buffer while recoding");

            length = m_MemoryStream.Position;
            return m_MemoryStream.GetBuffer();
        }

        protected abstract PacketType GetPacketType();
    }

    public class PoseDataRecorder : PacketRecorder<PoseData>
    {
        protected override PacketType GetPacketType()
        {
            return PacketType.Pose;
        }
    }

    public class FaceDataRecorder : PacketRecorder<FaceData>
    {
        protected override PacketType GetPacketType()
        {
            return PacketType.Face;
        }
    }
}
