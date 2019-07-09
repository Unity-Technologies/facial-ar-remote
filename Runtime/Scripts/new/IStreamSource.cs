using System.IO;

namespace PerformanceRecorder
{
    public interface IStreamSource
    {
        Stream stream { get; }
    }
}
