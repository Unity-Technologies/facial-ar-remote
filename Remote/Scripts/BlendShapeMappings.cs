using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    [CreateAssetMenu(fileName = "BlendShape Mappings", menuName = "AR Face Capture/BlendShape Mappings")]
    public class BlendShapeMappings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("String names of the blend shapes in the stream with their index in the array being their relative location.")]
        string[] m_LocationIdentifiers;

        [SerializeField]
        [Tooltip("Rename mapping values to apply blend shape locations to a blend shape controller.")]
        string[] m_BlendShapeNames;

        public string[] blendShapeNames { get { return m_BlendShapeNames; }}

        public string[] locationIdentifiers
        {
            get
            {
                return m_LocationIdentifiers;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
