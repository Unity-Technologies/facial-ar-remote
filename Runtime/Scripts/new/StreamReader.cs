using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public class StreamReader
    {
        byte[] m_Buffer = new byte[1024];
        public IStreamSource streamSource { get; set; }
        public IStreamRecorder recorder { get; set; }
        public IData<FaceData> faceDataOutput { get; set; }
        ConcurrentQueue<FaceData> m_FaceDataQueue = new ConcurrentQueue<FaceData>();

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

        /// <summary>
        /// Dequeue packets into outputs. Call this one from main thread.
        /// </summary>
        public void Dequeue()
        {
            Dequeue<FaceData>(m_FaceDataQueue, faceDataOutput);
        }

        void Dequeue<T>(ConcurrentQueue<T> queue, IData<T> output) where T : struct
        {
            var data = default(T);
            while (queue.TryDequeue(out data))
            {
                output.data = data;
            }
        }

        T Read<T>(Stream stream) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var bytes = GetBuffer(size);
            var readByteCount = stream.Read(bytes, 0, size);

            if (readByteCount != size)
                throw new Exception("Invalid read byte count");

            Record(bytes, size);

            return bytes.ToStruct<T>();
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
            m_FaceDataQueue.Enqueue(data);
        }

        byte[] GetBuffer(int size)
        {
            if (m_Buffer == null || m_Buffer.Length < size)
                m_Buffer = new byte[size];

            return m_Buffer;
        }
    }
}
