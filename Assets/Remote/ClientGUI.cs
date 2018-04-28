using System.Net;
using System.Net.Sockets;
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

    void OnGUI()
    {
        GUILayout.Label("Port:", m_Style);
        var port = GUILayout.TextField(m_Port.ToString(), m_Style);
        int tmpPort;
        if (int.TryParse(port, out tmpPort))
            m_Port = tmpPort;

        GUILayout.Label("IP address:", m_Style);
        m_IP = GUILayout.TextField(m_IP, m_Style);

        IPAddress address;
        GUI.enabled = IPAddress.TryParse(m_IP, out address);

        if (GUILayout.Button("Connect", m_Style))
        {
            var endPoint = new IPEndPoint(address, m_Port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);
            m_Client.Setup(socket);
            enabled = false;
        }

        GUI.enabled = true;
    }
}
