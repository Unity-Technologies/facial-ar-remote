using System;
using System.Threading;
using UnityEngine;

namespace PerformanceRecorder
{
    [Serializable]
    public class RemoteStream
    {
        [SerializeField]
        string m_Ip = "192.168.0.1";
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        bool m_IsServer = false;

        Thread m_ReadThread;
        Thread m_WriteThread;
        StreamReader m_Reader = new StreamReader();
        StreamWriter m_Writer = new StreamWriter();
        AdapterSource m_Adapter = new AdapterSource();
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();

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

        public bool isListening
        {
            get { return m_NetworkStreamSource.isListening; }
        }

        public bool isConnecting
        {
            get { return m_NetworkStreamSource.isConnecting; }
        }

        public bool isConnected
        {
            get { return m_NetworkStreamSource.isConnected; }
        }

        public StreamReader reader
        {
            get { return m_Reader; }
        }

        public StreamWriter writer
        {
            get { return m_Writer; }
        }

        public void Connect()
        {
            if (isServer)
                m_NetworkStreamSource.StartServer(port);
            else
                m_NetworkStreamSource.ConnectToServer(ip, port);
            
            SetupThreads();
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.StopConnections();
            Dispose();
        }

        void SetupThreads()
        {
            m_Adapter.streamSource = m_NetworkStreamSource;

            m_ReadThread = new Thread(() =>
            {
                while (true)
                {
                    reader.Read(m_Adapter.stream);
                    Thread.Sleep(1);
                };
            });
            m_ReadThread.Start();

            m_WriteThread = new Thread(() =>
            {
                while (true)
                {
                    var stream = m_NetworkStreamSource.stream;

                    if (stream != null)
                        writer.Send(stream);
                    
                    Thread.Sleep(1);
                };
            });
            m_WriteThread.Start();
        }

        void Dispose()
        {
            AbortThread(ref m_ReadThread);
            AbortThread(ref m_WriteThread);

            m_Reader.Clear();
            m_Writer.Clear();
        }

        void AbortThread(ref Thread thread)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }
    }
}
