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
#if UNITY_EDITOR
        public delegate void Callback(BlendShapesController contorller);
        public static event Callback controllerEnabled;
        public static event Callback controllerDisabled;

        void OnEnable()
        {
            controllerEnabled.Invoke(this);
        }

        void OnDisable()
        {
            controllerDisabled.Invoke(this);
        }
#endif

        [SerializeField]
        BlendShapeValues m_BlendShapeValues;

        [SerializeField]
        bool m_TrackingActive = true;

        [SerializeField]
        [Tooltip("Asset that contains the bindings between blend-shape values and SkinnnedMeshRenderer's blend-shape indices.")]
        BlendShapeMappings m_Mappings;
    
        [SerializeField]
        [Tooltip("Skinned Mesh Renders that contain the blend shapes that will be driven by this controller.")]
        SkinnedMeshRenderer[] m_SkinnedMeshRenderers = {};

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

        Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> m_Indices = new Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]>();
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
		/// <summary>
        /// All renderers with blend shape values being driven.
        /// </summary>
        public SkinnedMeshRenderer[] skinnedMeshRenderers
        {
            get { return m_SkinnedMeshRenderers; }
        }

        public Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> blendShapeIndices
        {
            get { return m_Indices; }
        }

        public IStreamReader streamReader { private get; set; }

        void Awake()
        {
            UpdateBlendShapeIndices();

            if (m_Overrides.Length != BlendShapeValues.Count)
                Array.Resize(ref m_Overrides, BlendShapeValues.Count);
        }

        void LateUpdate()
        {
            UpdateBlendShapes();
        }

        /// <summary>
        /// Updates BlendShape calculations by a time step and sets the values to the renderers.
        /// </summary>
        /// <param name="deltaTime">Time step to advance.</param>
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
                if (i >= BlendShapeValues.Count)
                    break;
                
                m_BlendShapeValues[i] = streamReader.blendShapesBuffer[i];
            }
        }

        /// <summary>
        /// Update the blend shape indices based on the incoming data stream.
        /// </summary>
        public void UpdateBlendShapeIndices()
        {
            m_Indices.Clear();

            if (mappings == null)
                return;
            
            ForeachRenderer((SkinnedMeshRenderer meshRenderer) =>
            {
                var mesh = meshRenderer.sharedMesh;
                var count = mesh.blendShapeCount;
                var indices = new BlendShapeIndexData[count];
                for (var i = 0; i < count; i++)
                {
                    var shapeName = mesh.GetBlendShapeName(i);
                    var index = -1;
                    for (var j = 0; j < mappings.blendShapeNames.Length; j++)
                    {
                        // Check using 'contains' rather than a direct comparison so that multiple blend shapes can 
                        // easily be driven by the same driver e.g. jaw and teeth
                        if (!string.IsNullOrEmpty(mappings.blendShapeNames[j]) 
                            && shapeName.Contains(mappings.blendShapeNames[j]))
                        {
                            index = j;
                            break;
                        }
                    }

                    indices[i] = new BlendShapeIndexData(index, shapeName);

                    if (index < 0)
                        Debug.LogWarningFormat("Blend shape {0} is not a valid AR blend shape", shapeName);
                }

                m_Indices.Add(meshRenderer, indices);
            });
        }

        void PostProcessValues()
        {
            for (var i = 0; i < BlendShapeValues.Count; i++)
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
            ForeachRenderer((SkinnedMeshRenderer meshRenderer) =>
            {
                var blendShapeIndexData = default(BlendShapeIndexData[]);

                if (m_Indices.TryGetValue(meshRenderer, out blendShapeIndexData))
                {
                    var indices = m_Indices[meshRenderer];
                    var length = indices.Length;
                    for (var i = 0; i < length; i++)
                    {
                        var datum = indices[i];
                        if (datum.index < 0)
                            continue;

                        meshRenderer.SetBlendShapeWeight(i, m_BlendShapeOutput[datum.index]);
                    }
                }
            });
        }

        void ForeachRenderer(Action<SkinnedMeshRenderer> action)
        {
            foreach (var skinnedMeshRenderer in m_SkinnedMeshRenderers)
            {
                if (skinnedMeshRenderer == null)
                    continue;

                if (skinnedMeshRenderer.sharedMesh == null)
                    continue;

                if (action != null)
                    action.Invoke(skinnedMeshRenderer);
            }
        }

        void OnValidate()
        {
            UpdateBlendShapeIndices();

            if (mappings == null)
                return;
            
            var blendShapeCount = BlendShapeValues.Count;
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

        bool HasOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length
                && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }
    }
}
