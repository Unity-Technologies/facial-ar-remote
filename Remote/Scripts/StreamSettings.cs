using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote {
    [Serializable]
    [CreateAssetMenu(fileName = "Stream Settings", menuName = "FacialRemote/Stream Settings")]
    public class StreamSettings : ScriptableObject
    {
        [SerializeField]
        byte m_ErrorCheck = 42;
        [SerializeField]
        int m_BlendShapeCount = 51;

        bool m_Initialized;

        public bool initialized { get { return m_Initialized; } }
        public byte errorCheck { get { return m_ErrorCheck; } }
        public int blendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get; private set; }
        public int PoseSize  { get; private set; }
        public int PoseOffset  { get; private set; }
        public int CameraPoseOffset  { get; private set; }
        public int FrameNumberOffset  { get; private set; }

        // 0 - Error check
        // 1-204 - Blendshapes
        // 205-232 - Pose
        // 233-260 - Camera Pose
        // 261-264 - Frame Number
        // 265 - Active state
        public int BufferSize { get; private set; }

        [SerializeField]
        Mapping[] m_Mappings;

        public Mapping[] mappings { get { return m_Mappings; }}

        public List<string> locations
        {
            get
            {
                if (m_Locations == null)
                {
                    m_Locations = new List<string>();
                }
                if (m_Locations.Count != m_BlendShapeCount)
                {
                    m_Locations.Clear();
                    foreach (var location in ARBlendShapeLocation.Locations)
                    {
                        m_Locations.Add(Filter(location)); // Eliminate capitalization and _ mismatch
                    }
                }
                return m_Locations;
            }
        }

        List<string> m_Locations = new List<string>();

        public void Initialize()
        {
            if (!m_Initialized)
                Awake();
        }

        void Awake()
        {
            BlendShapeSize = sizeof(float) * m_BlendShapeCount;
            PoseSize = sizeof(float) * 7;
            PoseOffset = BlendShapeSize + 1;
            CameraPoseOffset = PoseOffset + PoseSize;
            FrameNumberOffset = CameraPoseOffset + PoseSize;
            BufferSize = 1 + BlendShapeSize + PoseSize * 2 + sizeof(float) + 1;

            foreach (var location in ARBlendShapeLocation.Locations)
            {
                m_Locations.Add(Filter(location)); // Eliminate capitalization and _ mismatch
            }

            var mappingLength = m_Mappings.Length;
            for (var i = 0; i < mappingLength; i++)
            {
                var mapping = m_Mappings[i];
                mapping.from = Filter(mapping.from);
                mapping.to = Filter(mapping.to);
            }

            m_Locations.Sort();

            m_Initialized = true;
        }

        void OnDestroy()
        {
            m_Initialized = false;
        }

        public int GetLocationIndex(string location)
        {
            return locations.IndexOf(Filter(location));
        }

        public static string Filter(string @string)
        {
            return @string.ToLower().Replace("_", "");
        }
    }
}
