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

	[Range(0.1f, 1)]
	[SerializeField]
	float m_CameraSmoothing = 0.8f;

	[Range(0.1f, 1)]
	[SerializeField]
	float m_FaceSmoothing = 0.8f;

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
		Debug.Log("Possible IP addresses:");
		foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
		{
			Debug.Log(address);

			var endPoint = new IPEndPoint(address, m_Port);
			m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			m_Socket.Bind(endPoint);
			m_Socket.Listen(100);
			new Thread(() =>
			{
				m_Socket = m_Socket.Accept();
				Debug.Log("Client connected on " + address);

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
	}

	static void ArrayToPose(float[] poseArray, ref Pose pose)
	{
		pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
		pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
	}

	void Update()
	{
		m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, m_CameraPose.position, m_CameraSmoothing);
		m_CameraTransform.localRotation = Quaternion.Lerp(m_CameraTransform.localRotation, m_CameraPose.rotation, m_CameraSmoothing);
		m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, m_Pose.position, m_FaceSmoothing);
		m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, m_Pose.rotation * k_RotationOffset, m_FaceSmoothing);
		for (var i = 0; i < BlendshapeDriver.BlendshapeCount; i++)
		{
			m_SkinnedMeshRenderer.SetBlendShapeWeight(i, m_Blendshapes[i] * 100);
		}
	}
}
