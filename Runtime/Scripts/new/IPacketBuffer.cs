using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.IO;

namespace PerformanceRecorder
{
    public interface IPacketBuffer
    {
        PacketType packetType { get; }
        byte[] GetBuffer(out long length);
    }
}
