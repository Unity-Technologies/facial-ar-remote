using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client : MonoBehaviour
{
    Socket m_Socket;

    int m_FrameNumber;

    readonly byte[] m_Buffer = new byte[4096];

    public void Setup(Socket socket)
    {
        m_Socket = socket;
        enabled = true;

        new Thread(() =>
        {
            while (true)
            {
                if (m_Socket.Connected)
                {
                    m_Buffer[0] = (byte)m_FrameNumber;
                    m_Socket.Send(m_Buffer);
                }

                Thread.Sleep(10);
            }
        }).Start();
    }

    void OnGUI()
    {
        GUILayout.Label(m_Socket.Connected.ToString());
    }

    void Update()
    {
        m_FrameNumber = Time.frameCount;
    }
}
