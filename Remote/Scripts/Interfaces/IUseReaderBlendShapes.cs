using System;

namespace Unity.Labs.FacialRemote 
{
    public interface IUseReaderBlendShapes
    {
        Func<float[]> getBlendShapesBuffer { get; set; }
    }
}