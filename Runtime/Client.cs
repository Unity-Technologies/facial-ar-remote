#if INCLUDE_ARKIT_FACE_PLUGIN && INCLUDE_AR_FOUNDATION
#define FACETRACKING
#endif

using System;
using System.Net.Sockets;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARKit;
#if FACETRACKING
using System.Linq;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
#else
using UnityObject = UnityEngine.Object;
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

#pragma warning disable 649
        [SerializeField]
        [Tooltip("Stream settings that contain the settings for encoding the blend shapes' byte stream.")]
        StreamSettings m_StreamSettings;

#if FACETRACKING
        [SerializeField]
        ARFaceManager m_FaceManager;
#else
        [SerializeField]
        UnityObject m_FaceManager;
#endif
#pragma warning restore 649

        float[] m_BlendShapes;

        byte[] m_Buffer;
        Pose m_CameraPose;

        Transform m_CameraTransform;
        float m_CurrentTime = -1;

        Pose m_FacePose = new Pose(Vector3.zero, Quaternion.identity);
        bool m_Running;

        float m_StartTime;

#if FACETRACKING
        bool m_ARFaceActive;
        Dictionary<string, int> m_BlendShapeIndices;
#endif

        void Awake()
        {
            m_Buffer = new byte[m_StreamSettings.bufferSize];
            m_BlendShapes = new float[m_StreamSettings.BlendShapeCount];
            m_CameraTransform = Camera.main.transform;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if FACETRACKING
            m_FaceManager.facesChanged += ARFaceManagerOnFacesChanged;
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
#if FACETRACKING
            m_FaceManager.facesChanged -= ARFaceManagerOnFacesChanged;
#endif

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
#if FACETRACKING
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
#if FACETRACKING
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

#if FACETRACKING
        void ARFaceManagerOnFacesChanged(ARFacesChangedEventArgs changedEvent)
        {
            foreach (var arFace in changedEvent.removed)
            {
                FaceRemoved(arFace);
                // var trackableId = arFace.trackableId;
                // m_TrackedFaces.TryGetValue(trackableId, out var arFoundationFace);
                // arFace.ToARFoundationFace(m_ARFaceManager.subsystem, ref arFoundationFace);
                // m_TrackedFaces.Remove(trackableId);
                // RemoveFaceData(arFoundationFace);
            }

            foreach (var arFace in changedEvent.updated)
            {
                FaceUpdated(arFace);
                //UpdateFaceData(GetOrAddFace(arFace));
            }

            foreach (var arFace in changedEvent.added)
            {
                FaceAdded(arFace);
                //AddFaceData(GetOrAddFace(arFace));
            }
        }

        void FaceAdded(ARFace anchorData)
        {
            var anchorTransform = anchorData.transform;
            m_FacePose.position = anchorTransform.localPosition;
            m_FacePose.rotation = anchorTransform.localRotation;
            m_ARFaceActive = true;

            UpdateBlendShapes(anchorData);
        }

        void FaceUpdated(ARFace anchorData)
        {
            var anchorTransform = anchorData.transform;
            m_FacePose.position = anchorTransform.localPosition;
            m_FacePose.rotation = anchorTransform.localRotation;

            UpdateBlendShapes(anchorData);
        }

        void FaceRemoved(ARFace anchorData)
        {
            m_ARFaceActive = false;
        }

        void UpdateBlendShapes(ARFace anchorData)
        {
            var xrFaceSubsystem = m_FaceManager.subsystem;
            var arKitFaceSubsystem = (ARKitFaceSubsystem)xrFaceSubsystem;
            var blendShapes = new Dictionary<string, float>();

            var faceId = anchorData.trackableId;
            using (var blendShapeCoefficients = arKitFaceSubsystem.GetBlendShapeCoefficients(faceId, Allocator.Temp))
            {
                foreach (var featureCoefficient in blendShapeCoefficients)
                {
                    blendShapes[featureCoefficient.blendShapeLocation.ToString()] = featureCoefficient.coefficient;
                }
            }

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
