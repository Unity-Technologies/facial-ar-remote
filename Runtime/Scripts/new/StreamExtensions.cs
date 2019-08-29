using System;
using System.IO;
using System.Runtime.InteropServices;

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
                    throw new Exception("Reached end of stream");

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

        public static bool TryRead<T>(this Stream stream, out T data, byte[] bytes = null) where T : struct
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

        public static bool TryReadPoseData(this Stream stream, int version, out PoseData data, byte[] bytes = null)
        {
            data = default(PoseData);

            try
            {
                switch (version)
                {
                    default:
                        data = Read<PoseData>(stream, bytes); break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool TryReadFaceData(this Stream stream, int version, out FaceData data, byte[] bytes = null)
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

        public static bool TryReadCommand(this Stream stream, int version, out Command data, byte[] bytes = null)
        {
            data = default(Command);

            try
            {
                switch (version)
                {
                    default:
                        data = Read<Command>(stream, bytes); break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        
        public static bool TryReadCommandInt(this Stream stream, int version, out CommandInt data, byte[] bytes = null)
        {
            data = default(CommandInt);

            try
            {
                switch (version)
                {
                    default:
                        data = Read<CommandInt>(stream, bytes); break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool TryReadVirtualCameraState(this Stream stream, int version, out VirtualCameraState data, byte[] bytes = null)
        {
            data = default(VirtualCameraState);

            try
            {
                switch (version)
                {
                    default:
                        data = Read<VirtualCameraState>(stream, bytes); break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void Write<T>(this Stream stream, T data) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            stream.Write(data.ToBytes(), 0, size);
        }
    }
}
