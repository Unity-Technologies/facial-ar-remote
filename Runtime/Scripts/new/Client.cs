using System;
using System.Runtime.InteropServices;
using UnityEngine;
using StreamBufferDataV1 = Unity.Labs.FacialRemote.StreamBufferDataV1;
using StreamBufferDataV2 = Unity.Labs.FacialRemote.StreamBufferDataV2;

namespace PerformanceRecorder
{
    [Serializable]
    public class Client
    {
        public event Action<VirtualCameraStateData> stateChanged = (s) => {};

        [SerializeField]
        string m_Ip = "192.168.0.1";

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        bool m_IsServer = false;

        [SerializeField]
        VirtualCameraStateData m_State;
        
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();

        PacketStream m_PacketStream = new PacketStream();
        
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

        public AxisLock axisLock
        {
            get { return m_State.axisLock; }
            set { m_State.axisLock = value; }
        }

        public CameraRigType cameraRig
        {
            get { return m_State.cameraRig; }
            set { m_State.cameraRig = value; }
        }

        public float focalLength
        {
            get { return m_State.focalLength; }
            set { m_State.focalLength = value; }
        }

        public bool frozen
        {
            get { return m_State.frozen; }
            set { m_State.frozen = value; }
        }

        public Client()
        {
            m_PacketStream.reader.virtualCameraStateChanged += OnStateChanged;
        }

        public bool IsConnected()
        {
            return m_NetworkStreamSource.isConnected;
        }

        public bool IsTryingToConnect()
        {
            return m_NetworkStreamSource.isConnecting;
        }

        public void Connect()
        {
            m_PacketStream.streamSource = m_NetworkStreamSource;
            m_PacketStream.Start();
            m_NetworkStreamSource.ConnectToServer(ip, port);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.DisconnectClient();
            m_PacketStream.Stop();
        }

        public void SendCommand(CommandType command)
        {
            m_PacketStream.writer.Write(new Command(command));
        }
        
        public void SendCommandInt(CommandIntType command, int i)
        {
            m_PacketStream.writer.Write(new CommandInt(command, i));
        }

        public void Send(PoseData data)
        {
            m_PacketStream.writer.Write(data);
        }

        public void Send(FaceData data)
        {
            m_PacketStream.writer.Write(data);
        }

        public void SendState()
        {
            m_PacketStream.writer.Write(m_State);
        }

        public void Send(StreamBufferDataV1 data)
        {
            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }

        public void Send(StreamBufferDataV2 data)
        {
            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV2>());
        }

        public void Update()
        {
            m_PacketStream.reader.Receive();
        }
                
        void OnStateChanged(VirtualCameraStateData data)
        {
            m_State = data;

            stateChanged(data);
        }
    }
}
