using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(NetworkStream))]
    public class NetworkStreamEditor : Editor
    {
        SerializedProperty m_StreamSettings;
        SerializedProperty m_Port;
        SerializedProperty m_CatchUpThreshold;
        SerializedProperty m_CatchUpSize;
        SerializedProperty m_StreamRecorderOverride;

        void OnEnable()
        {
            m_StreamSettings = serializedObject.FindProperty("m_StreamSettings");
            m_Port = serializedObject.FindProperty("m_Port");
            m_CatchUpThreshold = serializedObject.FindProperty("m_CatchUpThreshold");
            m_CatchUpSize = serializedObject.FindProperty("m_CatchUpSize");
            m_StreamRecorderOverride = serializedObject.FindProperty("m_StreamRecorderOverride");

            var networkStream = (NetworkStream)target;
            var streamReader = networkStream.gameObject.GetComponent<StreamReader>();
            if (streamReader != null)
            {
                streamReader.ConnectDependencies();
            }
        }

        public override void OnInspectorGUI()
        {

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_StreamSettings);
                EditorGUILayout.PropertyField(m_Port);
                EditorGUILayout.PropertyField(m_CatchUpThreshold);
                EditorGUILayout.PropertyField(m_CatchUpSize);
                EditorGUILayout.PropertyField(m_StreamRecorderOverride);

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
