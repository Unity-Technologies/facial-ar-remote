
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class RemoteDevice : MonoBehaviour
    {
        [SerializeField]
        RemoteStream m_Stream = new RemoteStream();
        [SerializeField]
        List<BlendShapesController> m_BlendShapesControllers = new List<BlendShapesController>();
        FaceDataRecorder m_FaceDataRecorder = new FaceDataRecorder();

        public bool isRecording
        {
            get { return m_FaceDataRecorder.isRecording; }
        }
        
        public BlendShapesController[] blendShapesControllers
        {
            get { return m_BlendShapesControllers.ToArray(); }
            set { m_BlendShapesControllers = new List<BlendShapesController>(value); }
        }

        void OnEnable()
        {
            m_Stream.reader.faceDataChanged += FaceDataChanged;

            m_Stream.Connect();
        }

        void OnDisable()
        {
            m_Stream.Disconnect();

            m_Stream.reader.faceDataChanged -= FaceDataChanged;
        }

        void LateUpdate()
        {
            m_Stream.reader.Receive();
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

        public void StartRecording()
        {
            m_Stream.reader.Clear();
            m_FaceDataRecorder.StartRecording();
        }

        public void StopRecording()
        {
            m_FaceDataRecorder.StopRecording();
        }

        public void SaveRecording(string path)
        {
            using (var fileStream = File.Create(path))
            {
                var buffer = m_FaceDataRecorder.GetBuffer();
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
