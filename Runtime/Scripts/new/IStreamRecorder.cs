using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public interface IStreamRecorder
    {
        bool isRecording { get; }
        void StartRecording();
        void StopRecording();
        void Record(byte[] bytes, int size);
    }
}
