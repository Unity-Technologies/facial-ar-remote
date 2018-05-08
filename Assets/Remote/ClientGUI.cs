using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

class ClientGUI : MonoBehaviour
{
    [SerializeField]
    int m_Port = 9000;

    [SerializeField]
    string m_IP = "192.168.1.1";

    [SerializeField]
    Client m_Client;

    [SerializeField]
    GUIStyle m_Style = GUIStyle.none;

    Socket m_Socket;

    void OnGUI()
    {
        GUILayout.Label("Port:", m_Style);
        var port = GUILayout.TextField(m_Port.ToString(), m_Style);
        int tmpPort;
        if (int.TryParse(port, out tmpPort))
            m_Port = tmpPort;

        GUILayout.Label("IP address:", m_Style);
        m_IP = GUILayout.TextField(m_IP, m_Style);

        if (m_Socket == null)
        {
            IPAddress address;
            GUI.enabled = IPAddress.TryParse(m_IP, out address);
            if (GUILayout.Button("Connect", m_Style))
            {
                new Thread(() =>
                {
                    try
                    {
                        var endPoint = new IPEndPoint(address, m_Port);
                        m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        m_Socket.Connect(endPoint);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Exception trying to connect: " + e.Message);
                        m_Socket = null;
                    }
                }).Start();
            }

            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Cancel", m_Style))
            {
                m_Socket.Disconnect(false);
                m_Socket = null;
            }
        }
    }

    void Update()
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            m_Client.Setup(m_Socket);
            m_Socket = null;
            enabled = false;
        }
    }
}
