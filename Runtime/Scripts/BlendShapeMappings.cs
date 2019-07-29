using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class BlendShapeMap : ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_Path;
        [SerializeField]
        List<BlendShapeLocation> m_Locations = new List<BlendShapeLocation>();
        [SerializeField]
        List<int> m_Indices = new List<int>();

        Dictionary<BlendShapeLocation, int> m_Map = new Dictionary<BlendShapeLocation, int>();

        public string path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        public void Clear()
        {
            m_Map.Clear();
        }

        public int GetIndex(BlendShapeLocation location)
        {
            int index;
            if (m_Map.TryGetValue(location, out index))
                return index;
            
            return -1;
        }

        public void SetIndex(BlendShapeLocation location, int index)
        {
            m_Map[location] = index;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Locations.Clear();
            m_Indices.Clear();

            foreach (var pair in m_Map)
            {
                m_Locations.Add(pair.Key);
                m_Indices.Add(pair.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_Map.Clear();

            for (var i = 0; i < Math.Min(m_Locations.Count, m_Indices.Count); i++)
                m_Map.Add(m_Locations[i], m_Indices[i]);
        }
    }

    /// <summary>
    /// Asset defining the mapping of blend shape locations and mappings for a specific rig
    /// </summary>
    [CreateAssetMenu(fileName = "BlendShape Mappings", menuName = "AR Face Capture/BlendShape Mappings")]
    public class BlendShapeMappings : ScriptableObject, IEnumerable<BlendShapeMap>
    {
        [SerializeField]
        List<BlendShapeMap> m_Maps = new List<BlendShapeMap>();

#if UNITY_EDITOR
        [SerializeField]
        GameObject m_Prefab;

        public GameObject prefab
        {
            get { return m_Prefab; }
            private set { m_Prefab = value; }
        }
#endif
        public BlendShapeMap[] maps
        {
            get { return m_Maps.ToArray(); }
            private set { m_Maps = new List<BlendShapeMap>(value); }
        }

        public IEnumerator<BlendShapeMap> GetEnumerator()
        {
            return ((IEnumerable<BlendShapeMap>)m_Maps).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<BlendShapeMap>)m_Maps).GetEnumerator();
        }
    }
}
