using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class BlendShapesController : MonoBehaviour, IUseStreamSettings, IUseReaderActive, IUseReaderBlendShapes
    {
        [SerializeField]
        SkinnedMeshRenderer[] m_SkinnedMeshRenderers = {};

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

        [SerializeField]
        [Range(0f, 1f)]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        BlendShapeOverride[] m_Overrides;

        readonly Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> m_Indices = new Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]>();
        float[] m_BlendShapes;
        float[] m_BlendShapesScaled;

        public SkinnedMeshRenderer[] skinnedMeshRenderers { get { return m_SkinnedMeshRenderers; }}
        public Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> blendShapeIndices { get { return m_Indices; } }

        public float[] blendShapesScaled { get { return m_BlendShapesScaled; } }

        protected IStreamSettings streamSettings { get { return getStreamSettings(); } }
        public Func<IStreamSettings> getStreamSettings { get; set; }
        IStreamSettings readerStreamSettings { get { return getReaderStreamSettings(); } }
        public Func<IStreamSettings> getReaderStreamSettings { get; set; }
        bool isReaderStreamActive { get { return isStreamActive(); } }
        public Func<bool> isStreamActive { get; set; }
        bool isReaderTrackingActive { get { return isTrackingActive(); } }
        public Func<bool> isTrackingActive { get; set; }
        float[] readerBlendShapesBuffer { get { return getBlendShapesBuffer(); } }
        public Func<float[]> getBlendShapesBuffer { get; set; }

        [NonSerialized]
        [HideInInspector]
        public bool connected;

        void Start()
        {
            if (m_SkinnedMeshRenderers.Length < 1 || m_SkinnedMeshRenderers.All(a => a == null))
            {
                Debug.LogWarning("Blend Shape Controller needs a Skinned Mesh Renderer set.");
                enabled = false;
            }
        }

        void Update()
        {
            if (!connected || !isReaderStreamActive)
                return;

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

                    meshRenderer.SetBlendShapeWeight(i, m_BlendShapesScaled[datum.index]);
                }
            }
        }

        public void OnStreamSettingsChange()
        {
            m_BlendShapes = new float[streamSettings.BlendShapeCount];
            m_BlendShapesScaled = new float[streamSettings.BlendShapeCount];

            SetupBlendShapeIndices();
        }

        public void SetupBlendShapeIndices()
        {
            m_Indices.Clear();
            foreach (var meshRenderer in m_SkinnedMeshRenderers)
            {
                var mesh = meshRenderer.sharedMesh;
                var count = mesh.blendShapeCount;
                var indices = new BlendShapeIndexData[count];
                for (var i = 0; i < count; i++)
                {
                    var shapeName = mesh.GetBlendShapeName(i);
                    var index = -1;
                    foreach (var mapping in readerStreamSettings.mappings)
                    {
                        if (shapeName.Contains(mapping.from))
                            index = Array.IndexOf(readerStreamSettings.locations, mapping.to);
                    }

                    if (index < 0)
                    {
                        for (var j = 0; j < readerStreamSettings.locations.Length; j++)
                        {
                            if (shapeName.Contains(readerStreamSettings.locations[j]))
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
            for (var i = 0; i < streamSettings.BlendShapeCount; i++)
            {
                var blendShape = m_BlendShapes[i];
                var blendShapeTarget = readerBlendShapesBuffer[i];
                var threshold = UseOverride(i) ? m_Overrides[i].blendShapeThreshold : m_BlendShapeThreshold;
                var offset = UseOverride(i) ? m_Overrides[i].blendShapeOffset : 0f;
                var smoothing = UseOverride(i) ? m_Overrides[i].blendShapeSmoothing : m_BlendShapeSmoothing;

                if (force || isReaderTrackingActive)
                {
                    if (Mathf.Abs(blendShapeTarget - blendShape) > threshold)
                        m_BlendShapes[i] = Mathf.Lerp(blendShapeTarget, blendShape, smoothing);
                }
                else
                {
                    m_BlendShapes[i] =  Mathf.Lerp(0f, m_BlendShapes[i], m_TrackingLossSmoothing);
                }

                if (UseOverride(i))
                {
                    m_BlendShapesScaled[i] = Mathf.Min((m_BlendShapes[i] + offset) * m_Overrides[i].blendShapeCoefficient,
                        m_Overrides[i].blendShapeMax);
                }
                else
                {
                    m_BlendShapesScaled[i] = Mathf.Min(m_BlendShapes[i] * m_BlendShapeCoefficient, m_BlendShapeMax);
                }
            }
        }

        public void ValidateOverrides()
        {
            if (!connected || readerStreamSettings.locations == null || readerStreamSettings.locations.Length == 0)
                return;

            if (m_Overrides.Length != readerStreamSettings.BlendShapeCount)
            {
#if UNITY_EDITOR
                var overridesCopy = new BlendShapeOverride[readerStreamSettings.BlendShapeCount];

                for (var i = 0; i < readerStreamSettings.locations.Length; i++)
                {
                    var location = readerStreamSettings.locations[i];
                    var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location);
                    if (blendShapeOverride == null)
                    {
                        blendShapeOverride = new BlendShapeOverride(location);
                    }

                    overridesCopy[i] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
#endif
            }
        }

        bool UseOverride(int index)
        {
            return m_Overrides != null && index < m_Overrides.Length && m_Overrides[index] != null && m_Overrides[index].useOverride;
        }

        void OnValidate()
        {
            ValidateOverrides();
        }
    }
}
