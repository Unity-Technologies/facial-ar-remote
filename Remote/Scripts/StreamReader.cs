using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class StreamReader : MonoBehaviour
    {
        Pose m_HeadPose = new Pose(Vector3.zero, Quaternion.identity);
        Pose m_CameraPose = new Pose(Vector3.zero, Quaternion.identity);

        public bool faceActive { get; private set; }
        public bool trackingActive { get; private set; }
        public Pose headPose { get { return m_HeadPose; } }
        public Pose cameraPose { get { return m_CameraPose; } }

        public bool streamActive
        {
            get { return enabled && streamSource != null && streamSource.streamActive; }
        }

        public StreamSource streamSource { get; private set; }

        Vector3 m_LastPose;
        int m_TrackingLossCount;

        [SerializeField]
        [Range(1, 512)]
        int m_TrackingLossPadding = 64;

        public float[] blendShapesBuffer { get { return m_BlendShapesBuffer; } }
        public float[] headPoseArray { get { return m_HeadPoseArray; } }
        public float[] cameraPoseArray { get { return m_CameraPoseArray; } }

        float[] m_BlendShapesBuffer;
        float[] m_HeadPoseArray = new float[7];
        float[] m_CameraPoseArray = new float[7];

        List<IConnectedController> m_ConnectedControllers = new List<IConnectedController>();

        public void SetStreamSource(StreamSource source)
        {
            if (source == null || source.GetStreamSettings() == null)
                return;
            streamSource = source;
            m_BlendShapesBuffer = new float[source.GetStreamSettings().BlendShapeCount];

            UpdateStreamSettings(source.GetStreamSettings());
        }

        public void UnSetStreamSource()
        {
            streamSource = null;
        }

        public void UpdateStreamData(StreamSource source, ref byte[] buffer, int position)
        {
            if (source == null || source != streamSource || streamSource.GetStreamSettings() == null)
            {
                return;
            }
            var streamSettings = streamSource.GetStreamSettings();

            Buffer.BlockCopy(buffer, position + 1, m_BlendShapesBuffer, 0, streamSettings.BlendShapeSize);
            Buffer.BlockCopy(buffer, position + streamSettings.HeadPoseOffset, m_HeadPoseArray, 0, streamSettings.PoseSize);
            Buffer.BlockCopy(buffer, position + streamSettings.CameraPoseOffset, m_CameraPoseArray, 0, streamSettings.PoseSize);
            faceActive = buffer[position + streamSettings.BufferSize - 1] == 1;

            if (faceActive)
            {
                ArrayToPose(headPoseArray, ref m_HeadPose);
                ArrayToPose(cameraPoseArray, ref m_CameraPose);
            }
        }

        public void AddConnectedController(IConnectedController connectedController)
        {
            if (!m_ConnectedControllers.Contains(connectedController))
                m_ConnectedControllers.Add(connectedController);
        }

        public void RemoveConnectedController(IConnectedController connectedController)
        {
            if (m_ConnectedControllers.Contains(connectedController))
                m_ConnectedControllers.Remove(connectedController);
        }

        public void UpdateStreamSettings(IStreamSettings streamSettings)
        {
            foreach (var controller in m_ConnectedControllers)
            {
                controller.SetStreamSettings(streamSettings);
            }
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
