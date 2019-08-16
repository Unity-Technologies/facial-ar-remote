
using System.Collections.Generic;
using UnityEngine;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class RemoteDevice : MonoBehaviour
    {
        [SerializeField]
        string m_Ip = "192.168.0.1";
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        bool m_IsServer = false;
        [SerializeField]
        AdapterVersion m_AdapterVersion = AdapterVersion.V1;
        [SerializeField]
        List<BlendShapesController> m_BlendShapesControllers = new List<BlendShapesController>();
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        AdapterSource m_AdapterSource = new AdapterSource();
        PacketStream m_PacketStream = new PacketStream();
        FaceDataRecorder m_FaceDataRecorder = new FaceDataRecorder();

        public string ip
        {
            get { return m_Ip; }
            set { m_Ip = value; }
        }

        public int port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public bool isServer
        {
            get { return m_IsServer; }
            set { m_IsServer = value; }
        }

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
            m_PacketStream.reader.faceDataChanged += FaceDataChanged;
            
            if (isServer)
                m_NetworkStreamSource.StartServer(port);
            else
                m_NetworkStreamSource.ConnectToServer(ip, port);
            
            m_AdapterSource.version = m_AdapterVersion;
            m_AdapterSource.streamSource = m_NetworkStreamSource;
            m_PacketStream.streamSource = m_AdapterSource;
            m_PacketStream.Start();
        }

        void OnDisable()
        {
            m_NetworkStreamSource.StopConnections();
            m_PacketStream.Stop();
            m_PacketStream.reader.faceDataChanged -= FaceDataChanged;
        }

        void LateUpdate()
        {
            m_PacketStream.reader.Receive();
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
            m_PacketStream.reader.Clear();
            m_FaceDataRecorder.StartRecording();
        }

        public void StopRecording()
        {
            m_FaceDataRecorder.StopRecording();
        }

        public IPacketBuffer GetPacketBuffer()
        {
            return m_FaceDataRecorder;
        }
    }
}
