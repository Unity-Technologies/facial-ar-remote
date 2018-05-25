using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(Server))]
    public class ServerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var server = target as Server;
            using (new EditorGUI.DisabledGroupScope(!server.useRecorder))
            {
                if (server.isRecording)
                {
                    if (GUILayout.Button("Stop Recording"))
                        server.StopRecording();
                }
                else
                {
                    if (GUILayout.Button("Start Recording"))
                        server.StartRecording();
                }
            }
        }
    }
}
