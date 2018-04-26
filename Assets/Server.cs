using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
	[SerializeField]
	int m_Port = 9000;

	[SerializeField]
	SkinnedMeshRenderer m_SkinnedMeshRenderer;

	[SerializeField]
	Transform m_Transform;

	Socket m_Socket;
	readonly byte[] m_Buffer = new byte[Client.BufferSize];
	readonly float[] m_Blendshapes = new float[BlendshapeDriver.BlendshapeCount];
	Pose m_Pose;

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

			//var posePtr = Marshal.AllocHGlobal(Client.PoseSize);
			var poseArray = new float[7];

			while (true)
			{
				if (m_Socket.Connected)
				{
					try
					{
						m_Socket.Receive(m_Buffer);
						if (m_Buffer[0] == Client.ErrorCheck)
						{
							Buffer.BlockCopy(m_Buffer, 1, m_Blendshapes, 0, Client.BlendshapeSize);
							// Marshal.Copy(m_Buffer, Client.BlendshapeSize + 1, posePtr, Client.PoseSize);
							// Marshal.PtrToStructure(posePtr, m_Pose);

							Buffer.BlockCopy(m_Buffer, Client.BlendshapeSize + 1, poseArray, 0, Client.PoseSize);
							m_Pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
							m_Pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
						}
					}
					catch (Exception e)
					{
					}
				}

				Thread.Sleep(10);
			}
		}).Start();
	}

	void Update()
	{
		m_Transform.localPosition = m_Pose.position;
		m_Transform.localRotation = m_Pose.rotation;
		for (var i = 0; i < BlendshapeDriver.BlendshapeCount; i++)
		{
			m_SkinnedMeshRenderer.SetBlendShapeWeight(i, m_Blendshapes[i] * 100);
		}
	}
}
