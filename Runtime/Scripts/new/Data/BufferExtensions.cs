using System;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public static class BufferExtensions
    {
        public static T ToStruct<T>(this byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] ToBytes<T>(this T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            var buffer = new byte[size];
            var handle = default(GCHandle);

            try
            {
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr<T>(str, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }

            return buffer;
        }
    }
}
