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
        [Tooltip("Blend shape location to be overridden.")]
        BlendShapeLocation m_Location;

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

        /// <summary>
        /// Enables the use of these override values on a blend shape
        /// </summary>
        public bool useOverride
        {
            get { return m_UseOverride; }
            set { m_UseOverride = value; }
        }

        /// <summary>
        /// Blend shape name to be overridden
        /// </summary>
        public BlendShapeLocation location { get { return m_Location; } }
        
        /// <summary>
        /// Smoothing to apply to blend shape values coming from the stream reader
        /// </summary>
        public float blendShapeSmoothing { get { return m_BlendShapeSmoothing; } }
        
        /// <summary>
        /// Min threshold of change to register as a new blend shape value
        /// </summary>
        public float blendShapeThreshold { get { return m_BlendShapeThreshold; } }

        /// <summary>
        /// Offsets the zero value of the blend shape.
        /// </summary>
        public float blendShapeOffset
        {
            get { return m_BlendShapeOffset; }
            set { m_BlendShapeOffset = value; }
        }

        /// <summary>
        /// Scaling coefficient applied to the blend shape values from the stream reader
        /// </summary>
        public float blendShapeCoefficient { get { return m_BlendShapeCoefficient; } }
        
        /// <summary>
        /// Max value a scaled blend shape can reach
        /// </summary>
        public float blendShapeMax { get { return m_BlendShapeMax; } }

        BlendShapeOverride(){}

        public BlendShapeOverride(BlendShapeLocation location)
        {
            m_Location = location;
        }
    }
}
