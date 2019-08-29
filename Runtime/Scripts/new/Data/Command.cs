using System.Runtime.InteropServices;

namespace PerformanceRecorder
{
    public enum CommandType
    {
        StartRecording,
        StopRecording,
        FreezeCamera,
        UnfreezeCamera
    }

    public enum CommandIntType
    {
        SetCameraRig,
        SetLensSize
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
    
    [StructLayout(LayoutKind.Sequential)]
    public struct CommandInt
    {
        public CommandIntType type;
        public int i;

        public CommandInt(CommandIntType t, int i)
        {
            type = t;
            this.i = i;
        }
    }
}
