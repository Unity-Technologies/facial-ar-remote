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

        public static T Read<T>(this Stream stream, byte[] buffer = null) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            
            if (buffer == null)
                buffer = new byte[size];
            else if (buffer.Length < size)
                throw new Exception("The provided buffer is too small");
            
            Read(stream, buffer, size);

            return buffer.ToStruct<T>();
        }

        public static bool ReadFaceData(this Stream stream, int version, out FaceData data, byte[] buffer = null)
        {
            data = default(FaceData);
            
            try
            {
                switch (version)
                {
                    default:
                        data = Read<FaceData>(stream, buffer); break;
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
