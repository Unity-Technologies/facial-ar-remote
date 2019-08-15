using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ARStreamDescriptor
    {
        public int version;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacketBufferDescriptor
    {
        public PacketDescriptor packetDescriptor;
        public int length;
    }

    public class ARStreamWriter : IDisposable
    {
        static readonly int s_Version = 0;
        Stream m_Stream;

        public ARStreamWriter(Stream stream)
        {
            m_Stream = stream;

            var descriptor = new ARStreamDescriptor
            {
                version = s_Version
            };

            m_Stream.Write<ARStreamDescriptor>(descriptor);
        }

        public void Dispose()
        {
            if (m_Stream != null)
                m_Stream = null;
        }

        public void Write(IPacketBuffer packetBuffer)
        {
            if (m_Stream == null)
                throw new ObjectDisposedException("Write stream has been disposed");
            
            var length = default(long);
            var buffer = packetBuffer.GetBuffer(out length);
            var descriptor = new PacketBufferDescriptor
            {
                packetDescriptor = PacketDescriptor.Get(packetBuffer.packetType),
                length = (int)length
            };

            m_Stream.Write<PacketBufferDescriptor>(descriptor);
            m_Stream.Write(buffer, 0, (int)length);
        }
    }
}
