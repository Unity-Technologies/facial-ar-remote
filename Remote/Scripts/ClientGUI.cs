using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using UnityEngine.XR.iOS;
#endif

namespace Unity.Labs.FacialRemote
{
    class ClientGUI : MonoBehaviour
    {
        [SerializeField]
        Transform m_FaceAnchor;

        [Tooltip("Percentage of screen width outside which the Face Lost GUI will appear")]
        [Range(0, 1)]
        [SerializeField]
        float m_WidthPercent = 0.5f;

        [Tooltip("Percentage of screen height outside which the Face Lost GUI will appear")]
        [Range(0, 1)]
        [SerializeField]
        float m_HeightPercent = 0.5f;

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        string m_ServerIP = "192.168.1.2";

        [SerializeField]
        Client m_Client;

        [SerializeField]
        Canvas m_MainGUI;

        [SerializeField]
        Canvas m_FaceLostGUI;

        [SerializeField]
        Canvas m_NotSupportedGUI;

        [SerializeField]
        Button m_ConnectButton;

        [SerializeField]
        TMP_InputField m_PortTextField;

        [SerializeField]
        TMP_InputField m_IPTextField;

        Camera m_Camera;

        float m_CenterX;
        float m_CenterY;

        Socket m_Socket;

        void Awake()
        {
            m_Camera = Camera.main;
#if UNITY_IOS
            var config = new ARKitFaceTrackingConfiguration();
            if (config.IsSupported)
            {
                m_MainGUI.enabled = false;
                m_FaceLostGUI.enabled = false;
                m_NotSupportedGUI.enabled = false;
            }
            else
            {
#endif
                m_MainGUI.enabled = false;
                m_FaceLostGUI.enabled = false;
                m_NotSupportedGUI.enabled = true;
                enabled = false;
#if UNITY_IOS
            }
#endif
        }

        void Start()
        {
            m_PortTextField.onValueChanged.AddListener(OnPortValueChanged);
            m_IPTextField.onValueChanged.AddListener(OnIPValueChanged);

            m_ConnectButton.onClick.AddListener(OnConnectClick);

            m_CenterX = Screen.width / 2f;
            m_CenterY = Screen.height / 2f;

            // Make sure text fields match serialized values
            m_PortTextField.text = m_Port.ToString();
            m_IPTextField.text = m_ServerIP;
        }

        void Update()
        {
            m_FaceLostGUI.enabled = !FaceInView();

            var connected = m_Socket != null && m_Socket.Connected;
            if (m_MainGUI.enabled && connected)
                m_Client.StartCapture(m_Socket);

            m_MainGUI.enabled = !connected;
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
            m_ServerIP = value;
        }

        void OnConnectClick()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(m_ServerIP, out ip))
                return;

            new Thread(() =>
            {
                try
                {
                    var endPoint = new IPEndPoint(ip, m_Port);
                    var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(endPoint);

                    while (!socket.Connected)
                    {
                        Thread.Sleep(5);
                    }

                    m_Socket = socket;
                }
                catch (Exception e)
                {
                    Debug.Log("Exception trying to connect: " + e.Message);
                }
            }).Start();
        }

        bool FaceInView()
        {
            var anchorScreenPos = m_Camera.WorldToScreenPoint(m_FaceAnchor.position);

            return !(Mathf.Abs(anchorScreenPos.x - m_CenterX) / m_CenterX > m_WidthPercent)
                && !(Mathf.Abs(anchorScreenPos.y - m_CenterY) / m_CenterY > m_HeightPercent);
        }
    }
}
