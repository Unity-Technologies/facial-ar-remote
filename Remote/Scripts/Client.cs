using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    class Client : MonoBehaviour
    {
        const float k_Timeout = 5;

        [SerializeField]
        ClientGUI m_ClientGUI;

        [SerializeField]
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

        Dictionary<string, float> m_CurrentBlendShapes;
        Dictionary<string, int> m_BlendShapeIndices;

        void Awake()
        {
            if (m_StreamSettings == null)
            {
                Debug.LogError("Stream settings needs to be assigned!");
                enabled = false;
                return;
            }

            m_Buffer = new byte[m_StreamSettings.BufferSize];
            m_BlendShapes = new float[m_StreamSettings.BlendShapeCount];
            m_CameraTransform = Camera.main.transform;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
        }

        void FaceAdded (ARFaceAnchor anchorData)
        {
            m_CurrentBlendShapes = anchorData.blendShapes;

            if (m_BlendShapeIndices == null)
            {
                m_BlendShapeIndices = new Dictionary<string, int>();

                var names = m_CurrentBlendShapes.Keys.ToList();
                names.Sort();
                foreach (var kvp in m_CurrentBlendShapes)
                {
                    m_BlendShapeIndices[kvp.Key] = names.IndexOf(kvp.Key);
                }
            }

            UpdateBlendShapes();
        }

        void FaceUpdated (ARFaceAnchor anchorData)
        {
            m_CurrentBlendShapes = anchorData.blendShapes;
            UpdateBlendShapes();
        }

        void UpdateBlendShapes()
        {
            foreach (var kvp in m_CurrentBlendShapes)
            {
                m_BlendShapes[m_BlendShapeIndices[kvp.Key]] = kvp.Value;
            }
        }

        public void Setup(Socket socket)
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

                                var pose = UnityARFaceAnchorManager.Pose;
                                PoseToArray(pose, poseArray);
                                PoseToArray(m_CameraPose, cameraPoseArray);

                                frameNum[0] = count++;
                                frameTime[0] = m_TimeStamp;
                                Buffer.BlockCopy(poseArray, 0, m_Buffer, m_StreamSettings.PoseOffset, m_StreamSettings.PoseSize);
                                Buffer.BlockCopy(cameraPoseArray, 0, m_Buffer, m_StreamSettings.CameraPoseOffset, m_StreamSettings.PoseSize);
                                Buffer.BlockCopy(frameNum, 0, m_Buffer, m_StreamSettings.FrameNumberOffset, m_StreamSettings.FrameTimeSize);
                                Buffer.BlockCopy(frameTime, 0, m_Buffer, m_StreamSettings.FrameTimeOffset, m_StreamSettings.FrameTimeSize);
                                m_Buffer[m_Buffer.Length - 1] = (byte)(UnityARFaceAnchorManager.active ? 1 : 0);

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

        static void PoseToArray(Pose pose, float[] poseArray)
        {
            var position = pose.position;
            var rotation = pose.rotation;
            poseArray[0] = position.x;
            poseArray[1] = position.y;
            poseArray[2] = position.z;
            poseArray[3] = rotation.x;
            poseArray[4] = rotation.y;
            poseArray[5] = rotation.z;
            poseArray[6] = rotation.w;
        }

        void Update()
        {
            // TODO think may want to update information in LateUpdate
            m_CameraPose = new Pose(m_CameraTransform.position, m_CameraTransform.rotation);
            m_FreshData = true;
            if (m_Once)
                m_TimeStamp = 0f;
            else
                m_TimeStamp += Time.deltaTime;
        }

        void TryTimeout()
        {
            if (Time.time - m_StartTime > k_Timeout)
            {
                enabled = false;
                m_ClientGUI.enabled = true;
                m_TimeStamp = 0f;
            }
        }

        void OnDestroy()
        {
            m_Running = false;
        }
    }
}
