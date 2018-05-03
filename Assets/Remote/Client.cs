using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.iOS;

class Client : MonoBehaviour
{
    const float k_Timeout = 5;

    [SerializeField]
    ClientGUI m_ClientGUI;

    Transform m_CameraTransform;
    Pose m_CameraPose;

    Socket m_Socket;

    float m_StartTime;
    bool m_FreshData;
    bool m_Running;

    readonly byte[] m_Buffer = new byte[Server.BufferSize];
    readonly float[] m_Blendshapes = new float[Server.BlendshapeCount];

    Dictionary<string, float> currentBlendShapes;
    Dictionary<string, int> blendShapeIndices;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
        UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
    }

    void FaceAdded (ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;

        if (blendShapeIndices == null)
        {
            blendShapeIndices = new Dictionary<string, int>();

            var names = currentBlendShapes.Keys.ToList();
            names.Sort();
            foreach (var kvp in currentBlendShapes)
            {
                blendShapeIndices[kvp.Key] = names.IndexOf(kvp.Key);
            }
        }

        UpdateBlendshapes();
    }

    void FaceUpdated (ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
        UpdateBlendshapes();
    }

    void UpdateBlendshapes()
    {
        foreach (var kvp in currentBlendShapes)
        {
            m_Blendshapes[blendShapeIndices[kvp.Key]] = kvp.Value;
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
                            m_Buffer[0] = Server.ErrorCheck;
                            Buffer.BlockCopy(m_Blendshapes, 0, m_Buffer, 1, Server.BlendshapeSize);

                            var pose = UnityARFaceAnchorManager.Pose;
                            PoseToArray(pose, poseArray);
                            PoseToArray(m_CameraPose, cameraPoseArray);

                            const int poseOffset = Server.BlendshapeSize + 1;
                            const int cameraPoseOffset = poseOffset + Server.PoseSize;
                            const int frameNumOffset = cameraPoseOffset + Server.PoseSize;

                            frameNum[0] = count++;
                            Buffer.BlockCopy(poseArray, 0, m_Buffer, poseOffset, Server.PoseSize);
                            Buffer.BlockCopy(cameraPoseArray, 0, m_Buffer, cameraPoseOffset, Server.PoseSize);
                            Buffer.BlockCopy(frameNum, 0, m_Buffer, frameNumOffset, sizeof(int));
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

                Thread.Sleep(5);
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
        m_CameraPose = new Pose(m_CameraTransform.position, m_CameraTransform.rotation);
        m_FreshData = true;
    }

    void TryTimeout()
    {
        if (Time.time - m_StartTime > k_Timeout)
        {
            enabled = false;
            m_ClientGUI.enabled = true;
        }
    }

    void OnDestroy()
    {
        m_Running = false;
    }
}
