using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public interface IConnectedController
    {
        void SetStreamSettings(IStreamSettings streamSettings);
    }

    [Serializable]
    public class BlendShapeIndexData
    {
        public int index;
        public string name;

        public BlendShapeIndexData(int index, string name)
        {
            this.index = index;
            this.name = name;
        }
    }

    public class BlendShapesController : MonoBehaviour, IConnectedController
    {
        [SerializeField]
        StreamReader m_Reader;

        [SerializeField]
        StreamSettings m_StreamSettings;

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
        BlendShapeOverride[] m_Overrides;

        [SerializeField]
        [Range(0f, 1f)]
        float m_TrackingLossSmoothing = 0.1f;

        IStreamSettings m_ConnectedStreamSettings;

        readonly Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> m_Indices = new Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]>();
        float[] m_BlendShapes;
        float[] m_BlendShapesScaled;

        public SkinnedMeshRenderer[] skinnedMeshRenderers { get { return m_SkinnedMeshRenderers; }}
        public Dictionary<SkinnedMeshRenderer, BlendShapeIndexData[]> blendShapeIndices { get { return m_Indices; } }

        public float[] blendShapesScaled { get { return m_BlendShapesScaled; } }

        void Awake()
        {
            if (m_Reader == null)
            {
                enabled = false;
                return;
            }

            Init();
        }

        // HACK
        public void Init()
        {
            m_Reader.AddConnectedController(this);

        }

        void Start()
        {
            if (m_Reader == null)
            {
                Debug.LogWarning("Blend Shape Controller needs a Server set.");
                enabled = false;
                return;
            }

            if (m_SkinnedMeshRenderers.Length < 1 || m_SkinnedMeshRenderers.All(a => a == null))
            {
                Debug.LogWarning("Blend Shape  Controller needs a Skinned Mesh Renderer set.");
                enabled = false;
                return;
            }

            // TODO should get data from connected settings
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
                    var lower = StreamSettings.Filter(shapeName);
                    var index = -1;
                    foreach (var mapping in m_StreamSettings.mappings)
                    {
                        if (lower.Contains(mapping.from))
                            index = m_StreamSettings.locations.IndexOf(mapping.to);
                    }

                    if (index < 0)
                    {
                        for (var j = 0; j < m_StreamSettings.locations.Count; j++)
                        {
                            if (lower.Contains(m_StreamSettings.locations[j]))
                            {
                                index = j;
                                break;
                            }
                        }
                    }

                    indices[i] = new BlendShapeIndexData(index, shapeName);;

                    if (index < 0)
                        Debug.LogWarningFormat("Blend shape {0} is not a valid AR blend shape", shapeName);
                }

                m_Indices.Add(meshRenderer, indices);
            }
        }

        public void SetStreamSettings(IStreamSettings streamSettings)
        {
            if (streamSettings == null)
                return;

            m_ConnectedStreamSettings = streamSettings;
            m_BlendShapes = new float[streamSettings.BlendShapeCount];
            m_BlendShapesScaled = new float[streamSettings.BlendShapeCount];
        }

        void Update()
        {
            if (!m_Reader.streamActive)
                return;

            InterpolateBlendShapes();

            foreach (var renderer in m_SkinnedMeshRenderers)
            {
                var indices = m_Indices[renderer];
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    var datum = indices[i];
                    if (datum.index < 0)
                        continue;

                    renderer.SetBlendShapeWeight(i, m_BlendShapesScaled[datum.index]);
                }
            }
        }

        public void InterpolateBlendShapes(bool force = false)
        {
            for (var i = 0; i < m_StreamSettings.BlendShapeCount; i++)
            {
                var blendShape = m_BlendShapes[i];
                var blendShapeTarget = m_Reader.blendShapesBuffer[i];
                var threshold = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeThreshold : m_BlendShapeThreshold;
                var smoothing = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeSmoothing : m_BlendShapeSmoothing;

                if (force || m_Reader.trackingActive)
                {
                    if (Mathf.Abs(blendShapeTarget - blendShape) > threshold)
                        m_BlendShapes[i] = Mathf.Lerp(blendShapeTarget, blendShape, smoothing);
                }
                else
                {
                    m_BlendShapes[i] =  Mathf.Lerp(0f, m_BlendShapes[i], m_TrackingLossSmoothing);
                }

                if (m_Overrides[i].useOverride)
                {
                    m_BlendShapesScaled[i] = Mathf.Min(m_BlendShapes[i] * m_Overrides[i].blendShapeCoefficient, m_Overrides[i].blendShapeMax);
                }
                else
                {
                    m_BlendShapesScaled[i] = Mathf.Min(m_BlendShapes[i] * m_BlendShapeCoefficient, m_BlendShapeMax);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Reader == null || m_StreamSettings.locations ==null || m_StreamSettings.locations.Count == 0)
                return;

            if (m_Overrides.Length != m_StreamSettings.BlendShapeCount)
            {
                var overridesCopy = new BlendShapeOverride[m_StreamSettings.BlendShapeCount];

                foreach (var location in m_StreamSettings.locations)
                {
                    var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location);
                    if (blendShapeOverride == null)
                    {
                        blendShapeOverride = new BlendShapeOverride(location);
                    }
                    overridesCopy[m_StreamSettings.locations.IndexOf(location)] = blendShapeOverride;
                }

                m_Overrides = overridesCopy;
            }
        }
#endif
    }

}
