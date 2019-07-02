using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(PlaybackStream))]
    public class PlaybackStreamEditor : Editor
    {
        SerializedProperty m_PlaybackDataProp;

        void OnEnable()
        {
            m_PlaybackDataProp = serializedObject.FindProperty("m_PlaybackData");

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
                EditorGUILayout.PropertyField(m_PlaybackDataProp);

                if (m_PlaybackDataProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("No Playback Data has been set. You Will be unable to record, playback, " +
                        "or bake Stream Data.", MessageType.Warning);
                }

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
