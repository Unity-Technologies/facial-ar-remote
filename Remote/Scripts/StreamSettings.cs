using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class Mapping
    {
        [SerializeField]
        string m_From;

        [SerializeField]
        string m_To;

        public string from
        {
            get { return m_From; }
        }

        public string to
        {
            get { return m_To; }
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "Stream Settings", menuName = "FacialRemote/Stream Settings")]
    public class StreamSettings : ScriptableObject, IStreamSettings
    {
        [SerializeField]
        byte m_ErrorCheck = 42;

        [SerializeField]
        int m_BlendShapeCount = 51;

        [SerializeField]
        Mapping[] m_Mappings = {};

        [SerializeField]
        int m_BlendShapeSize;

        [SerializeField]
        int m_FrameNumberSize;

        [SerializeField]
        int m_FrameTimeSize;

        [SerializeField]
        int m_HeadPoseOffset;

        [SerializeField]
        int m_CameraPoseOffset;

        [SerializeField]
        int m_FrameNumberOffset;

        [SerializeField]
        int m_FrameTimeOffset;

        [SerializeField]
        int m_BufferSize;

        [SerializeField]
        string[] m_Locations = { };

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int FrameNumberSize { get { return m_FrameNumberSize; } }
        public int FrameTimeSize { get { return m_FrameTimeSize; } }
        public int HeadPoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset  { get { return m_CameraPoseOffset; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }
        public int FrameTimeOffset { get { return m_FrameTimeOffset;  } }

        // 0 - Error check
        // 1-204 - Blendshapes
        // 205-232 - Pose
        // 233-260 - Camera Pose
        // 261-264 - Frame Number
        // 265-268 - Frame Time
        // 269 - Active state
        public int BufferSize { get { return m_BufferSize; } }

        public Mapping[] mappings { get { return m_Mappings; }}

        public string[] locations
        {
            get
            {
                if (m_Locations.Length != m_BlendShapeCount)
                {
                    var locs = new List<string>();
                    foreach (var location in BlendShapeUtils.Locations)
                    {
                        locs.Add(location); // Eliminate capitalization and _ mismatch
                    }
                    m_Locations = locs.ToArray();
                }
                return m_Locations;
            }
        }

        void OnValidate()
        {
            var poseSize = BlendShapeUtils.PoseSize;
            m_BlendShapeSize = sizeof(float) * m_BlendShapeCount;
            m_FrameNumberSize = sizeof(int);
            m_FrameTimeSize = sizeof(float);
            m_HeadPoseOffset = BlendShapeSize + 1;
            m_CameraPoseOffset = HeadPoseOffset + poseSize;
            m_FrameNumberOffset = CameraPoseOffset + poseSize;
            m_FrameTimeOffset = FrameNumberOffset + FrameNumberSize;
            m_BufferSize = 1 + BlendShapeSize + poseSize * 2 + FrameNumberSize + FrameTimeSize + 1;

            if (m_Locations.Length == 0 || m_Locations.Length != m_BlendShapeCount)
            {
                var locs = new List<string>();
                foreach (var location in BlendShapeUtils.Locations)
                {
                    locs.Add(location);
                }

                locs.Sort();
                m_Locations = locs.ToArray();
            }
        }
    }
}
