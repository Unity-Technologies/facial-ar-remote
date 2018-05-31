using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class BlendShapeReader : MonoBehaviour
    {
        Pose m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
        Pose m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);

        public bool faceActive { get; private set; }
        public bool trackingActive { get; private set; }
        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        [SerializeField]
        StreamSettings m_StreamSettings;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        public StreamSettings streamSettings { get { return m_StreamSettings; } }

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }
        public float[] headPoseArray { get { return m_HeadPoseArray; } }
        public float[] cameraPoseArray { get { return m_CameraPoseArray; } }

        float[] m_BlendShapesBuffer;
        float[] m_HeadPoseArray = new float[7];
        float[] m_CameraPoseArray = new float[7];

        public void UpdateStreamData(ref byte[] buffer, int position)
        {
            Buffer.BlockCopy(buffer, position + 1, m_BlendShapesBuffer, 0, m_StreamSettings.BlendShapeSize);
            Buffer.BlockCopy(buffer, position + m_StreamSettings.PoseOffset, m_HeadPoseArray, 0, m_StreamSettings.PoseSize);
            Buffer.BlockCopy(buffer, position + m_StreamSettings.CameraPoseOffset, m_CameraPoseArray, 0, m_StreamSettings.PoseSize);
            faceActive = buffer[position + m_StreamSettings.BufferSize - 1] == 1;

            if (faceActive)
            {
                ArrayToPose(headPoseArray, ref m_HeadPose);
                ArrayToPose(cameraPoseArray, ref m_CameraPose);
            }
        }

        void Awake()
        {
            m_BlendShapesBuffer = new float[m_StreamSettings.blendShapeCount];
        }

        void Update()
        {
            if (m_HeadPose.position == m_LastPose)
            {
                m_TrackingLossCount++;
                if (!faceActive && m_TrackingLossCount > m_TrackingLossPadding)
                    trackingActive = false;
                else
                    trackingActive = true;
            }
            else
            {
                m_TrackingLossCount = 0;
            }
            m_LastPose = m_HeadPose.position;
        }

        static void ArrayToPose(float[] poseArray, ref Pose pose)
        {
            pose.position = new Vector3(poseArray[0], poseArray[1], poseArray[2]);
            pose.rotation = new Quaternion(poseArray[3], poseArray[4], poseArray[5], poseArray[6]);
        }
    }
}
