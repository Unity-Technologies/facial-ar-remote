using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
	[SerializeField]
	int m_Port = 9000;

	int m_FrameNumber;

	Socket m_Socket;

	readonly byte[] m_Buffer = new byte[4096];

	void Start()
	{
		var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
		Debug.Log(ip);
		var endPoint = new IPEndPoint(ip, m_Port);
		m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		m_Socket.Bind(endPoint);
		m_Socket.Listen(100);
		new Thread(() =>
		{
			m_Socket = m_Socket.Accept();
			Debug.Log("accepted");

			while (true)
			{
				if (m_Socket.Connected)
				{
					m_Socket.Receive(m_Buffer);
					m_FrameNumber = m_Buffer[0];
					Thread.Sleep(10);
				}
			}
		}).Start();
	}

	void Update()
	{
		if (!m_Socket.Connected)
			return;

		Debug.Log(m_FrameNumber);
	}
}
