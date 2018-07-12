using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(BlendShapesController))]
    public class BlendShapesControllerEditor : Editor
    {
        SerializedProperty m_SkinnedMeshRenderers;
        SerializedProperty m_BlendShapeSmoothing;
        SerializedProperty m_BlendShapeThreshold;
        SerializedProperty m_BlendShapeCoefficient;
        SerializedProperty m_BlendShapeMax;
        SerializedProperty m_TrackingLossSmoothing;
        SerializedProperty m_Overrides;

        void OnEnable()
        {
            m_SkinnedMeshRenderers = serializedObject.FindProperty("m_SkinnedMeshRenderers");
            m_BlendShapeSmoothing = serializedObject.FindProperty("m_BlendShapeSmoothing");
            m_BlendShapeThreshold = serializedObject.FindProperty("m_BlendShapeThreshold");
            m_BlendShapeCoefficient = serializedObject.FindProperty("m_BlendShapeCoefficient");
            m_BlendShapeMax = serializedObject.FindProperty("m_BlendShapeMax");
            m_TrackingLossSmoothing = serializedObject.FindProperty("m_TrackingLossSmoothing");
            m_Overrides = serializedObject.FindProperty("m_Overrides");
        }

        public override void OnInspectorGUI()
        {
            using (var check =  new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_SkinnedMeshRenderers);
                EditorGUILayout.PropertyField(m_BlendShapeSmoothing);
                EditorGUILayout.PropertyField(m_BlendShapeThreshold);
                EditorGUILayout.PropertyField(m_BlendShapeCoefficient);
                EditorGUILayout.PropertyField(m_BlendShapeMax);
                EditorGUILayout.PropertyField(m_TrackingLossSmoothing);
                EditorGUILayout.PropertyField(m_Overrides);

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
