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
            foreach (var blendShapesController in m_BlendShapesControllers)
            {
                blendShapesController.blendShapeInput = faceData.blendShapeValues;
            }
        }
    }
}
