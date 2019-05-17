using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamSettings))]
    public class StreamSettingsEditor : Editor
    {
        SerializedProperty m_ErrorCheck;
        SerializedProperty m_BlendShapeCount;
        SerializedProperty m_BlendShapeSize;
        SerializedProperty m_FrameNumberSize;
        SerializedProperty m_FrameTimeSize;
        SerializedProperty m_HeadPoseOffset;
        SerializedProperty m_CameraPoseOffset;
        SerializedProperty m_FrameNumberOffset;
        SerializedProperty m_FrameTimeOffset;
        SerializedProperty m_BufferSize;
        SerializedProperty m_Locations;
        SerializedProperty m_Mappings;

        void OnEnable()
        {
            m_ErrorCheck = serializedObject.FindProperty("m_ErrorCheck");
            m_BlendShapeCount = serializedObject.FindProperty("m_BlendShapeCount");
            m_BlendShapeSize = serializedObject.FindProperty("m_BlendShapeSize");
            m_FrameNumberSize = serializedObject.FindProperty("m_FrameNumberSize");
            m_FrameTimeSize = serializedObject.FindProperty("m_FrameTimeSize");
            m_HeadPoseOffset = serializedObject.FindProperty("m_HeadPoseOffset");
            m_CameraPoseOffset = serializedObject.FindProperty("m_CameraPoseOffset");
            m_FrameNumberOffset = serializedObject.FindProperty("m_FrameNumberOffset");
            m_FrameTimeOffset = serializedObject.FindProperty("m_FrameTimeOffset");
            m_BufferSize = serializedObject.FindProperty("m_BufferSize");
            m_Locations = serializedObject.FindProperty("m_Locations");
            m_Mappings = serializedObject.FindProperty("m_Mappings");
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_ErrorCheck);
                EditorGUILayout.PropertyField(m_BlendShapeCount);
                EditorGUILayout.PropertyField(m_BlendShapeSize);
                EditorGUILayout.PropertyField(m_FrameNumberSize);
                EditorGUILayout.PropertyField(m_FrameTimeSize);
                EditorGUILayout.PropertyField(m_HeadPoseOffset);
                EditorGUILayout.PropertyField(m_CameraPoseOffset);
                EditorGUILayout.PropertyField(m_FrameNumberOffset);
                EditorGUILayout.PropertyField(m_FrameTimeOffset);
                EditorGUILayout.PropertyField(m_BufferSize);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Location Identifier");
                EditorGUILayout.LabelField("BlendShape Name");
                EditorGUILayout.EndHorizontal();
                for (var i = 0; i < m_Locations.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_Locations.GetArrayElementAtIndex(i), new GUIContent());
                    EditorGUILayout.PropertyField(m_Mappings.GetArrayElementAtIndex(i), new GUIContent());
                    EditorGUILayout.EndHorizontal();
                }

                if (check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
