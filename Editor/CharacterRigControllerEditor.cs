using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(CharacterRigController))]
    public class CharacterRigControllerEditor : Editor
    {
        SerializedProperty m_EyeSmoothing;
        SerializedProperty m_HeadSmoothing;
        SerializedProperty m_TrackingLossSmoothing;
        SerializedProperty m_DriveEyes;
        SerializedProperty m_LeftEye;
        SerializedProperty m_RightEye;
        SerializedProperty m_RightEyeNegZ;
        SerializedProperty m_LeftEyeNegZ;
        SerializedProperty m_EyeAngleRange;
        SerializedProperty m_EyeLookDistance;
        SerializedProperty m_DriveHead;
        SerializedProperty m_HeadBone;
        SerializedProperty m_HeadFollowAmount;
        SerializedProperty m_DriveNeck;
        SerializedProperty m_NeckBone;
        SerializedProperty m_NeckFollowAmount;

        bool m_EyeFoldout;
        bool m_HeadFoldout;
        bool m_NeckFoldout;

        void OnEnable()
        {
            m_HeadBone = serializedObject.FindProperty("m_HeadBone");
            m_NeckBone = serializedObject.FindProperty("m_NeckBone");
            m_LeftEye = serializedObject.FindProperty("m_LeftEye");
            m_RightEye = serializedObject.FindProperty("m_RightEye");
            m_TrackingLossSmoothing = serializedObject.FindProperty("m_TrackingLossSmoothing");

            m_DriveEyes = serializedObject.FindProperty("m_DriveEyes");
            m_RightEyeNegZ = serializedObject.FindProperty("m_RightEyeNegZ");
            m_LeftEyeNegZ = serializedObject.FindProperty("m_LeftEyeNegZ");
            m_EyeAngleRange = serializedObject.FindProperty("m_EyeAngleRange");
            m_EyeSmoothing = serializedObject.FindProperty("m_EyeSmoothing");
            m_EyeLookDistance = serializedObject.FindProperty("m_EyeLookDistance");

            m_DriveHead = serializedObject.FindProperty("m_DriveHead");
            m_HeadFollowAmount = serializedObject.FindProperty("m_HeadFollowAmount");
            m_HeadSmoothing = serializedObject.FindProperty("m_HeadSmoothing");

            m_DriveNeck = serializedObject.FindProperty("m_DriveNeck");
            m_NeckFollowAmount = serializedObject.FindProperty("m_NeckFollowAmount");
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_HeadBone);
                EditorGUILayout.PropertyField(m_NeckBone);
                EditorGUILayout.PropertyField(m_LeftEye);
                EditorGUILayout.PropertyField(m_RightEye);
                EditorGUILayout.PropertyField(m_TrackingLossSmoothing);

                m_EyeFoldout = EditorGUILayout.Foldout(m_EyeFoldout, "Eye Settings");
                if (m_EyeFoldout)
                {
                    EditorGUILayout.PropertyField(m_DriveEyes);
                    EditorGUILayout.PropertyField(m_EyeLookDistance);
                    EditorGUILayout.PropertyField(m_RightEyeNegZ);
                    EditorGUILayout.PropertyField(m_LeftEyeNegZ);
                    EditorGUILayout.PropertyField(m_EyeAngleRange);
                    EditorGUILayout.PropertyField(m_EyeSmoothing);
                }

                m_HeadFoldout = EditorGUILayout.Foldout(m_HeadFoldout, "Head Settings");
                if (m_HeadFoldout)
                {
                    EditorGUILayout.PropertyField(m_DriveHead);
                    EditorGUILayout.PropertyField(m_HeadFollowAmount);
                    EditorGUILayout.PropertyField(m_HeadSmoothing);
                }

                m_NeckFoldout = EditorGUILayout.Foldout(m_NeckFoldout, "Neck Settings");
                if (m_NeckFoldout)
                {
                    EditorGUILayout.PropertyField(m_DriveNeck);
                    EditorGUILayout.PropertyField(m_NeckFollowAmount);
                }

                if (check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
