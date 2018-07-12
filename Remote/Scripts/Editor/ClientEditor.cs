using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(Client))]
    public class ClientEditor : Editor
    {
        SerializedProperty m_ClientGUI;
        SerializedProperty m_StreamSettings;

        void OnEnable()
        {
            m_ClientGUI = serializedObject.FindProperty("m_ClientGUI");
            m_StreamSettings = serializedObject.FindProperty("m_StreamSettings");
        }

        public override void OnInspectorGUI()
        {
            var client = target as Client;
            if (client == null)
                return;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (EditorGUILayout.PropertyField(m_ClientGUI))
                    client.InitializeClient();
                EditorGUILayout.PropertyField(m_StreamSettings);

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
