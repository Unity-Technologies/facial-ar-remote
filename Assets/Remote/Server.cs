using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

class Server : MonoBehaviour
{
	public const byte ErrorCheck = 42;
	public const int BlendshapeSize = sizeof(float) * BlendshapeDriver.BlendshapeCount;
	public const int PoseSize = sizeof(float) * 7;
	public const int PoseOffset = BlendshapeSize + 1;
	public const int CameraPoseOffset = PoseOffset + PoseSize;
	public const int FrameNumberOffset = CameraPoseOffset + PoseSize;

	// 0 - Error check
	// 1-204 - Blendshapes
	// 205-232 - Pose
	// 233-260 - Camera Pose
	// 261-264 - Frame Number
	// 265 - Active state
	public const int BufferSize = 266;

	const int k_BufferPrewarm = 16;
	const int k_MaxBufferQueue = 1024; // No use in bufferring really old frames
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
	Transform m_HipsTransform;

	[SerializeField]
	Transform m_FaceTransform;

	Socket m_Socket;
	readonly float[] m_Blendshapes = new float[BlendshapeDriver.BlendshapeCount];
	GameObject m_FaceGameObject;
	Pose m_FacePose;
	Pose m_CameraPose;
	Transform m_CameraTransform;
	bool m_Active;
	Vector3 m_HipsOffset;
	bool m_Running;
	int m_LastFrameNum;

	readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
	readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

	void Start()
	{
		m_HipsOffset = m_HipsTransform.position - m_FaceTransform.position;
		Application.targetFrameRate = 60;
		for (var i = 0; i < k_BufferPrewarm; i++)
		{
			m_UnusedBuffers.Enqueue(new byte[BufferSize]);
		}

		m_FaceGameObject = m_FaceTransform.gameObject;
		m_CameraTransform = Camera.main.transform;
		Debug.Log("Possible IP addresses:");
		foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
		{
			Debug.Log(address);

			var endPoint = new IPEndPoint(address, m_Port);
			m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			m_Socket.Bind(endPoint);
			m_Socket.Listen(100);
			m_Running = true;
			m_LastFrameNum = -1;
			new Thread(() =>
			{
				m_Socket = m_Socket.Accept();
				Debug.Log("Client connected on " + address);

				var frameNumArray = new int[1];

				while (m_Running)
				{
					if (m_Socket.Connected)
					{
						try
						{
							var buffer = m_UnusedBuffers.Count == 0 ? new byte[BufferSize] : m_UnusedBuffers.Dequeue();
							m_Socket.Receive(buffer);
							if (buffer[0] == ErrorCheck)
							{
								m_BufferQueue.Enqueue(buffer);
								Buffer.BlockCopy(buffer, FrameNumberOffset, frameNumArray, 0, sizeof(int));

								var frameNum = frameNumArray[0];
								if (m_LastFrameNum != frameNum - 1)
									Debug.LogFormat("Dropped frame {0} (last frame: {1}) ", frameNum, m_LastFrameNum);

								m_LastFrameNum = frameNum;
							}
						}
						catch (Exception e)
						{
							Debug.LogError(e.Message);
						}
					}

					if (m_BufferQueue.Count > k_MaxBufferQueue)
						m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue());

					Thread.Sleep(1);
				}
			}).Start();
		}
	}

	static void ArrayToPose(float[] poseArray, ref Pose pose)
	{
		pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
		pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
	}

	bool DequeueBuffer()
	{
		if (m_BufferQueue.Count == 0)
			return false;

		if (m_BufferQueue.Count > 2)
			m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue()); // Throw out an old frame

		var poseArray = new float[7];
		var cameraPoseArray = new float[7];
		var buffer = m_BufferQueue.Dequeue();
		Buffer.BlockCopy(buffer, 1, m_Blendshapes, 0, BlendshapeSize);
		Buffer.BlockCopy(buffer, PoseOffset, poseArray, 0, PoseSize);
		Buffer.BlockCopy(buffer, CameraPoseOffset, cameraPoseArray, 0, PoseSize);
		ArrayToPose(poseArray, ref m_FacePose);
		ArrayToPose(cameraPoseArray, ref m_CameraPose);
		m_Active = buffer[buffer.Length - 1] == 1;
		m_UnusedBuffers.Enqueue(buffer);

		return true;
	}

	void Update()
	{
		if (!DequeueBuffer())
			return;

		m_FaceGameObject.SetActive(m_Active);
		m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, m_CameraPose.position, m_CameraSmoothing);
		m_CameraTransform.localRotation = Quaternion.Lerp(m_CameraTransform.localRotation, m_CameraPose.rotation, m_CameraSmoothing);
		var facePosition = m_FacePose.position;
		m_HipsTransform.position = Vector3.Lerp(m_HipsTransform.position, facePosition + m_HipsOffset, m_FaceSmoothing);
		m_FaceTransform.rotation = Quaternion.Lerp(m_FaceTransform.rotation, m_FacePose.rotation * k_RotationOffset, m_FaceSmoothing);

		var toCamera = facePosition - m_CameraPose.position;
		toCamera.y = 0;
		if (toCamera.magnitude > 0)
			m_HipsTransform.rotation = Quaternion.Lerp(m_HipsTransform.rotation, Quaternion.LookRotation(toCamera) * k_RotationOffset, m_FaceSmoothing);

		for (var i = 0; i < BlendshapeDriver.BlendshapeCount; i++)
		{
			m_SkinnedMeshRenderer.SetBlendShapeWeight(i, m_Blendshapes[i] * 100);
		}
	}

	void OnDestroy()
	{
		m_Running = false;
	}
}
