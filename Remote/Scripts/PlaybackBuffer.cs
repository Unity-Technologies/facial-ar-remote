using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc />
    /// <summary>
    /// Contains settings and stream data for a recorded session.
    /// </summary>
    [Serializable]
    public class PlaybackBuffer : IStreamSettings
    {
        [SerializeField]
        [Tooltip("Name of the recorded stream.")]
        string m_Name;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Recorded byte stream of blend shape data.")]
        byte[] m_RecordStream = { };

        [SerializeField]
        [HideInInspector]
        [Tooltip("Error check byte value.")]
        byte m_ErrorCheck = 42;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Number of blend shapes in the stream.")]
        int m_BlendShapeCount = 51;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Size of blend shapes in the byte array.")]
        int m_BlendShapeSize = 51 * sizeof(float);

        [SerializeField]
        [HideInInspector]
        [Tooltip("Size of frame number value in byte array.")]
        int m_FrameNumberSize = sizeof(int);

        [SerializeField]
        [HideInInspector]
        [Tooltip("Size of frame time value in byte array.")]
        int m_FrameTimeSize = sizeof(float);

        [SerializeField]
        [HideInInspector]
        [Tooltip("Location of head pose in byte array.")]
        int m_HeadPoseOffset = 205;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Location of camera pose in byte array.")]
        int m_CameraPoseOffset = 233;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Location of frame number value in byte array.")]
        int m_FrameNumberOffset = 261;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Location of frame time value in byte array.")]
        int m_FrameTimeOffset = 265;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Total size of buffer of byte array for single same of data.")]
        int m_BufferSize = 270;

        [SerializeField]
        [Tooltip("String names of the blend shapes in the stream with their index in the array being their relative location.")]
        string[] m_Locations = {};

        [SerializeField]
        [Tooltip("Rename mapping values to apply blend shape locations to a blend shape controller.")]
        Mapping[] m_Mappings = {};

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int FrameNumberSize { get { return m_FrameNumberSize; } }
        public int FrameTimeSize { get { return m_FrameTimeSize; } }
        public int HeadPoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset { get { return m_CameraPoseOffset; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }
        public int FrameTimeOffset { get { return m_FrameTimeOffset; } }
        public int bufferSize { get { return m_BufferSize; } }
        public Mapping[] mappings { get { return m_Mappings; } }

        public string[] locations
        {
            get
            {
                if (m_Locations.Length != m_BlendShapeCount)
                {
                    UseDefaultLocations();
                }
                return m_Locations;
            }
        }

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public byte[] recordStream
        {
            get { return m_RecordStream; }
            set { m_RecordStream = value; }
        }

        PlaybackBuffer() {}

        public PlaybackBuffer(IStreamSettings streamSettings)
        {
            m_BufferSize = streamSettings.bufferSize;
            m_ErrorCheck = streamSettings.ErrorCheck;
            m_BlendShapeCount = streamSettings.BlendShapeCount;
            m_BlendShapeSize = streamSettings.BlendShapeSize;
            m_HeadPoseOffset = streamSettings.HeadPoseOffset;
            m_CameraPoseOffset = streamSettings.CameraPoseOffset;
            m_FrameNumberOffset = streamSettings.FrameNumberOffset;

            m_Locations = streamSettings.locations;
            m_Mappings = streamSettings.mappings;
        }

        public void UseDefaultLocations()
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
