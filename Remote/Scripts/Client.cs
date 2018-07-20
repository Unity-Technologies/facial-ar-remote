using System;
#if UNITY_IOS
using System.Linq;
using System.Collections.Generic;
#endif
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#endif

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Streams blend shape data from a device to a connected server.
    /// </summary>
    public class Client : MonoBehaviour
    {
        const float k_Timeout = 5;

        [SerializeField]
        [Tooltip("GUI used to interact with the client.")]
        ClientGUI m_ClientGUI;

        [SerializeField]
        [Tooltip("Stream settings that contain the settings for encoding the blend shapes' byte stream.")]
        StreamSettings m_StreamSettings;

        Transform m_CameraTransform;
        Pose m_CameraPose;

        Socket m_Socket;

        float m_StartTime;
        bool m_FreshData;
        float m_TimeStamp;
        bool m_Once;
        bool m_Running;

        byte[] m_Buffer;
        float[] m_BlendShapes;

#if UNITY_IOS
        Dictionary<string, float> m_CurrentBlendShapes;
        Dictionary<string, int> m_BlendShapeIndices;
#endif

        Pose m_FacePose = new Pose(Vector3.zero, Quaternion.identity);

        bool m_ARFaceActive;

        void Awake()
        {
            InitializeClient();

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_IOS
            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
            UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
#endif
        }

        void Start()
        {
            if (m_StreamSettings == null)
            {
                Debug.LogError("Stream settings is not assigned! Disabling Client.");
                enabled = false;
            }

            if (m_ClientGUI == null)
            {
                Debug.LogWarning("Stream Settings is not assigned. Disabling Client.");
            }
        }

        void Update()
        {
            if(m_Socket == null || m_CameraTransform == null)
                return;

            m_CameraPose = new Pose(m_CameraTransform.position, m_CameraTransform.rotation);
            m_FreshData = true;

            if (m_Socket.Connected && m_Once)
            {
                m_TimeStamp = 0f;
                m_Once = false;
            }
            else
                m_TimeStamp += Time.deltaTime;
        }

        void OnDestroy()
        {
            m_Running = false;
        }

        /// <summary>
        /// Initializes client's starting values.
        /// </summary>
        public void InitializeClient()
        {
            if (m_StreamSettings != null)
            {
                m_Buffer = new byte[m_StreamSettings.BufferSize];
                m_BlendShapes = new float[m_StreamSettings.BlendShapeCount];
                m_CameraTransform = Camera.main.transform;
            }
        }

        /// <summary>
        /// Starts stream thread using the provided socket.
        /// </summary>
        /// <param name="socket">The socket to use when streaming blend shape data.</param>
        public void SetupSocket(Socket socket)
        {
            m_CameraTransform = Camera.main.transform;
            m_StartTime = Time.time;
            m_Socket = socket;
            enabled = true;
            var poseArray = new float[7];
            var cameraPoseArray = new float[7];
            var frameNum = new int[1];
            var frameTime = new float[1];
            m_Once = true;
            Application.targetFrameRate = 60;
            m_Running = true;
            new Thread(() =>
            {
                var count = 0;
                while (m_Running)
                {
                    try {
                        if (m_Socket.Connected)
                        {
                            if (m_FreshData)
                            {
                                m_FreshData = false;
                                m_Buffer[0] = m_StreamSettings.ErrorCheck;
                                Buffer.BlockCopy(m_BlendShapes, 0, m_Buffer, 1, m_StreamSettings.BlendShapeSize);

                                BlendShapeUtils.PoseToArray(m_FacePose, poseArray);
                                BlendShapeUtils.PoseToArray(m_CameraPose, cameraPoseArray);

                                frameNum[0] = count++;
                                frameTime[0] = m_TimeStamp;
                                Buffer.BlockCopy(poseArray, 0, m_Buffer, m_StreamSettings.HeadPoseOffset, m_StreamSettings.PoseSize);
                                Buffer.BlockCopy(cameraPoseArray, 0, m_Buffer, m_StreamSettings.CameraPoseOffset, m_StreamSettings.PoseSize);
                                Buffer.BlockCopy(frameNum, 0, m_Buffer, m_StreamSettings.FrameNumberOffset, m_StreamSettings.FrameTimeSize);
                                Buffer.BlockCopy(frameTime, 0, m_Buffer, m_StreamSettings.FrameTimeOffset, m_StreamSettings.FrameTimeSize);
                                m_Buffer[m_Buffer.Length - 1] = (byte)(m_ARFaceActive ? 1 : 0);

                                m_Socket.Send(m_Buffer);
                            }
                        }
                        else
                        {
                            TryTimeout();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        TryTimeout();
                    }

                    Thread.Sleep(4);
                }
            }).Start();
        }

#if UNITY_IOS
        void FaceAdded (ARFaceAnchor anchorData)
        {
            m_FacePose.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            m_FacePose.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
            m_ARFaceActive = true;

            m_CurrentBlendShapes = anchorData.blendShapes;

            if (m_BlendShapeIndices == null)
            {
                m_BlendShapeIndices = new Dictionary<string, int>();

                var names = m_StreamSettings.locations.ToList();
                names.Sort();

                foreach (var kvp in m_CurrentBlendShapes)
                {
                    var index = names.IndexOf(kvp.Key);
                    if (index >= 0)
                        m_BlendShapeIndices[kvp.Key] = index;
                }
            }

            UpdateBlendShapes();
        }

        void FaceUpdated (ARFaceAnchor anchorData)
        {
            m_FacePose.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            m_FacePose.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

            m_CurrentBlendShapes = anchorData.blendShapes;
            UpdateBlendShapes();
        }

        void FaceRemoved (ARFaceAnchor anchorData)
        {
            m_ARFaceActive = false;
        }


        void UpdateBlendShapes()
        {
            foreach (var kvp in m_CurrentBlendShapes)
            {
                int index;
                if (m_BlendShapeIndices.TryGetValue(kvp.Key, out index))
                    m_BlendShapes[index] = kvp.Value;
            }
        }
#endif

        void TryTimeout()
        {
            if (Time.time - m_StartTime > k_Timeout)
            {
                enabled = false;
                if (m_ClientGUI != null)
                    m_ClientGUI.enabled = true;
                m_Once = true;
            }
        }
    }
}
