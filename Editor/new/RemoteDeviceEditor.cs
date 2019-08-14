using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    [CustomEditor(typeof(RemoteDevice))]
    public class RemoteDeviceEditor : Editor
    {
        RemoteDevice m_Device;

        void OnEnable()
        {
            m_Device = target as RemoteDevice;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_Device.isRecording))
                {
                    if (GUILayout.Button("Record"))
                    {
                        m_Device.StartRecording();
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!m_Device.isRecording))
                {
                    if (GUILayout.Button("Stop Recording"))
                    {
                        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + GenerateFileName());
                        var path = uniqueAssetPath + ".arstream";

                        m_Device.StopRecording();

                        using (var fileStream = File.Create(path))
                        using (var writer = new ARStreamWriter(fileStream))
                        {
                            writer.Write(m_Device.GetPacketBuffer());
                        }

                        AssetDatabase.Refresh();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        string GenerateFileName()
        {
            return string.Format("{0:yyyy_MM_dd_HH_mm}-Take", DateTime.Now);
        }
    }
}
