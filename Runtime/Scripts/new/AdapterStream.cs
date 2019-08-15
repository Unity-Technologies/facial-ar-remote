using System.IO;
using Unity.Labs.FacialRemote;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public enum AdapterVersion
    {
        V1,
        V2
    }

    public class AdapterStream : MemoryStream
    {
        byte[] m_Buffer = new byte[1024];
        int m_RemainingBytes = 0;
        public Stream input { get; set; }
        public AdapterVersion version { get; set; }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0 || input == null)
                return 0;

            var remainingBytes = m_RemainingBytes;

            if (remainingBytes == 0)
            {
                var size = GetSize();
                var readBuffer = GetBuffer(size);
                var readOffset = 0;
            
                do {
                    var readBytes = input.Read(readBuffer, readOffset, size - readOffset);

                    if (readBytes == 0)
                        return 0;

                    readOffset += readBytes;

                } while(readOffset < count);
                
                var desc = new PacketDescriptor()
                {
                    type = PacketType.Face,
                    version = 0
                };
                var data = GetFaceData(readBuffer);

                m_RemainingBytes = Marshal.SizeOf<PacketDescriptor>();
                m_RemainingBytes += Marshal.SizeOf<FaceData>();

                Position = 0;
                this.Write<PacketDescriptor>(desc);
                this.Write<FaceData>(data);
                Flush();
                Position = 0;
            }
            else if (m_RemainingBytes < count)
            {
                m_RemainingBytes = 0;
                return base.Read(buffer, offset, remainingBytes);
            }
            
            m_RemainingBytes -= count;

            return base.Read(buffer, offset, count);
        }

        int GetSize()
        {
            if (version == AdapterVersion.V1)
                return Marshal.SizeOf<StreamBufferDataV1>();
            else if (version == AdapterVersion.V2)
                return Marshal.SizeOf<StreamBufferDataV2>();
            return 0;
        }

        FaceData GetFaceData(byte[] buffer)
        {
            if (version == AdapterVersion.V1)
            {
                var data = buffer.ToStruct<StreamBufferDataV1>();
                return new FaceData()
                {
                    id = 0,
                    timeStamp = data.FrameTime,
                    blendShapeValues = data.BlendshapeValues,
                };
            }
            else if (version == AdapterVersion.V2)
            {
                var data = buffer.ToStruct<StreamBufferDataV2>();
                return new FaceData()
                {
                    id = 0,
                    timeStamp = data.FrameTime,
                    blendShapeValues = data.BlendshapeValues,
                };
            }

            return default(FaceData);
        }
    }

    public class AdapterSource : IStreamSource
    {
        AdapterStream m_AdapterStream = new AdapterStream();
        public IStreamSource streamSource { get; set; }
        public AdapterVersion version
        {
            get { return m_AdapterStream.version; }
            set { m_AdapterStream.version = value; }
        }

        public Stream stream
        {
            get
            {
                if (streamSource == null)
                    m_AdapterStream.input = null;
                else
                    m_AdapterStream.input = streamSource.stream;
                
                return m_AdapterStream;
            }
        }
    }
}
