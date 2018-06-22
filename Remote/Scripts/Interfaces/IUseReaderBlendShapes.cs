using System;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Interface for getting the blend shapes buffer from the Stream Reader.
    /// </summary>
    public interface IUseReaderBlendShapes
    {
        /// <summary>
        /// Gets a reference to the blend shape buffer on the Stream Reader.
        /// </summary>
        Func<float[]> getBlendShapesBuffer { get; set; }
    }
}
