using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    public interface IStreamSettings
    {
        bool Initialized { get; }
        byte ErrorCheck { get; }
        int BlendShapeCount { get; }
        int BlendShapeSize { get; }
        int PoseSize { get; }
        int FrameNumberSize { get; }
        int FrameTimeSize { get; }
        int HeadPoseOffset { get; }
        int CameraPoseOffset { get; }
        int FrameNumberOffset  { get; }
        int FrameTimeOffset { get; }
        int BufferSize { get; }

        void Initialize();
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

        [NonSerialized]
        bool m_Initialized;

        public bool Initialized { get { return m_Initialized; } }
        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get; private set; }
        public int PoseSize { get; private set; }
        public int FrameNumberSize { get; private set; }
        public int FrameTimeSize { get; private set; }
        public int HeadPoseOffset { get; private set; }
        public int CameraPoseOffset  { get; private set; }
        public int FrameNumberOffset  { get; private set; }
        public int FrameTimeOffset { get; private set; }

        // 0 - Error check
        // 1-204 - Blendshapes
        // 205-232 - Pose
        // 233-260 - Camera Pose
        // 261-264 - Frame Number
        // 265-268 - Frame Time
        // 269 - Active state
        public int BufferSize { get; private set; }

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
            {
                BlendShapeSize = sizeof(float) * m_BlendShapeCount;
                PoseSize = sizeof(float) * 7;
                FrameNumberSize = sizeof(int);
                FrameTimeSize = sizeof(float);
                HeadPoseOffset = BlendShapeSize + 1;
                CameraPoseOffset = HeadPoseOffset + PoseSize;
                FrameNumberOffset = CameraPoseOffset + PoseSize;
                FrameTimeOffset = FrameNumberOffset + FrameNumberSize;
                BufferSize = 1 + BlendShapeSize + PoseSize * 2 + FrameNumberSize + FrameTimeSize + 1;
                Debug.Log(string.Format("Buffer Size: {0}", BufferSize));

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

        void OnValidate()
        {
            m_Initialized = false;
        }
    }
}
