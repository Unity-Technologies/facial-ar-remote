using System;
using System.IO;
using UnityEngine;

namespace PerformanceRecorder
{
    [Serializable]
    public class AssetStreamSource : IStreamSource
    {
        [SerializeField]
        ARStreamAsset m_Asset;
        MemoryStream m_MemoryStream = null;
        
        public Stream stream
        {
            get
            {
                if (m_MemoryStream == null && m_Asset != null)
                    m_MemoryStream = new MemoryStream(m_Asset.bytes);

                return m_MemoryStream;
            }
        }

        public ARStreamAsset asset
        {
            get { return m_Asset; }
            set
            {
                if (m_Asset != value)
                    DisposeMemoryStream();
                
                m_Asset = value;
            }
        }

        void DisposeMemoryStream()
        {
            if (m_MemoryStream != null)
            {
                m_MemoryStream.Dispose();
                m_MemoryStream = null;
            }
        }
    }
}
