using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Asset defining the mapping of blend shape locations and mappings for a specific rig
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "BlendShape Mappings", menuName = "AR Face Capture/BlendShape Mappings")]
    public class BlendShapeMappings : ScriptableObject
    {
        [SerializeField]
        StreamSettings m_StreamSettings;
        
        [SerializeField]
        [Tooltip("Names of the data streams with their index in the array being their relative location")]
        string[] m_LocationIdentifiers;

        [SerializeField]
        [Tooltip("Blend shape names from the rig. These must be row/index aligned with the location identifiers")]
        string[] m_BlendShapeNames;

        /// <summary>
        /// Blend shape names from the rig.
        /// </summary>
        public string[] blendShapeNames { get { return m_BlendShapeNames; }}
        
#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.EditorUtility.SetDirty(this);

            if (m_StreamSettings == null)
                return;
            
            if (m_LocationIdentifiers.Length == 0 && m_StreamSettings.locations.Length != 0 
                || m_LocationIdentifiers.Length != m_StreamSettings.BlendShapeCount)
            {
                var locs = new List<string>();
                foreach (var location in m_StreamSettings.locations)
                {
                    locs.Add(location);
                }

                m_LocationIdentifiers = locs.ToArray();
            }
            
            if (blendShapeNames.Length != m_LocationIdentifiers.Length)
            {
                var maps = new List<string>();
                foreach (var bsn in blendShapeNames)
                {
                    maps.Add(bsn);
                }

                for (var i = maps.Count; i < m_LocationIdentifiers.Length; i++)
                {
                    maps.Add("");
                }

                m_BlendShapeNames = maps.ToArray();
            }
        }
#endif
    }
}
