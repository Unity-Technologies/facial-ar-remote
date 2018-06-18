using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class Mapping
    {
        public string from;
        public string to;
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
        public int m_BlendShapeSize;
        [SerializeField]
        public int m_PoseSize;
        [SerializeField]
        public int m_FrameNumberSize;
        [SerializeField]
        public int m_FrameTimeSize;
        [SerializeField]
        public int m_HeadPoseOffset;
        [SerializeField]
        public int m_CameraPoseOffset;
        [SerializeField]
        public int m_FrameNumberOffset;
        [SerializeField]
        public int m_FrameTimeOffset;
        [SerializeField]
        public int m_BufferSize;

        [SerializeField]
        string[] m_Locations = { };

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int PoseSize { get { return m_PoseSize; } }
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
                    m_Locations.ToArray();
                }
                return m_Locations;
            }
        }

        public void ValidateData()
        {
            if (m_BlendShapeCount != BlendShapeUtils.Locations.Length)
                Debug.LogWarningFormat("Blend Shape Count of {0} does not match Plugin's Blend Shape Location Length of {1}",
                    m_BlendShapeCount, BlendShapeUtils.Locations.Length);
            m_BlendShapeSize = sizeof(float) * m_BlendShapeCount;
            m_PoseSize = sizeof(float) * 7;
            m_FrameNumberSize = sizeof(int);
            m_FrameTimeSize = sizeof(float);
            m_HeadPoseOffset = BlendShapeSize + 1;
            m_CameraPoseOffset = HeadPoseOffset + PoseSize;
            m_FrameNumberOffset = CameraPoseOffset + PoseSize;
            m_FrameTimeOffset = FrameNumberOffset + FrameNumberSize;
            m_BufferSize = 1 + BlendShapeSize + PoseSize * 2 + FrameNumberSize + FrameTimeSize + 1;

            if (m_Locations.Length == 0 || m_Locations.Length != m_BlendShapeCount)
            {
                var locs = new List<string>();
                foreach (var location in BlendShapeUtils.Locations)
                {
//                    locs.Add(Filter(location)); // Eliminate capitalization and _ mismatch
                    locs.Add(location); // Eliminate capitalization and _ mismatch
                }
                locs.Sort();
                m_Locations = locs.ToArray();
            }

//            var mappingLength = m_Mappings.Length;
//            for (var i = 0; i < mappingLength; i++)
//            {
//                var mapping = m_Mappings[i];
//                mapping.from = Filter(mapping.from);
//                mapping.to = Filter(mapping.to);
//            }
        }
    }
}
