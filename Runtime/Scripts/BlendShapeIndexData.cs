namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Struct associating a blend shape index with a blend shape name.
    /// </summary>
    public struct BlendShapeIndexData
    {
        readonly int m_Index;
        readonly string m_Name;

        /// <summary>
        /// The index of the blend shape in the renderer
        /// </summary>
        public int index { get { return m_Index; } }
        
        /// <summary>
        /// The name of the blend shape in the rig
        /// </summary>
        public string name { get { return m_Name; } }

        public BlendShapeIndexData(int index, string name)
        {
            m_Index = index;
            m_Name = name;
        }
    }
}
