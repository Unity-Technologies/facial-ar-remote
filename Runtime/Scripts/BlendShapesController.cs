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
    [ExecuteAlways]
    public class BlendShapesController : MonoBehaviour, IUsesStreamReader
    {
        [SerializeField]
        BlendShapeValues m_BlendShapeValues;
        [SerializeField]
        bool m_TrackingActive = true;
        [SerializeField]
        BlendShapeMappings m_BlendShapeMappings;
    
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

        Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> m_Indices = new Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]>();
        Vector2[] m_SmoothedBlendShapes;
        float[] m_BlendShapes;

        public SkinnedMeshRenderer[] skinnedMeshRenderers
        {
            get { return m_SkinnedMeshRenderers; }
        }

        public Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> blendShapeIndices
        {
            get { return m_Indices; }
        }

        public float[] blendShapes
        {
            get { return m_BlendShapes; }
        }

        public bool isTrackingActive
        {
            get { return m_TrackingActive; }
            set { m_TrackingActive = value; }
        }

        public IStreamReader streamReader { private get; set; }

#if UNITY_EDITOR
        float m_LastTime;
#endif
        void Awake()
        {
            m_SmoothedBlendShapes = new Vector2[m_BlendShapeValues.Count];
            m_BlendShapes = new float[m_BlendShapeValues.Count];

            UpdateBlendShapeIndices();

            if (m_Overrides.Length != m_BlendShapeValues.Count)
                Array.Resize(ref m_Overrides, m_BlendShapeValues.Count);

#if UNITY_EDITOR
            m_LastTime = Time.realtimeSinceStartup;
#endif
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var currentTime = Time.realtimeSinceStartup;
                deltaTime = currentTime - m_LastTime;
                m_LastTime = currentTime;
            }
#endif

            UpdateBlendShapes(deltaTime);
        }

        public void UpdateBlendShapes(float deltaTime)
        {
            UpdateFromStreamReader(deltaTime);
            PostProcessValues(deltaTime);
            UpdateSkinnedMeshRenderers();
        }

        void UpdateFromStreamReader(float deltaTime)
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

        /// <summary>
        /// Update the blend shape indices based on the incoming data stream.
        /// </summary>
        /// <param name="settings">The stream settings used for this mapping.</param>
        public void UpdateBlendShapeIndices()
        {
            m_Indices.Clear();
            
            ForeachRenderer((SkinnedMeshRenderer meshRenderer) =>
            {
                var mesh = meshRenderer.sharedMesh;
                var count = mesh.blendShapeCount;
                var indices = new BlendShapeIndexData[count];
                for (var i = 0; i < count; i++)
                {
                    var shapeName = mesh.GetBlendShapeName(i);
                    var index = -1;
                    for (var j = 0; j < m_BlendShapeMappings.blendShapeNames.Length; j++)
                    {
                        // Check using 'contains' rather than a direct comparison so that multiple blend shapes can 
                        // easily be driven by the same driver e.g. jaw and teeth
                        if (!string.IsNullOrEmpty(m_BlendShapeMappings.blendShapeNames[j]) 
                            && shapeName.Contains(m_BlendShapeMappings.blendShapeNames[j]))
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

        void PostProcessValues(float deltaTime)
        {
            for (var i = 0; i < m_BlendShapeValues.Count; i++)
            {
                var targetValue = m_BlendShapeValues[i];
                var hasOverride = HasOverride(i);
                var blendShapeOverride = m_Overrides[i]; //TODO: this might crash
                var threshold = hasOverride ? blendShapeOverride.blendShapeThreshold : m_BlendShapeThreshold;
                var offset = hasOverride ? blendShapeOverride.blendShapeOffset : 0f;
                var smoothing = hasOverride ? blendShapeOverride.blendShapeSmoothing : m_BlendShapeSmoothing;
                var scale = hasOverride ? blendShapeOverride.blendShapeCoefficient : m_BlendShapeCoefficient;
                var maxValue = hasOverride ? blendShapeOverride.blendShapeMax : m_BlendShapeMax;
                var currentValue = m_SmoothedBlendShapes[i].x;
                var currentSpeed = m_SmoothedBlendShapes[i].y;

                if (isTrackingActive)
                {
                    if (Mathf.Abs(targetValue - currentValue) > threshold)
                        currentValue = Mathf.SmoothDamp(currentValue, targetValue, ref currentSpeed, smoothing, Mathf.Infinity, deltaTime);
                }
                else
                    currentValue = Mathf.SmoothDamp(currentValue, 0f, ref currentSpeed, m_TrackingLossSmoothing, Mathf.Infinity, deltaTime);

                m_SmoothedBlendShapes[i] = new Vector2(currentValue, currentSpeed);

                m_BlendShapes[i] = Mathf.Min(currentValue * scale + offset, maxValue);
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

                        meshRenderer.SetBlendShapeWeight(i, m_BlendShapes[datum.index]);
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

#if UNITY_EDITOR
        void OnValidate()
        {
            if (streamReader == null)
                return;

            var streamSource = streamReader.streamSource;
            if (streamSource == null)
                return;
            
            var streamSettings = streamSource.streamSettings;
            
            if (m_BlendShapeMappings == null)
                return;
            // if (streamSettings == null || streamSettings.locations == null || streamSettings.locations.Length == 0)
            //     return;

            // We do our best to keep the overrides up-to-date with current settings, but it's possible to get out of sync
            var blendShapeCount = streamSettings.BlendShapeCount;
            if (m_Overrides.Length != blendShapeCount)
            {
                var overridesCopy = new BlendShapeOverride[blendShapeCount];

                for (var i = 0; i < blendShapeCount; i++)
                {
                    //var location = streamSettings.mappings[i];
                    var location = m_BlendShapeMappings.blendShapeNames[i];
                    var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location)
                        ?? new BlendShapeOverride(location);

                    overridesCopy[i] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
            }
        }
#endif

        bool HasOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length
                && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }
    }
}
