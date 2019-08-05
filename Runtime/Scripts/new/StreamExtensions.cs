using System;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public static class StreamExtensions
    {
        public static void Read(this Stream stream, byte[] bytes, int count)
        {
            if (bytes == null)
                throw new NullReferenceException("Output buffer is null");

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

        public static T Read<T>(this Stream stream, byte[] bytes = null) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            
            if (bytes == null)
                bytes = new byte[size];
            else if (bytes.Length < size)
                throw new Exception("The provided buffer is too small");
            
            Read(stream, bytes, size);

            return bytes.ToStruct<T>();
        }

        public static bool Read<T>(this Stream stream, out T data, byte[] bytes = null) where T : struct
        {
            data = default(T);

            try
            {
                data = Read<T>(stream, bytes);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool ReadFaceData(this Stream stream, int version, out FaceData data, byte[] bytes = null)
        {
            data = default(FaceData);

            try
            {
                switch (version)
                {
                    default:
                        data = Read<FaceData>(stream, bytes); break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
