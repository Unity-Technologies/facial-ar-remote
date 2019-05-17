using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(BlendShapeMappings))]
    public class BlendShapeMappingsEditor : Editor
    {
        SerializedProperty m_LocationIdentifiers;
        SerializedProperty m_BlendShapeNames;

        void OnEnable()
        {
            m_LocationIdentifiers = serializedObject.FindProperty("m_LocationIdentifiers");
            m_BlendShapeNames = serializedObject.FindProperty("m_BlendShapeNames");
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Location Identifier");
                EditorGUILayout.LabelField("BlendShape Name");
                EditorGUILayout.EndHorizontal();
                for (var i = 0; i < m_LocationIdentifiers.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_LocationIdentifiers.GetArrayElementAtIndex(i), new GUIContent());
                    EditorGUILayout.PropertyField(m_BlendShapeNames.GetArrayElementAtIndex(i), new GUIContent());
                    EditorGUILayout.EndHorizontal();
                }

                if (check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
