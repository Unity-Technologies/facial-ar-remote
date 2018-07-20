using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Unity.Labs.FacialRemote
{
    class ClientGUI : MonoBehaviour
    {
        [SerializeField]
        Transform m_FaceAnchor;

        [Tooltip("Percentage of screen width outside which the Face Lost GUI will appear")]
        [Range(0,1)]
        [SerializeField]
        float m_WidthPercent = 0.5f;

        [Tooltip("Percentage of screen height outside which the Face Lost GUI will appear")]
        [Range(0, 1)]
        [SerializeField]
        float m_HeightPercent = 0.5f;

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        string m_IP = "192.168.1.2";

        [SerializeField]
        Client m_Client;

        [SerializeField]
        Canvas m_MainGUI;

        [SerializeField]
        Canvas m_FaceLostGUI;

        [SerializeField]
        Canvas m_NotSupprotedGUI;

        [SerializeField]
        Button m_ConnectButton;

        [SerializeField]
        TMP_InputField m_PortTextField;

        [SerializeField]
        TMP_InputField m_IPTextField;

        Socket m_Socket;
        Camera m_Camera;

        float m_CenterX;
        float m_CenterY;

        void Awake()
        {
            m_Camera = Camera.main;
            if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
            {
                m_MainGUI.enabled = false;
                m_FaceLostGUI.enabled = false;
                m_NotSupprotedGUI.enabled = false;
            }
            else
            {
                m_MainGUI.enabled = false;
                m_FaceLostGUI.enabled = false;
                m_NotSupprotedGUI.enabled = true;
                enabled = false;
            }
        }

        void Start()
        {
            m_PortTextField.onValueChanged.AddListener(OnPortValueChanged);
            m_IPTextField.onValueChanged.AddListener(OnIPValueChanged);

            m_ConnectButton.onClick.AddListener(OnConnectClick);

            m_CenterX = Screen.width / 2f;
            m_CenterY = Screen.height / 2f;

            OnIPValueChanged(m_IP);
        }

        void Update()
        {
            m_FaceLostGUI.enabled = !FaceInView();
            m_MainGUI.enabled = m_Socket == null || !m_Socket.Connected;

            if (m_Socket != null && m_Socket.Connected)
                m_Client.SetupSocket(m_Socket);
        }

        void OnPortValueChanged(string value)
        {
            int port;
            if (int.TryParse(value, out port))
                m_Port = port;
            else
                m_PortTextField.text = m_Port.ToString();
        }

        void OnIPValueChanged(string value)
        {
            IPAddress ip;
            m_ConnectButton.gameObject.SetActive(IPAddress.TryParse(value, out ip));
            m_IP = value;
        }

        void OnConnectClick()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(m_IP, out ip))
                return;

            new Thread(() =>
            {
                try
                {
                    var endPoint = new IPEndPoint(ip, m_Port);
                    m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    m_Socket.Connect(endPoint);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception trying to connect: " + e.Message);
                    m_Socket = null;
                }
            }).Start();

            m_ConnectButton.gameObject.SetActive(false);
        }

        bool FaceInView()
        {
            var anchorScreenPos = m_Camera.WorldToScreenPoint(m_FaceAnchor.position);

            return !(Mathf.Abs(anchorScreenPos.x - m_CenterX) / m_CenterX > m_WidthPercent)
                && !(Mathf.Abs(anchorScreenPos.y - m_CenterY) / m_CenterY > m_HeightPercent);
        }
    }
}
