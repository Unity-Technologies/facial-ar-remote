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
            Server m_Server;

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
            readonly float[] m_BlendShapes = new float[Server.BlendShapeCount];
            readonly float[] m_BlendShapesScaled = new float[Server.BlendShapeCount];

            void Start()
            {
                if (m_Server == null)
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
                        var lower = Server.Filter(name);
                        var index = -1;
                        foreach (var mapping in m_Server.mappings)
                        {
                            if (lower.Contains(mapping.from))
                                index = m_Server.locations.IndexOf(mapping.to);
                        }

                        if (index < 0)
                        {
                            for (var j = 0; j < m_Server.locations.Count; j++)
                            {
                                if (lower.Contains(m_Server.locations[j]))
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
                if (!m_Server.running)
                    return;

                //Interpolate blend shapes
                for (var i = 0; i < Server.BlendShapeCount; i++)
                {
                    var blendShape = m_BlendShapes[i];
                    var blendShapeTarget = m_Server.blendShapesBuffer[i];
                    var threshold = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeThreshold : m_BlendShapeThreshold;
                    var smoothing = m_Overrides[i].useOverride ? m_Overrides[i].blendShapeSmoothing : m_BlendShapeSmoothing;

                    if (m_Server.trackingActive)
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
                if (m_Server == null || m_Server.locations ==null || m_Server.locations.Count == 0)
                    return;

                if (m_Overrides.Length != Server.BlendShapeCount)
                {
                    var overridesCopy = new BlendShapeOverride[Server.BlendShapeCount];

                    foreach (var location in m_Server.locations)
                    {
                        var blendShapeOverride = m_Overrides.FirstOrDefault(f => f.name == location);
                        if (blendShapeOverride == null)
                        {
                            blendShapeOverride = new BlendShapeOverride(location);
                        }
                        overridesCopy[m_Server.locations.IndexOf(location)] = blendShapeOverride;
                    }

                    m_Overrides = overridesCopy;
                }
            }
#endif
        }

    }
}
