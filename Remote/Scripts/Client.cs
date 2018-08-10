using System;
using System.Net.Sockets;
using System.Threading;
#if UNITY_IOS
using System.Linq;
using System.Collections.Generic;
#endif
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#endif

namespace Unity.Labs.FacialRemote
{
    /// <summary>
    /// Streams blend shape data from a device to a connected server.
    /// </summary>
    class Client : MonoBehaviour
    {
        const float k_Timeout = 5;
        const int k_SleepTime = 4;

        [SerializeField]
        [Tooltip("Stream settings that contain the settings for encoding the blend shapes' byte stream.")]
        StreamSettings m_StreamSettings;

        float[] m_BlendShapes;

        byte[] m_Buffer;
        Pose m_CameraPose;

        Transform m_CameraTransform;
        float m_CurrentTime = -1;

        Pose m_FacePose = new Pose(Vector3.zero, Quaternion.identity);
        bool m_Running;

        float m_StartTime;

#if UNITY_IOS
        bool m_ARFaceActive;
        Dictionary<string, int> m_BlendShapeIndices;
#endif

        void Awake()
        {
            m_Buffer = new byte[m_StreamSettings.bufferSize];
            m_BlendShapes = new float[m_StreamSettings.BlendShapeCount];
            m_CameraTransform = Camera.main.transform;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_IOS
            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
            UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
#endif
        }

        void Start()
        {
            // Wait to be enabled on StartCapture
            enabled = false;
            if (m_StreamSettings == null)
            {
                Debug.LogError("Stream settings is not assigned! Deactivating Client.");
                gameObject.SetActive(false);
            }
        }

        void Update()
        {
            m_CameraPose = new Pose(m_CameraTransform.position, m_CameraTransform.rotation);
            m_CurrentTime = Time.time;
        }

        void OnDestroy()
        {
            m_Running = false;
        }

        /// <summary>
        /// Starts stream thread using the provided socket.
        /// </summary>
        /// <param name="socket">The socket to use when streaming blend shape data.</param>
        public void StartCapture(Socket socket)
        {
            m_CameraTransform = Camera.main.transform;
            if (!m_CameraTransform)
            {
                Debug.LogWarning("Could not find main camera. Camera pose will not be captured");
                m_CameraTransform = transform; // Use this transform to avoid null references
            }

            m_StartTime = Time.time;
            enabled = true;
            var poseArray = new float[BlendShapeUtils.PoseFloatCount];
            var cameraPoseArray = new float[BlendShapeUtils.PoseFloatCount];
            var frameNum = new int[1];
            var frameTime = new float[1];

            Application.targetFrameRate = 60;
            m_Running = true;

            new Thread(() =>
            {
                var count = 0;
#if UNITY_IOS
                var lastIndex = m_Buffer.Length - 1;
#endif
                while (m_Running)
                {
                    try
                    {
                        if (socket.Connected)
                        {
                            if (m_CurrentTime > 0)
                            {
                                frameNum[0] = count++;
                                frameTime[0] = m_CurrentTime - m_StartTime;
                                m_CurrentTime = -1;

                                m_Buffer[0] = m_StreamSettings.ErrorCheck;
                                Buffer.BlockCopy(m_BlendShapes, 0, m_Buffer, 1, m_StreamSettings.BlendShapeSize);

                                BlendShapeUtils.PoseToArray(m_FacePose, poseArray);
                                BlendShapeUtils.PoseToArray(m_CameraPose, cameraPoseArray);

                                Buffer.BlockCopy(poseArray, 0, m_Buffer, m_StreamSettings.HeadPoseOffset, BlendShapeUtils.PoseSize);
                                Buffer.BlockCopy(cameraPoseArray, 0, m_Buffer, m_StreamSettings.CameraPoseOffset, BlendShapeUtils.PoseSize);
                                Buffer.BlockCopy(frameNum, 0, m_Buffer, m_StreamSettings.FrameNumberOffset, m_StreamSettings.FrameNumberSize);
                                Buffer.BlockCopy(frameTime, 0, m_Buffer, m_StreamSettings.FrameTimeOffset, m_StreamSettings.FrameTimeSize);
#if UNITY_IOS
                                m_Buffer[lastIndex] = (byte)(m_ARFaceActive ? 1 : 0);
#endif

                                socket.Send(m_Buffer);
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

                    Thread.Sleep(k_SleepTime);
                }
            }).Start();
        }

#if UNITY_IOS
        void FaceAdded(ARFaceAnchor anchorData)
        {
            var anchorTransform = anchorData.transform;
            m_FacePose.position = UnityARMatrixOps.GetPosition(anchorTransform);
            m_FacePose.rotation = UnityARMatrixOps.GetRotation(anchorTransform);
            m_ARFaceActive = true;

            var blendShapes = anchorData.blendShapes;

            if (m_BlendShapeIndices == null)
            {
                m_BlendShapeIndices = new Dictionary<string, int>();

                var names = m_StreamSettings.locations.ToList();
                names.Sort();

                foreach (var kvp in blendShapes)
                {
                    var key = kvp.Key;
                    var index = names.IndexOf(key);
                    if (index >= 0)
                        m_BlendShapeIndices[key] = index;
                }
            }

            UpdateBlendShapes(blendShapes);
        }

        void FaceUpdated(ARFaceAnchor anchorData)
        {
            var anchorTransform = anchorData.transform;
            m_FacePose.position = UnityARMatrixOps.GetPosition(anchorTransform);
            m_FacePose.rotation = UnityARMatrixOps.GetRotation(anchorTransform);

            UpdateBlendShapes(anchorData.blendShapes);
        }

        void FaceRemoved(ARFaceAnchor anchorData)
        {
            m_ARFaceActive = false;
        }

        void UpdateBlendShapes(Dictionary<string, float> blendShapes)
        {
            foreach (var kvp in blendShapes)
            {
                int index;
                if (m_BlendShapeIndices.TryGetValue(kvp.Key, out index))
                    m_BlendShapes[index] = kvp.Value;
            }
        }
#endif

        void TryTimeout()
        {
            if (m_CurrentTime - m_StartTime > k_Timeout)
            {
                m_Running = false;
                enabled = false;
            }
        }
    }
}
