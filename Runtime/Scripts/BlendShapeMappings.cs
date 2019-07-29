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
        List<int> m_Indices = new List<int>();
        [SerializeField]
        List<BlendShapeLocation> m_Locations = new List<BlendShapeLocation>();

        Dictionary<int, BlendShapeLocation> m_Map = new Dictionary<int, BlendShapeLocation>();

        public string path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        public void Clear()
        {
            m_Map.Clear();
        }

        public BlendShapeLocation Get(int index)
        {
            BlendShapeLocation location;
            if (m_Map.TryGetValue(index, out location))
                return location;
            
            return BlendShapeLocation.Invalid;
        }

        public void Set(int index, BlendShapeLocation location)
        {
            m_Map[index] = location;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_Map.Clear();

            for (var i = 0; i < Math.Min(m_Locations.Count, m_Indices.Count); i++)
            {
                var location = m_Locations[i];
                
                if (location == BlendShapeLocation.Invalid)
                    continue;

                Set(m_Indices[i], location);
            }
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
