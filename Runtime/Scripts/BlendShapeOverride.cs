using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Stores the override values for an individual blend shape.
    /// </summary>
    [Serializable]
    public class BlendShapeOverride
    {
        [SerializeField]
        [Tooltip("Enables the use of these override values on a blend shape.")]
        bool m_UseOverride;

        [SerializeField]
        [Tooltip("Blend shape name to be overridden.")]
        string m_Name;

        [Range(0f, 1)]
        [SerializeField]
        [Tooltip("Smoothing to apply to blend shape values coming from the stream reader.")]
        float m_BlendShapeSmoothing = 0.1f;

        [Range(0, 0.1f)]
        [SerializeField]
        [Tooltip("Min threshold of change to register as a new blend shape value.")]
        float m_BlendShapeThreshold = 0.01f;

        [Range(-100, 100)]
        [SerializeField]
        [Tooltip("Offsets the zero value of the blend shape.")]
        float m_BlendShapeOffset;

        [Range(0, 200f)]
        [SerializeField]
        [Tooltip("Scaling coefficient applied to the blend shape values from the stream reader.")]
        float m_BlendShapeCoefficient = 120f;

        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Max value a scaled blend shape can reach.")]
        float m_BlendShapeMax = 100f;

        public bool useOverride { get { return m_UseOverride; } }
        public string name { get { return m_Name; } }
        public float blendShapeSmoothing { get { return m_BlendShapeSmoothing; } }
        public float blendShapeThreshold { get { return m_BlendShapeThreshold; } }
        public float blendShapeOffset { get { return m_BlendShapeOffset; } }
        public float blendShapeCoefficient { get { return m_BlendShapeCoefficient; } }
        public float blendShapeMax { get { return m_BlendShapeMax; } }

        BlendShapeOverride(){}

        public BlendShapeOverride(string name)
        {
            m_Name = name;
        }
    }
}
