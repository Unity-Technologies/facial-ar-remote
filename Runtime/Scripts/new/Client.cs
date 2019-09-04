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
        [SerializeField]
        string m_Ip = "192.168.0.1";
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        bool m_IsServer = false;

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

        public PacketStream packetStream => m_PacketStream;

        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        PacketStream m_PacketStream = new PacketStream();
        
        public bool IsConnected()
        {
            return m_NetworkStreamSource.isConnected || m_NetworkStreamSource.isConnecting;
        }

        public void Connect()
        {
            packetStream.streamSource = m_NetworkStreamSource;
            packetStream.Start();
            m_NetworkStreamSource.ConnectToServer(ip, port);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.DisconnectClient();
            packetStream.Stop();
        }

        public void SendCommand(CommandType command)
        {
            packetStream.writer.Write(new Command(command));
        }
        
        public void SendCommandInt(CommandIntType command, int i)
        {
            packetStream.writer.Write(new CommandInt(command, i));
        }

        public void Send(PoseData data)
        {
            packetStream.writer.Write(data);
        }

        public void Send(FaceData data)
        {
            packetStream.writer.Write(data);
        }

        public void Send(VirtualCameraStateData data)
        {
            packetStream.writer.Write(data);
        }

        public void Send(StreamBufferDataV1 data)
        {
            packetStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }

        public void Send(StreamBufferDataV2 data)
        {
            packetStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV2>());
        }
    }
}
