using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Updates blend shape values from the stream reader to the skinned mesh renders referenced in this script.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class BlendShapesController : Actor
    {
        [SerializeField]
        BlendShapeValues m_BlendShapeValues;

        [SerializeField]
        bool m_TrackingActive = true;

        [SerializeField]
        [Tooltip("Asset that contains the bindings between blend-shape values and SkinnnedMeshRenderer's blend-shape indices.")]
        BlendShapeMappings m_Mappings;

        [Range(0f, 1)]
        [SerializeField]
        [Tooltip("Smoothing to apply to blend shape values coming from the stream reader.")]
        float m_Smoothing = 0.1f;

        [Range(0, 0.1f)]
        [SerializeField]
        [Tooltip("Min threshold of change to register as a new blend shape value.")]
        float m_Threshold = 0.01f;

        [Range(0, 200f)]
        [SerializeField]
        [Tooltip("Scaling coefficient applied to the blend shape values from the stream reader.")]
        float m_Multiplier = 120f;

        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Max value a scaled blend shape can reach.")]
        float m_Maximum = 100f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Smoothing to return to zero pose of character's blend shapes when tracking is lost.")]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("Overrides settings for individual blend shapes.")]
        BlendShapeOverride[] m_Overrides = new BlendShapeOverride[BlendShapeValues.count];
        BlendShapeMappings m_CurrentMappings;
        BlendShapeMap[] m_Maps = null;
        List<SkinnedMeshRenderer> m_SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        BlendShapeValues m_SmoothedBlendShapes;
        BlendShapeValues m_BlendShapeOutput;

        public BlendShapeValues blendShapeInput
        {
            get { return m_BlendShapeValues; }
            set { m_BlendShapeValues = value; }
        }

        public BlendShapeValues blendShapeOutput
        {
            get { return m_BlendShapeOutput; }
        }

        public bool isTrackingActive
        {
            get { return m_TrackingActive; }
            set { m_TrackingActive = value; }
        }

        public BlendShapeMappings mappings
        {
            get { return m_Mappings; }
            set { m_Mappings = value; }
        }

        void LateUpdate()
        {
            UpdateBlendShapes();
        }

        /// <summary>
        /// Updates BlendShape calculations and sets the values to the renderers.
        /// </summary>
        public void UpdateBlendShapes()
        {
            PostProcessValues();
            UpdateSkinnedMeshRenderers();
        }

        /// <summary>
        /// Updates internal references from the mappings asset.
        /// </summary>
        public void Rebind()
        {
            BlendShapesMappingsUtils.Prepare(transform, mappings, ref m_SkinnedMeshRenderers, ref m_Maps);
        }

        void PostProcessValues()
        {
            for (var i = 0; i < BlendShapeValues.count; i++)
            {
                var targetValue = m_BlendShapeValues[i];
                var hasOverride = HasOverride(i);
                var blendShapeOverride = m_Overrides[i]; //TODO: this might crash
                var threshold = hasOverride ? blendShapeOverride.blendShapeThreshold : m_Threshold;
                var offset = hasOverride ? blendShapeOverride.blendShapeOffset : 0f;
                var smoothing = hasOverride ? blendShapeOverride.blendShapeSmoothing : m_Smoothing;
                var scale = hasOverride ? blendShapeOverride.blendShapeCoefficient : m_Multiplier;
                var maxValue = hasOverride ? blendShapeOverride.blendShapeMax : m_Maximum;
                var currentValue = m_SmoothedBlendShapes[i];

                if (isTrackingActive)
                {
                    if (Mathf.Abs(targetValue - currentValue) > threshold)
                        currentValue = Mathf.Lerp(targetValue, currentValue, smoothing);
                }
                else
                    currentValue = Mathf.Lerp(0f, currentValue, m_TrackingLossSmoothing);

                m_SmoothedBlendShapes[i] = currentValue;

                m_BlendShapeOutput[i] = Mathf.Min(currentValue * scale + offset, maxValue);
            }
        }

        void UpdateSkinnedMeshRenderers()
        {
            if (m_CurrentMappings != mappings)
            {
                Rebind();
                m_CurrentMappings = mappings;
            }

            for (var i = 0; i < m_SkinnedMeshRenderers.Count; ++i)
            {
                var skinnedMeshRenderer = m_SkinnedMeshRenderers[i];

                if (skinnedMeshRenderer == null)
                    continue;

                var mesh = skinnedMeshRenderer.sharedMesh;
                var map = m_Maps[i];

                for (var j = 0; j < mesh.blendShapeCount; j++)
                {
                    var location = map.Get(j);

                    if (location == BlendShapeLocation.Invalid)
                        continue;

                    skinnedMeshRenderer.SetBlendShapeWeight(j, m_BlendShapeOutput.GetValue(location));
                }
            }
        }

        void OnValidate()
        {
            if (m_Overrides.Length != BlendShapeValues.count)
            {
                var overridesCopy = new BlendShapeOverride[BlendShapeValues.count];

                for (var i = 0; i < BlendShapeValues.count; i++)
                {
                    var location = (BlendShapeLocation)i;
                    var blendShapeOverride = Array.Find(m_Overrides, f => f.location == location)
                                            ?? new BlendShapeOverride(location);
                    overridesCopy[i] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
            }
        }

        bool HasOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length
                && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }
    }
}
