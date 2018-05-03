using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.iOS;

class Server : MonoBehaviour
{
	[Serializable]
	class Mapping
	{
		public string from;
		public string to;
	}

	public const byte ErrorCheck = 42;
	public const int BlendshapeCount = 51;
	public const int BlendshapeSize = sizeof(float) * BlendshapeCount;
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

	[SerializeField]
	int m_Port = 9000;

	[Range(0.1f, 1)]
	[SerializeField]
	float m_CameraSmoothing = 0.8f;

	[Range(0.1f, 1)]
	[SerializeField]
	float m_FaceSmoothing = 0.8f;

	[SerializeField]
	SkinnedMeshRenderer[] m_SkinnedMeshRenderers;

	[SerializeField]
	bool m_TrackCamera;

	[SerializeField]
	bool m_TrackHeadPosition;

	[SerializeField]
	bool m_TrackHeadRotation;

	[SerializeField]
	Transform m_HipsTransform;

	[SerializeField]
	Transform m_HeadTransform;

	[SerializeField]
	Mapping[] m_Mappings;

	Socket m_Socket;
	readonly float[] m_Blendshapes = new float[BlendshapeCount];
	GameObject m_FaceGameObject;
	Pose m_FacePose;
	Pose m_CameraPose;
	Transform m_CameraTransform;
	bool m_Active;
	Vector3 m_HipsOffset;
	bool m_Running;
	int m_LastFrameNum;

	readonly Dictionary<SkinnedMeshRenderer, int[]> m_Indices = new Dictionary<SkinnedMeshRenderer, int[]>();

	readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
	readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

	void Start()
	{
		m_FaceGameObject = m_HeadTransform.gameObject;

		if (m_TrackHeadRotation && m_HeadTransform)
			m_HipsOffset = m_HipsTransform.position - m_HeadTransform.position;

		Application.targetFrameRate = 60;
		for (var i = 0; i < k_BufferPrewarm; i++)
		{
			m_UnusedBuffers.Enqueue(new byte[BufferSize]);
		}

		var locations = new List<string>();
		foreach (var location in ARBlendShapeLocation.Locations)
		{
			locations.Add(Filter(location)); // Eliminate capitalization and _ mismatch
		}

		var mappingLength = m_Mappings.Length;
		for (var i = 0; i < mappingLength; i++)
		{
			var mapping = m_Mappings[i];
			mapping.from = Filter(mapping.from);
			mapping.to = Filter(mapping.to);
		}

		locations.Sort();
		var locationCount = locations.Count;

		foreach (var renderer in m_SkinnedMeshRenderers)
		{
			var mesh = renderer.sharedMesh;
			var count = mesh.blendShapeCount;
			var indices = new int[count];
			for (var i = 0; i < count; i++)
			{
				var name = mesh.GetBlendShapeName(i);
				var lower = Filter(name);
				var index = -1;
				foreach (var mapping in m_Mappings)
				{
					if (lower.Contains(mapping.from))
						index = locations.IndexOf(mapping.to);
				}

				if (index < 0)
				{
					for (var j = 0; j < locationCount; j++)
					{
						if (lower.Contains(locations[j]))
						{
							index = j;
							break;
						}
					}
				}

				indices[i] = index;

				if (index < 0)
					Debug.LogWarningFormat("Blendshape {0} is not a valid AR blendshape", name);
			}

			m_Indices.Add(renderer, indices);
		}

		if (m_TrackCamera)
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

	static string Filter(string @string)
	{
		return @string.ToLower().Replace("_", "");
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

		var facePosition = m_FacePose.position;
		if (m_TrackCamera)
		{
			m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, m_CameraPose.position, m_CameraSmoothing);
			m_CameraTransform.localRotation = Quaternion.Lerp(m_CameraTransform.localRotation, m_CameraPose.rotation, m_CameraSmoothing);

			var toCamera = facePosition - m_CameraPose.position;
			toCamera.y = 0;
			if (toCamera.magnitude > 0)
				m_HipsTransform.rotation = Quaternion.Lerp(m_HipsTransform.rotation, Quaternion.LookRotation(toCamera), m_FaceSmoothing);
		}

		if (m_TrackHeadPosition)
			m_HipsTransform.position = Vector3.Lerp(m_HipsTransform.position, facePosition + m_HipsOffset, m_FaceSmoothing);

		if (m_TrackHeadRotation)
			m_HeadTransform.rotation = Quaternion.Lerp(m_HeadTransform.rotation, m_FacePose.rotation, m_FaceSmoothing);

		foreach (var renderer in m_SkinnedMeshRenderers)
		{
			var indices = m_Indices[renderer];
			var length = indices.Length;
			for (var i = 0; i < length; i++)
			{
				var index = indices[i];
				if (index < 0)
					continue;

				renderer.SetBlendShapeWeight(i, m_Blendshapes[index] * 100);
			}
		}
	}

	void OnDestroy()
	{
		m_Running = false;
	}
}
