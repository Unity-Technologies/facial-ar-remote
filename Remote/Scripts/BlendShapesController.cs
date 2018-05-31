using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    namespace Unity.Labs.FacialRemote
    {
        public class BlendShapesController : MonoBehaviour
        {
            [SerializeField]
            BlendShapeReader m_Reader;

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

            readonly Dictionary<SkinnedMeshRenderer, int[]> m_Indices = new Dictionary<SkinnedMeshRenderer, int[]>();
            float[] m_BlendShapes;
            float[] m_BlendShapesScaled;

            void Awake()
            {
                if (m_Reader == null || m_Reader.streamSettings == null)
                {
                    enabled = false;
                    return;
                }

                m_BlendShapes = new float[m_Reader.streamSettings.blendShapeCount];
                m_BlendShapesScaled = new float[m_Reader.streamSettings.blendShapeCount];
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

                foreach (var meshRenderer in m_SkinnedMeshRenderers)
                {
                    var mesh = meshRenderer.sharedMesh;
                    var count = mesh.blendShapeCount;
                    var indices = new int[count];
                    for (var i = 0; i < count; i++)
                    {
                        var name = mesh.GetBlendShapeName(i);
                        var lower = StreamSettings.Filter(name);
                        var index = -1;
                        foreach (var mapping in m_Reader.streamSettings.mappings)
                        {
                            if (lower.Contains(mapping.from))
                                index = m_Reader.streamSettings.locations.IndexOf(mapping.to);
                        }

                        if (index < 0)
                        {
                            for (var j = 0; j < m_Reader.streamSettings.locations.Count; j++)
                            {
                                if (lower.Contains(m_Reader.streamSettings.locations[j]))
                                {
                                    index = j;
                                    break;
                                }
                            }
                        }

                        indices[i] = index;

                        if (index < 0)
                            Debug.LogWarningFormat("Blend shape {0} is not a valid AR blend shape", name);
                    }

                    m_Indices.Add(meshRenderer, indices);
                }
            }

            void Update()
            {
                if (!m_Reader.streamActive)
                    return;

                //Interpolate blend shapes
                for (var i = 0; i < m_Reader.streamSettings.blendShapeCount; i++)
                {
                    var blendShape = m_BlendShapes[i];
                    var blendShapeTarget = m_Reader.blendShapesBuffer[i];
                    var threshold = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeThreshold : m_BlendShapeThreshold;
                    var smoothing = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeSmoothing : m_BlendShapeSmoothing;

                    if (m_Reader.trackingActive)
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

                foreach (var renderer in m_SkinnedMeshRenderers)
                {
                    var indices = m_Indices[renderer];
                    var length = indices.Length;
                    for (var i = 0; i < length; i++)
                    {
                        var index = indices[i];
                        if (index < 0)
                            continue;

                        renderer.SetBlendShapeWeight(i, m_BlendShapesScaled[index]);
                    }
                }
            }

#if UNITY_EDITOR
            void OnValidate()
            {
                if (m_Reader == null || m_Reader.streamSettings.locations ==null || m_Reader.streamSettings.locations.Count == 0)
                    return;

                if (m_Overrides.Length != m_Reader.streamSettings.blendShapeCount)
                {
                    var overridesCopy = new BlendShapeOverride[m_Reader.streamSettings.blendShapeCount];

                    foreach (var location in m_Reader.streamSettings.locations)
                    {
                        var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location);
                        if (blendShapeOverride == null)
                        {
                            blendShapeOverride = new BlendShapeOverride(location);
                        }
                        overridesCopy[m_Reader.streamSettings.locations.IndexOf(location)] = blendShapeOverride;
                    }

                    m_Overrides = overridesCopy;
                }
            }
#endif
        }

    }
}
