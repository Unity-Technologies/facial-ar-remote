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

        [SerializeField]
        [Range(0,1)]
        float m_WidthPercent = 0.5f;

        [SerializeField]
        [Range(0, 1)]
        float m_HeightPercent = 0.5f;
        
        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        string m_IP = "10.0.1.3";

        [SerializeField]
        Client m_Client;

        [SerializeField]
        Canvas m_MainGUI;

        [SerializeField]
        Canvas m_FaceLostGUI;

        [SerializeField]
        Canvas m_NotSupprotedGUI;

        [SerializeField]
        Canvas m_DisconnectGUI;

        [SerializeField]
        Button m_ConnectButton;

        [SerializeField]
        Button m_DisconnectButton;

        [SerializeField]
        Button m_CancelButton;

        [SerializeField]
        TMP_InputField m_PortTextField;

        [SerializeField]
        TMP_InputField m_IPTextField;

        Socket m_Socket;

        bool m_DeviceSupported;
        bool m_Once;
        bool m_IPValid;

        float m_CenterX;
        float m_CenterY;

        void Awake()
        {
            m_DeviceSupported = UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX;
            m_MainGUI.enabled = false;
            m_FaceLostGUI.enabled = false;
            m_NotSupprotedGUI.enabled = false;
            m_DisconnectGUI.enabled = false;

            m_CancelButton.gameObject.SetActive(false);
        }

        void Start()
        {
            m_PortTextField.onValueChanged.AddListener(OnPortValueChanged);
            m_IPTextField.onValueChanged.AddListener(OnIPValueChanged);
            
            m_ConnectButton.onClick.AddListener(OnConnectClick);
            m_CancelButton.onClick.AddListener(OnCancelClick);
            m_DisconnectButton.onClick.AddListener(OnDisconnectClick);

            m_CenterX = Screen.width / 2f;
            m_CenterY = Screen.height / 2f;
        }

        void Update()
        {
            if (!m_DeviceSupported)
            {
                m_MainGUI.enabled = false;
                m_FaceLostGUI.enabled = false;
                m_DisconnectGUI.enabled = false;
                m_NotSupprotedGUI.enabled = true;
                return;
            }

            m_NotSupprotedGUI.enabled = false;
            m_FaceLostGUI.enabled = !FaceInView();

            if (m_Socket == null)
            {
                m_MainGUI.enabled = true;
                m_DisconnectGUI.enabled = false;

                m_ConnectButton.gameObject.SetActive(m_IPValid);

                m_CancelButton.gameObject.SetActive(false);
            }
            else if (!m_Socket.Connected)
            {
                m_MainGUI.enabled = true;
                m_DisconnectGUI.enabled = false;
                m_ConnectButton.gameObject.SetActive(false);
                m_CancelButton.gameObject.SetActive(true);
            }
            else
            {
                m_MainGUI.enabled = false;
                m_DisconnectGUI.enabled = true;
            }
            
            if (!m_Once && m_Socket != null && m_Socket.Connected)
            {
                m_Client.Setup(m_Socket);
                m_Once = true;
            }
        }
        
        void OnPortValueChanged(string value)
        {
            int tmpPort;
            if (int.TryParse(value, out tmpPort))
            {
                m_Port = tmpPort;
            }
            else
            {
                m_PortTextField.text = m_Port.ToString();
            }
        }
        
        void OnIPValueChanged(string value)
        {
            IPAddress tmpIP;
            if (IPAddress.TryParse(value, out tmpIP))
            {
                m_IPValid = true;
            }

            m_IP = value;
        }

        void OnConnectClick()
        {
            IPAddress tmpIP;
            if (!IPAddress.TryParse(m_IP, out tmpIP))
            {
                return;
            }
            
            new Thread(() =>
            {
                try
                {
                    var endPoint = new IPEndPoint(tmpIP, m_Port);
                    m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    m_Socket.Connect(endPoint);
                    m_Once = false;
                }
                catch (Exception e)
                {
                    Debug.Log("Exception trying to connect: " + e.Message);
                    m_Socket = null;
                }
            }).Start();

            m_ConnectButton.gameObject.SetActive(false);
        }

        void OnCancelClick()
        {
            m_ConnectButton.gameObject.SetActive(false);
            m_CancelButton.gameObject.SetActive(true);
                    
            try
            {
                m_Socket.Disconnect(false);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
                    
            m_Socket = null;
            m_Once = false;
        }

        void OnDisconnectClick()
        {
            try
            {
                m_Socket.Disconnect(false);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            m_Socket = null;
            m_Once = false;
        }

        bool FaceInView()
        {
            var anchorScreenPos = Camera.main.WorldToScreenPoint(m_FaceAnchor.position);
            
            return (!(Mathf.Abs(anchorScreenPos.x - m_CenterX) / m_CenterX > m_WidthPercent)) 
                && (!(Mathf.Abs(anchorScreenPos.y - m_CenterY) / m_CenterY > m_HeightPercent));
        }
    }
}
