namespace Unity.Labs.FacialRemote
{
    public struct BlendShapeIndexData
    {
        readonly int m_Index;
        readonly string m_Name;

        public int index { get { return m_Index; } }
        public string name { get { return m_Name; } }

        public BlendShapeIndexData(int index, string name)
        {
            m_Index = index;
            m_Name = name;
        }
    }
}
