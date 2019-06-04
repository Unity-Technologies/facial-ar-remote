using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(StreamReader))]
    public class StreamReaderEditor : Editor
    {
        SerializedProperty m_Character;
        SerializedProperty m_TrackingLossPadding;
        SerializedProperty m_VerboseLogging;
        SerializedProperty m_BlendShapesControllerOverride;
        SerializedProperty m_CharacterRigControllerOverride;
        SerializedProperty m_HeadBoneOverride;
        SerializedProperty m_CameraOverride;
        SerializedProperty m_StreamSourceOverrides;

        void OnEnable()
        {
            m_Character = serializedObject.FindProperty("m_Character");
            m_VerboseLogging = serializedObject.FindProperty("m_VerboseLogging");
            m_TrackingLossPadding = serializedObject.FindProperty("m_TrackingLossPadding");
            m_BlendShapesControllerOverride = serializedObject.FindProperty("m_BlendShapesControllerOverride");
            m_CharacterRigControllerOverride = serializedObject.FindProperty("m_CharacterRigControllerOverride");
            m_HeadBoneOverride = serializedObject.FindProperty("m_HeadBoneOverride");
            m_CameraOverride = serializedObject.FindProperty("m_CameraOverride");
            m_StreamSourceOverrides = serializedObject.FindProperty("m_StreamSourceOverrides");

            var streamReader = (StreamReader)target;
            streamReader.ConnectDependencies();
        }

        public override void OnInspectorGUI()
        {
            var streamReader = (StreamReader)target;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_Character);
                EditorGUILayout.PropertyField(m_TrackingLossPadding);
                EditorGUILayout.PropertyField(m_VerboseLogging);
                EditorGUILayout.Space();
                
                if (streamReader.blendShapesController == null)
                    EditorGUILayout.HelpBox("No Blend Shape Controller has been set or found. Note this data can " +
                        "still be recorded in the stream.", MessageType.Warning);

                if (streamReader.characterRigController == null)
                {
                    EditorGUILayout.HelpBox("No Character Rig Controller has been set or found. Note this data can " +
                        "still be recorded in the stream.", MessageType.Warning);
                }

                if (streamReader.headBone == null)
                {
                    EditorGUILayout.HelpBox("No Head Bone Transform has been set or found. Note this data can still " +
                        "be recorded in the stream.", MessageType.Warning);
                }

                if (streamReader.cameraTransform == null)
                {
                    EditorGUILayout.HelpBox("No Camera has been set or found. Note this data can still be recorded " +
                        "in the stream.", MessageType.Warning);
                }

                EditorGUILayout.LabelField("Overrides", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_BlendShapesControllerOverride);
                EditorGUILayout.PropertyField(m_CharacterRigControllerOverride);
                EditorGUILayout.PropertyField(m_HeadBoneOverride);
                EditorGUILayout.PropertyField(m_CameraOverride);
                EditorGUILayout.PropertyField(m_StreamSourceOverrides, true);

                EditorGUILayout.Space();
                
                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Open AR Face Capture Window", GUILayout.Width(200)))
                {
                    ARFaceCaptureWindow.ShowWindow();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal ();

                if (check.changed)
                {
                    streamReader.ConnectDependencies();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            // Want editor to update every frame
            EditorUtility.SetDirty(target);
        }
    }
}
