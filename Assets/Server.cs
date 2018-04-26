using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
	static readonly Quaternion k_RotationOffset = Quaternion.AngleAxis(180, Vector3.up);

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
	Pose m_CameraPose;
	Transform m_CameraTransform;

	void Start()
	{
		m_CameraTransform = Camera.main.transform;
		var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];

		Debug.Log(ip);

		var endPoint = new IPEndPoint(ip, m_Port);
		m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		m_Socket.Bind(endPoint);
		m_Socket.Listen(100);
		new Thread(() =>
		{
			m_Socket = m_Socket.Accept();
			Debug.Log("Client connected");

			var poseArray = new float[7];
			var cameraPoseArray = new float[7];

			while (true)
			{
				if (m_Socket.Connected)
				{
					try
					{
						m_Socket.Receive(m_Buffer);
						if (m_Buffer[0] == Client.ErrorCheck)
						{
							const int poseOffset = Client.BlendshapeSize + 1;
							const int cameraPoseOffset = poseOffset + Client.PoseSize;

							Buffer.BlockCopy(m_Buffer, 1, m_Blendshapes, 0, Client.BlendshapeSize);
							Buffer.BlockCopy(m_Buffer, poseOffset, poseArray, 0, Client.PoseSize);
							Buffer.BlockCopy(m_Buffer, cameraPoseOffset, cameraPoseArray, 0, Client.PoseSize);
							ArrayToPose(poseArray, ref m_Pose);
							ArrayToPose(cameraPoseArray, ref m_CameraPose);
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e.Message);
					}
				}

				Thread.Sleep(10);
			}
		}).Start();
	}

	static void ArrayToPose(float[] poseArray, ref Pose pose)
	{
		pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
		pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
	}

	void Update()
	{
		m_CameraTransform.localPosition = m_CameraPose.position;
		m_CameraTransform.localRotation = m_CameraPose.rotation;
		m_Transform.localPosition = m_Pose.position;
		m_Transform.localRotation = m_Pose.rotation * k_RotationOffset;
		for (var i = 0; i < BlendshapeDriver.BlendshapeCount; i++)
		{
			m_SkinnedMeshRenderer.SetBlendShapeWeight(i, m_Blendshapes[i] * 100);
		}
	}
}
