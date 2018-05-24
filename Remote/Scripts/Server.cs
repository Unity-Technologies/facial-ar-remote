using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class Mapping
    {
        public string from;
        public string to;
    }

    public class Server : MonoBehaviour
    {
        public const byte ErrorCheck = 42;
        public const int BlendShapeCount = 51;
        public const int BlendShapeSize = sizeof(float) * BlendShapeCount;
        public const int PoseSize = sizeof(float) * 7;
        public const int PoseOffset = BlendShapeSize + 1;
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
        const int k_MaxBufferQueue = 512; // No use in bufferring really old frames

        [SerializeField]
        int m_Port = 9000;

        [SerializeField]
        int m_CatchupSize = 2;

        [SerializeField]
        Mapping[] m_Mappings;

        Socket m_Socket;

        public Mapping[] mappings { get { return m_Mappings; }}

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }

        readonly float[] m_BlendShapesBuffer = new float[BlendShapeCount];
        Pose m_FacePose = new Pose(Vector3.zero, Quaternion.identity);
        Pose m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);
        public bool faceActive { get; private set; }
        public bool running { get; private set; }
        int m_LastFrameNum;

        readonly Queue<byte[]> m_BufferQueue = new Queue<byte[]>(k_BufferPrewarm);
        readonly Queue<byte[]> m_UnusedBuffers = new Queue<byte[]>(k_BufferPrewarm);

        [SerializeField]
        float m_BufferSize;

        public Pose facePose { get { return m_FacePose; } }
        public Pose cameraPose { get { return m_CameraPose; } }

        public List<string> locations
        {
            get
            {
                if (m_Locations == null)
                {
                    m_Locations = new List<string>();
                }
                if (m_Locations.Count != BlendShapeCount)
                {
                    m_Locations.Clear();
                    foreach (var location in ARBlendShapeLocation.Locations)
                    {
                        m_Locations.Add(Filter(location)); // Eliminate capitalization and _ mismatch
                    }
                }
                return m_Locations;
            }
        }

        List<string> m_Locations = new List<string>();

        void Awake()
        {
            Application.targetFrameRate = 60;
            for (var i = 0; i < k_BufferPrewarm; i++)
            {
                m_UnusedBuffers.Enqueue(new byte[BufferSize]);
            }

            foreach (var location in ARBlendShapeLocation.Locations)
            {
                m_Locations.Add(Filter(location)); // Eliminate capitalization and _ mismatch
            }

            var mappingLength = m_Mappings.Length;
            for (var i = 0; i < mappingLength; i++)
            {
                var mapping = m_Mappings[i];
                mapping.from = Filter(mapping.from);
                mapping.to = Filter(mapping.to);
            }

            m_Locations.Sort();
        }

        void Start()
        {
            Debug.Log("Possible IP addresses:");
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Debug.Log(address);

                var endPoint = new IPEndPoint(address, m_Port);
                m_Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(100);
                running = true;
                m_LastFrameNum = -1;
                var connectionAddress = address;
                new Thread(() =>
                {
                    m_Socket = m_Socket.Accept();
                    Debug.Log(string.Format("Client connected on {0}", connectionAddress));

                    var frameNumArray = new int[1];

                    while (running)
                    {
                        if (m_Socket.Connected)
                        {
                            try
                            {
                                var buffer = m_UnusedBuffers.Count == 0 ? new byte[BufferSize] : m_UnusedBuffers.Dequeue();
                                for (var i = 0; i < BufferSize; i++)
                                {
                                    buffer[i] = 0;
                                }

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

        public int GetLocationIndex(string location)
        {
            return locations.IndexOf(Filter(location));
        }

        public static string Filter(string @string)
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

            if (m_BufferQueue.Count > m_CatchupSize)
            {
                for (var i = 0; i < m_CatchupSize; i++)
                {
                    m_UnusedBuffers.Enqueue(m_BufferQueue.Dequeue()); // Throw out an old frame
                }
            }

            var poseArray = new float[7];
            var cameraPoseArray = new float[7];
            var buffer = m_BufferQueue.Dequeue();
            Buffer.BlockCopy(buffer, 1, m_BlendShapesBuffer, 0, BlendShapeSize);
            Buffer.BlockCopy(buffer, PoseOffset, poseArray, 0, PoseSize);
            Buffer.BlockCopy(buffer, CameraPoseOffset, cameraPoseArray, 0, PoseSize);
            ArrayToPose(poseArray, ref m_FacePose);
            ArrayToPose(cameraPoseArray, ref m_CameraPose);
            faceActive = faceActive && buffer[buffer.Length - 1] == 1;
            m_UnusedBuffers.Enqueue(buffer);

            return true;
        }

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64
            ;

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        void Update()
        {
            m_BufferSize = m_BufferQueue.Count;
            if (!DequeueBuffer())
                return;

            if (m_FacePose.position == m_LastPose)
            {
                m_TrackingLossCount++;
                if (m_TrackingLossCount > m_TrackingLossPadding)
                    faceActive = false;
                else
                    faceActive = true;
            }
            else
            {
                m_TrackingLossCount = 0;
            }
            m_LastPose = m_FacePose.position;
        }

        void OnDestroy()
        {
            running = false;
        }
    }

}
