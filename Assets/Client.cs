using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Client : MonoBehaviour
{
    public const byte ErrorCheck = 42;
    public const int BlendshapeSize = sizeof(float) * BlendshapeDriver.BlendshapeCount;
    public const int PoseSize = sizeof(float) * 7;
    // 0 - Error check
    // 1-200 - Blendshapes
    // 201-228 - Pose
    // 229-256 - Camera Pose
    public const int BufferSize = 257;

    const float k_Timeout = 5;

    [SerializeField]
    ClientGUI m_ClientGUI;

    Transform m_CameraTransform;
    Pose m_CameraPose;

    Socket m_Socket;

    float m_StartTime;
    bool m_FreshData;

    readonly byte[] m_Buffer = new byte[BufferSize];

    public void Setup(Socket socket)
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_CameraTransform = Camera.main.transform;
        m_StartTime = Time.time;
        m_Socket = socket;
        enabled = true;
        //var posePtr = Marshal.AllocHGlobal(PoseSize);
        var poseArray = new float[7];
        var cameraPoseArray = new float[7];

        new Thread(() =>
        {
            while (true)
            {
                try {
                    if (m_Socket.Connected)
                    {
                        if (m_FreshData)
                        {
                            m_FreshData = false;
                            m_Buffer[0] = ErrorCheck;
                            Buffer.BlockCopy(BlendshapeDriver.BlendShapes, 0, m_Buffer, 1, BlendshapeSize);

                            // Marshal.StructureToPtr(UnityARFaceAnchorManager.Pose, posePtr, true);
                            // Marshal.Copy(posePtr, m_Buffer, BlendshapeSize + 1, PoseSize);
                            var pose = UnityARFaceAnchorManager.Pose;
                            PoseToArray(pose, poseArray);
                            PoseToArray(m_CameraPose, cameraPoseArray);
                            Buffer.BlockCopy(poseArray, 0, m_Buffer, BlendshapeSize + 1, PoseSize);
                            Buffer.BlockCopy(cameraPoseArray, 0, m_Buffer, BlendshapeSize + PoseSize + 1, PoseSize);

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
                    TryTimeout();
                }

                Thread.Sleep(10);
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
}
