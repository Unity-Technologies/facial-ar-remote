using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class XRStream : MonoBehaviour
    {
        [SerializeField]
        RemoteStream m_RemoteStream = new RemoteStream();
        [SerializeField]
        List<BlendShapesController> m_BlendShapesControllers = new List<BlendShapesController>();
        FaceDataRecorder m_FaceDataRecorder = new FaceDataRecorder();
        int m_TakeCount;
        
        public BlendShapesController[] blendShapesControllers
        {
            get { return m_BlendShapesControllers.ToArray(); }
            set { m_BlendShapesControllers = new List<BlendShapesController>(value); }
        }

        void OnEnable()
        {
            m_RemoteStream.reader.faceDataChanged += FaceDataChanged;

            m_RemoteStream.Connect();
        }

        void OnDisable()
        {
            m_RemoteStream.Disconnect();

            m_RemoteStream.reader.faceDataChanged -= FaceDataChanged;
        }

        void LateUpdate()
        {
            m_RemoteStream.reader.Receive();
        }

        void FaceDataChanged(FaceData faceData)
        {
            if (m_FaceDataRecorder.isRecording)
                m_FaceDataRecorder.Record(faceData);

            foreach (var blendShapesController in m_BlendShapesControllers)
            {
                blendShapesController.blendShapeInput = faceData.blendShapeValues;
            }
        }

        [ContextMenu("Start Recording")]
        public void StartRecoding()
        {
            m_TakeCount++;
            m_FaceDataRecorder.StartRecording();
        }

        [ContextMenu("Stop Recording")]
        public void StopRecording()
        {
            m_FaceDataRecorder.StopRecording();
        }

        [ContextMenu("Save Recording")]
        public void SaveRecording()
        {
            var path = "Assets/" + GenerateFileName() +".arstream";

            using (var fileStream = File.Create(path))
            {
                var buffer = m_FaceDataRecorder.GetBuffer();
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        string GenerateFileName()
        {
            return string.Format("{0:yyyy_MM_dd_HH_mm}-Take{1:00}", DateTime.Now, m_TakeCount);
        }
    }
}
