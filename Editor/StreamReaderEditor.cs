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

        void OnEnable()
        {
            m_Character = serializedObject.FindProperty("m_Character");
            m_VerboseLogging = serializedObject.FindProperty("m_VerboseLogging");
            m_TrackingLossPadding = serializedObject.FindProperty("m_TrackingLossPadding");

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
