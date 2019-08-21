using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    /// <summary>
    /// Stores the mappings between BlendShape indices and BlendShapeLocations of a SkinnedMeshRenderer.
    /// </summary>
    public class BlendShapeMap : ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_Path;
        [SerializeField]
        List<int> m_Indices = new List<int>();
        [SerializeField]
        List<BlendShapeLocation> m_Locations = new List<BlendShapeLocation>();
        Dictionary<int, BlendShapeLocation> m_Map = new Dictionary<int, BlendShapeLocation>();

        /// <summary>
        /// Transform path to SkinnedMeshRenderer from the root
        /// </summary>
        public string path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        /// <summary>
        /// Clear the mappings
        /// </summary>
        public void Clear()
        {
            m_Map.Clear();
        }

        /// <summary>
        /// Get the BlendShapeLocation from a BlendShape index
        /// </summary>
        public BlendShapeLocation Get(int index)
        {
            BlendShapeLocation location;
            if (m_Map.TryGetValue(index, out location))
                return location;
            
            return BlendShapeLocation.Invalid;
        }

        /// <summary>
        /// Set the BlendShapeLocation of a BlendShape index
        /// </summary>
        public void Set(int index, BlendShapeLocation location)
        {
            m_Map[index] = location;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Indices = new List<int>(m_Map.Keys);
            m_Locations = new List<BlendShapeLocation>(m_Map.Values);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_Map.Clear();

            for (var i = 0; i < Math.Min(m_Locations.Count, m_Indices.Count); i++)
                Set(m_Indices[i], m_Locations[i]);
        }
    }

    /// <summary>
    /// Asset storing a collection of BlendShapeMaps for a specific rig
    /// </summary>
    [CreateAssetMenu(fileName = "BlendShape Mappings", menuName = "AR Face Capture/BlendShape Mappings")]
    public class BlendShapeMappings : ScriptableObject
    {
        [SerializeField, HideInInspector]
        List<BlendShapeMap> m_Maps = new List<BlendShapeMap>();

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("Select the source Prefab to build the BlendShapeMappings from.")]
        GameObject m_Prefab;

        /// <summary>
        /// Prefab associated with this mappings file. Editor only.
        /// </summary>
        public GameObject prefab
        {
            get { return m_Prefab; }
            private set { m_Prefab = value; }
        }
#endif
        /// <summary>
        /// Array of BlendShapeMap that this BlendShapeMappings object contains.
        /// </summary>
        public BlendShapeMap[] maps
        {
            get { return m_Maps.ToArray(); }
            set { m_Maps = new List<BlendShapeMap>(value); }
        }
    }
}
