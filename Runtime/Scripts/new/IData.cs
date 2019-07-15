using System.IO;

namespace PerformanceRecorder
{
    public interface IData<T> where T : struct
    {
        T data { get; set; }
    }
}
