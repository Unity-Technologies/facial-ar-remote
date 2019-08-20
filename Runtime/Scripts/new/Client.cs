using System;
using System.Runtime.InteropServices;
using Microsoft.IO;
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

        public bool isConnected
        {
            get { return m_NetworkStreamSource.isConnected || m_NetworkStreamSource.isConnecting; }
        }

        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        PacketStream m_PacketStream = new PacketStream();

        public void Connect()
        {
            m_NetworkStreamSource.ConnectToServer(ip, port);
            m_PacketStream.streamSource = m_NetworkStreamSource;
            m_PacketStream.Start();
        }

        public void Disconnect()
        {
            m_PacketStream.Stop();
            m_NetworkStreamSource.StopConnections();
        }

        public void SendStartRecording()
        {
            m_PacketStream.writer.Write(new Command(CommandType.StartRecording));
        }

        public void SendStopRecording()
        {
            m_PacketStream.writer.Write(new Command(CommandType.StopRecording));
        }

        public void Send(PoseData data)
        {
            m_PacketStream.writer.Write(data);
        }

        public void Send(FaceData data)
        {
            m_PacketStream.writer.Write(data);
        }

        public void Send(StreamBufferDataV1 data)
        {
            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV1>());
        }

        public void Send(StreamBufferDataV2 data)
        {
            m_PacketStream.writer.Write(data.ToBytes(), Marshal.SizeOf<StreamBufferDataV2>());
        }
    }
}
