using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IUsesStreamReader" />
    /// <summary>
    /// Updates blend shape values from the stream reader to the skinned mesh renders referenced in this script.
    /// </summary>
    public class BlendShapesController : MonoBehaviour, IUsesStreamReader
    {
        [SerializeField]
        [Tooltip("Skinned Mesh Renders that contain the blend shapes that will be driven by this controller.")]
        SkinnedMeshRenderer[] m_SkinnedMeshRenderers = {};

        [Range(0f, 1)]
        [SerializeField]
        [Tooltip("Smoothing to apply to blend shape values coming from the stream reader.")]
        float m_BlendShapeSmoothing = 0.1f;

        [Range(0, 0.1f)]
        [SerializeField]
        [Tooltip("Min threshold of change to register as a new blend shape value.")]
        float m_BlendShapeThreshold = 0.01f;

        [Range(0, 200f)]
        [SerializeField]
        [Tooltip("Scaling coefficient applied to the blend shape values from the stream reader.")]
        float m_BlendShapeCoefficient = 120f;

        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Max value a scaled blend shape can reach.")]
        float m_BlendShapeMax = 100f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Smoothing to return to zero pose of character's blend shapes when tracking is lost.")]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("Overrides settings for individual blend shapes.")]
        BlendShapeOverride[] m_Overrides;

        readonly Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> m_Indices = new Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]>();
        float[] m_BlendShapes;

        IStreamSettings m_LastStreamSettings;

        public SkinnedMeshRenderer[] skinnedMeshRenderers { get { return m_SkinnedMeshRenderers; }}
        public Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> blendShapeIndices { get { return m_Indices; } }

        public float[] blendShapesScaled { get; private set; }

        public IStreamReader streamReader { private get; set; }

        void Start()
        {
            var filteredList = new List<SkinnedMeshRenderer>();
            foreach (var renderer in m_SkinnedMeshRenderers)
            {
                if (renderer == null)
                {
                    Debug.LogWarning("Null element in SkinnedMeshRenderer list in " + this);
                    continue;
                }

                if (renderer.sharedMesh == null)
                {
                    Debug.LogWarning("Missing mesh in " + renderer);
                    continue;
                }

                filteredList.Add(renderer);
            }

            m_SkinnedMeshRenderers = filteredList.ToArray();

            if (m_SkinnedMeshRenderers.Length < 1)
            {
                Debug.LogWarning("Blend Shape Controller has no valid Skinned Mesh Renderers.");
                enabled = false;
            }
        }

        void Update()
        {
            var streamSource = streamReader.streamSource;
            if (streamSource == null || !streamSource.active)
                return;

            var streamSettings = streamSource.streamSettings;
            if (streamSettings != m_LastStreamSettings)
                UpdateBlendShapeIndices(streamSettings);

            InterpolateBlendShapes();

            foreach (var meshRenderer in m_SkinnedMeshRenderers)
            {
                var indices = m_Indices[meshRenderer];
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    var datum = indices[i];
                    if (datum.index < 0)
                        continue;

                    meshRenderer.SetBlendShapeWeight(i, blendShapesScaled[datum.index]);
                }
            }
        }

        public void UpdateBlendShapeIndices(IStreamSettings settings)
        {
            m_LastStreamSettings = settings;
            var blendShapeCount = settings.BlendShapeCount;
            m_BlendShapes = new float[blendShapeCount];
            blendShapesScaled = new float[blendShapeCount];
            m_Indices.Clear();
            var streamSettings = streamReader.streamSource.streamSettings;
            foreach (var meshRenderer in m_SkinnedMeshRenderers)
            {
                var mesh = meshRenderer.sharedMesh;
                var count = mesh.blendShapeCount;
                var indices = new BlendShapeIndexData[count];
                for (var i = 0; i < count; i++)
                {
                    var shapeName = mesh.GetBlendShapeName(i);
                    var index = -1;
                    foreach (var mapping in streamSettings.mappings)
                    {
                        if (shapeName.Contains(mapping.from))
                            index = Array.IndexOf(streamSettings.locations, mapping.to);
                    }

                    if (index < 0)
                    {
                        for (var j = 0; j < streamSettings.locations.Length; j++)
                        {
                            if (shapeName.Contains(streamSettings.locations[j]))
                            {
                                index = j;
                                break;
                            }
                        }
                    }

                    indices[i] = new BlendShapeIndexData(index, shapeName);

                    if (index < 0)
                        Debug.LogWarningFormat("Blend shape {0} is not a valid AR blend shape", shapeName);
                }

                m_Indices.Add(meshRenderer, indices);
            }
        }

        public void InterpolateBlendShapes(bool force = false)
        {
            var streamSettings = streamReader.streamSource.streamSettings;
            for (var i = 0; i < streamSettings.BlendShapeCount; i++)
            {
                var blendShape = m_BlendShapes[i];
                var blendShapeTarget = streamReader.blendShapesBuffer[i];
                var useOverride = UseOverride(i);
                var blendShapeOverride = m_Overrides[i];
                var threshold = useOverride ? blendShapeOverride.blendShapeThreshold : m_BlendShapeThreshold;
                var offset = useOverride ? blendShapeOverride.blendShapeOffset : 0f;
                var smoothing = useOverride ? blendShapeOverride.blendShapeSmoothing : m_BlendShapeSmoothing;

                if (force || streamReader.trackingActive)
                {
                    if (Mathf.Abs(blendShapeTarget - blendShape) > threshold)
                        m_BlendShapes[i] = Mathf.Lerp(blendShapeTarget, blendShape, smoothing);
                }
                else
                {
                    m_BlendShapes[i] =  Mathf.Lerp(0f, blendShape, m_TrackingLossSmoothing);
                }

                if (useOverride)
                {
                    blendShapesScaled[i] = Mathf.Min(blendShape * blendShapeOverride.blendShapeCoefficient + offset,
                        blendShapeOverride.blendShapeMax);
                }
                else
                {
                    blendShapesScaled[i] = Mathf.Min(blendShape * m_BlendShapeCoefficient, m_BlendShapeMax);
                }
            }
        }

        void OnValidate()
        {
            if (streamReader == null)
                return;

            var streamSource = streamReader.streamSource;
            if (streamSource == null)
                return;

            var streamSettings = streamSource.streamSettings;
            // if (streamSettings == null || streamSettings.locations == null || streamSettings.locations.Length == 0)
            //     return;

            // We do our best to keep the overrides up-to-date with current settings, but it's possible to get out of sync
            var blendshapeCount = streamSettings.BlendShapeCount;
            if (m_Overrides.Length != blendshapeCount)
            {
#if UNITY_EDITOR
                var overridesCopy = new BlendShapeOverride[blendshapeCount];

                for (var i = 0; i < blendshapeCount; i++)
                {
                    var location = streamSettings.locations[i];
                    var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location)
                        ?? new BlendShapeOverride(location);

                    overridesCopy[i] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
#endif
            }
        }

        bool UseOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length
                && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }
    }
}
