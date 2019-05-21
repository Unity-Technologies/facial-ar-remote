using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(PlaybackStream))]
    public class PlaybackStreamEditor : Editor
    {
        SerializedProperty m_PlaybackData;
        SerializedProperty m_BlendShapeMappings;

        void OnEnable()
        {
            m_PlaybackData = serializedObject.FindProperty("m_PlaybackData");
            m_BlendShapeMappings = serializedObject.FindProperty("m_BlendShapeMappings");

            var playbackStream = (PlaybackStream)target;
            var streamReader = playbackStream.gameObject.GetComponent<StreamReader>();
            if (streamReader != null)
            {
                streamReader.ConnectDependencies();
            }
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_PlaybackData);
                EditorGUILayout.PropertyField(m_BlendShapeMappings);

                if (m_PlaybackData.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("No Playback Data has been set. You Will be unable to Record, Playback, " +
                        "or Bake a Stream Data!", MessageType.Warning);
                }

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
