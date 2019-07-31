using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class XRStream : MonoBehaviour
    {
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        List<BlendShapesController> m_BlendShapesControllers = new List<BlendShapesController>();

        Thread m_Thread;
        StreamReader m_StreamReader = new StreamReader();
        AdapterSource m_Adapter = new AdapterSource();
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();

        public int port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public BlendShapesController[] blendShapesControllers
        {
            get { return m_BlendShapesControllers.ToArray(); }
            set { m_BlendShapesControllers = new List<BlendShapesController>(value); }
        }

        void OnEnable()
        {
            m_StreamReader.faceDataChanged += FaceDataChanged;

            StartServer();
        }

        void OnDisable()
        {
            StopServer();

            m_StreamReader.faceDataChanged -= FaceDataChanged;
        }

        void StartServer()
        {
            m_Adapter.streamSource = m_NetworkStreamSource;
            m_NetworkStreamSource.StartServer(m_Port);

            m_Thread = new Thread(() =>
            {
                while (true)
                {
                    m_StreamReader.Read(m_Adapter.stream);
                    Thread.Sleep(1);
                };
            });
            m_Thread.Start();
        }

        void StopServer()
        {
            m_NetworkStreamSource.StopConnections();
            DisposeThread();
        }

        void DisposeThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        void LateUpdate()
        {
            m_StreamReader.Dequeue();
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
