using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote.Unity.Labs.FacialRemote
{
    [Serializable]
    public class BlendShapeOverride
    {
        [SerializeField]
        bool m_UseOverride;

        [SerializeField]
        string m_Name;

        [Range(0f, 1)]
        [SerializeField]
        float m_BlendShapeSmoothing = 0.1f;

        [Range(0, 0.1f)]
        [SerializeField]
        float m_BlendShapeThreshold = 0.01f;

        [Range(0, 200f)]
        [SerializeField]
        float m_BlendShapeCoefficient = 120f;

        [Range(0, 100)]
        [SerializeField]
        float m_BlendShapeMax = 100f;

        public bool useOverride { get { return m_UseOverride; } set { m_UseOverride = value; } }
        public string name { get { return m_Name; } }
        public float blendShapeSmoothing { get { return m_BlendShapeSmoothing; } }
        public float blendShapeThreshold { get { return m_BlendShapeThreshold; } }
        public float blendShapeCoefficient { get { return m_BlendShapeCoefficient; } }
        public float blendShapeMax { get { return m_BlendShapeMax; } }

        BlendShapeOverride(){}

        public BlendShapeOverride(string name)
        {
            m_Name = name;
        }
    }
}
