using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace PerformanceRecorder
{
    public class ARStreamAsset : ScriptableObject
    {
        [SerializeField]
        byte[] m_Bytes = {};

        public byte[] bytes
        {
            get { return m_Bytes; }
            set { m_Bytes = value; }
        }
    }
}
