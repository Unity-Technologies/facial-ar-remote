using System;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public struct BlendShapeIndexData
    {
        public int index;
        public string name;

        public BlendShapeIndexData(int index, string name)
        {
            this.index = index;
            this.name = name;
        }
    }
}
