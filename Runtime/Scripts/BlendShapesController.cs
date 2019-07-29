using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IUsesStreamReader" />
    /// <summary>
    /// Updates blend shape values from the stream reader to the skinned mesh renders referenced in this script.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BlendShapesController : MonoBehaviour, IUsesStreamReader
    {
        [SerializeField, HideInInspector]
        BlendShapeValues m_BlendShapeValues;

        [SerializeField, HideInInspector]
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
        BlendShapeOverride[] m_Overrides;
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
            private set { m_Mappings = value; }
        }

        public IStreamReader streamReader { private get; set; }

        void Awake()
        {
            if (m_Overrides.Length != m_BlendShapeValues.Count)
                Array.Resize(ref m_Overrides, m_BlendShapeValues.Count);
        }

        void LateUpdate()
        {
            UpdateBlendShapes();
        }

        /// <summary>
        /// Updates BlendShape calculations by a time step and sets the values to the renderers.
        public void UpdateBlendShapes()
        {
            UpdateFromStreamReader();
            PostProcessValues();
            UpdateSkinnedMeshRenderers();
        }

        void UpdateFromStreamReader()
        {
            if (streamReader == null)
                return;
            
            var streamSource = streamReader.streamSource;
            if (streamSource == null || !streamSource.isActive)
                return;
            
            isTrackingActive = !streamReader.faceTrackingLost;

            var streamSettings = streamReader.streamSource.streamSettings;
            for (var i = 0; i < streamSettings.BlendShapeCount; ++i)
            {
                if (i >= m_BlendShapeValues.Count)
                    break;
                
                m_BlendShapeValues[i] = streamReader.blendShapesBuffer[i];
            }
        }

        void PostProcessValues()
        {
            for (var i = 0; i < m_BlendShapeValues.Count; i++)
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
            PrepareMappings();

            for (var i = 0; i < m_SkinnedMeshRenderers.Count; ++i)
            {
                var skinnedMeshRenderer = m_SkinnedMeshRenderers[i];

                if (skinnedMeshRenderer == null)
                    continue;

                var map = m_Maps[i];

                for (var j = 0; j < 52; j++)
                {
                    var location = (BlendShapeLocation)j;
                    var index = map.GetIndex(location);

                    if (index == -1)
                        continue;

                    skinnedMeshRenderer.SetBlendShapeWeight(index, m_BlendShapeOutput[j]);
                }
            }
        }

        void PrepareMappings()
        {
            if (m_CurrentMappings == mappings)
                return;

            m_SkinnedMeshRenderers.Clear();
            m_Maps = null;

            if (mappings != null)
            {
                m_Maps = mappings.maps;

                foreach (var map in m_Maps)
                    m_SkinnedMeshRenderers.Add(GetSkinnedMeshRenderer(map.path));
            }

            m_CurrentMappings = mappings;
        }

        SkinnedMeshRenderer GetSkinnedMeshRenderer(string path)
        {
            var targetTransform = transform.Find(path);

            if (targetTransform == null)
                return null;

            return targetTransform.GetComponent<SkinnedMeshRenderer>();
        }

        /*
        void OnValidate()
        {
            UpdateBlendShapeIndices();

            if (mappings == null)
                return;
            
            var blendShapeCount = m_BlendShapeValues.Count;
            if (m_Overrides.Length != blendShapeCount)
            {
                var overridesCopy = new BlendShapeOverride[blendShapeCount];

                for (var i = 0; i < blendShapeCount; i++)
                {
                    var location = mappings.blendShapeNames[i];
                    var blendShapeOverride = Array.Find(m_Overrides, f => f.name == location)
                                            ?? new BlendShapeOverride(location);
                    overridesCopy[i] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
            }
        }
        */

        bool HasOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length
                && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }
    }
}
