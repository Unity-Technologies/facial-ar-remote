using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public class StreamReader
    {
        byte[] m_Buffer = new byte[1024];
        public IStreamSource streamSource { get; set; }
        public IStreamRecorder recorder { get; set; }
        public IData<FaceData> faceDataOutput { get; set; }

        public void Read()
        {
            if (streamSource == null)
                return;

            var stream = streamSource.stream;

            if (stream == null)
                return;
            
            try
            {
                var descriptor = Read<PacketDescriptor>(stream);
                switch (descriptor.type)
                {
                    case PacketType.Face:
                        ReadFaceData(stream, descriptor);
                        break;
                }
            }
            catch {}
        }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }

        T Read<T>(Stream stream) where T : struct
        {
            var data = default(T);
            var size = Marshal.SizeOf<T>();
            var bytes = GetBuffer(size);
            var readByteCount = stream.Read(bytes, 0, size);

            if (readByteCount != size)
                throw new Exception("Invalid read byte count");
            
            data = bytes.ToStruct<T>();

            Record(bytes, size);

            return data;
        }


        void Record(byte[] bytes, int size)
        {
            if (recorder != null && recorder.isRecording)
                recorder.Record(bytes, size);
        }

        void ReadFaceData(Stream stream, PacketDescriptor descriptor)
        {
            //TODO: use descriptor's version to read the correct struct and upgrade to latest FaceData

            var data = Read<FaceData>(stream);

            if (faceDataOutput != null)
                faceDataOutput.data = data;
        }
    }
}
