using System.Runtime.InteropServices;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public enum CommandType
    {
        StartRecording,
        StopRecording
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Command
    {
        public CommandType type;

        public Command(CommandType t)
        {
            type = t;
        }
    }
}
