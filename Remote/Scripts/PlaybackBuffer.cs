using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    public static class BlendShapeLocation
    {
        public static readonly string[] Locations = 
        {
            ARBlendShapeLocation.BrowDownLeft,
            ARBlendShapeLocation.BrowDownRight,
            ARBlendShapeLocation.BrowInnerUp,
            ARBlendShapeLocation.BrowOuterUpLeft,
            ARBlendShapeLocation.BrowOuterUpRight,
            ARBlendShapeLocation.CheekPuff,
            ARBlendShapeLocation.CheekSquintLeft,
            ARBlendShapeLocation.CheekSquintRight,
            ARBlendShapeLocation.EyeBlinkLeft,
            ARBlendShapeLocation.EyeBlinkRight,
            ARBlendShapeLocation.EyeLookDownLeft,
            ARBlendShapeLocation.EyeLookDownRight,
            ARBlendShapeLocation.EyeLookInLeft,
            ARBlendShapeLocation.EyeLookInRight,
            ARBlendShapeLocation.EyeLookOutLeft,
            ARBlendShapeLocation.EyeLookOutRight,
            ARBlendShapeLocation.EyeLookUpLeft,
            ARBlendShapeLocation.EyeLookUpRight,
            ARBlendShapeLocation.EyeSquintLeft,
            ARBlendShapeLocation.EyeSquintRight,
            ARBlendShapeLocation.EyeWideLeft,
            ARBlendShapeLocation.EyeWideRight,
            ARBlendShapeLocation.JawForward,
            ARBlendShapeLocation.JawLeft,
            ARBlendShapeLocation.JawOpen,
            ARBlendShapeLocation.JawRight,
            ARBlendShapeLocation.MouthClose,
            ARBlendShapeLocation.MouthDimpleLeft,
            ARBlendShapeLocation.MouthDimpleRight,
            ARBlendShapeLocation.MouthFrownLeft,
            ARBlendShapeLocation.MouthFrownRight,
            ARBlendShapeLocation.MouthFunnel,
            ARBlendShapeLocation.MouthLeft,
            ARBlendShapeLocation.MouthLowerDownLeft,
            ARBlendShapeLocation.MouthLowerDownRight,
            ARBlendShapeLocation.MouthPressLeft,
            ARBlendShapeLocation.MouthPressRight,
            ARBlendShapeLocation.MouthPucker,
            ARBlendShapeLocation.MouthRight,
            ARBlendShapeLocation.MouthRollLower,
            ARBlendShapeLocation.MouthRollUpper,
            ARBlendShapeLocation.MouthShrugLower,
            ARBlendShapeLocation.MouthShrugUpper,
            ARBlendShapeLocation.MouthSmileLeft,
            ARBlendShapeLocation.MouthSmileRight,
            ARBlendShapeLocation.MouthStretchLeft,
            ARBlendShapeLocation.MouthStretchRight,
            ARBlendShapeLocation.MouthUpperUpLeft,
            ARBlendShapeLocation.MouthUpperUpRight,
            ARBlendShapeLocation.NoseSneerLeft,
            ARBlendShapeLocation.NoseSneerRight,
//            ARBlendShapeLocation.TongueOut,
        };
    }
    
    [Serializable]
    public class PlaybackBuffer: IStreamSettings
    {
        [SerializeField]
        string m_Name;

        [SerializeField]
        [HideInInspector]
        byte[] m_RecordStream = { };

        [SerializeField]
        [HideInInspector]
        byte m_ErrorCheck = 42;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeCount = 51;

        [SerializeField]
        [HideInInspector]
        int m_BlendShapeSize = 51 * sizeof(float);

        [SerializeField]
        [HideInInspector]
        int m_PoseSize = 7 * sizeof(float);

        [SerializeField]
        [HideInInspector]
        int m_FrameNumberSize = sizeof(int);

        [SerializeField]
        [HideInInspector]
        int m_FrameTimeSize = sizeof(float);

        [SerializeField]
        [HideInInspector]
        int m_HeadPoseOffset = 205;

        [SerializeField]
        [HideInInspector]
        int m_CameraPoseOffset = 233;

        [SerializeField]
        [HideInInspector]
        int m_FrameNumberOffset = 261;

        [SerializeField]
        [HideInInspector]
        int m_FrameTimeOffset = 265;

        [SerializeField]
        [HideInInspector]
        int m_BufferSize = 270;

        [SerializeField]
        string[] m_Locations = {};

        [SerializeField]
        Mapping[] m_Mappings = {};

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int PoseSize { get { return m_PoseSize; } }
        public int FrameNumberSize { get { return m_FrameNumberSize; } }
        public int FrameTimeSize { get { return m_FrameTimeSize; } }
        public int HeadPoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset { get { return m_CameraPoseOffset; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }
        public int FrameTimeOffset { get { return m_FrameTimeOffset; } }
        public int BufferSize { get { return m_BufferSize; } }
        public Mapping[] mappings { get { return m_Mappings; } }

//        public string[] locations { get { return m_Locations; } }

        public string[] locations
        {
            get
            {
                if (m_Locations.Length != m_BlendShapeCount)
                {
                    var locs = new List<string>();
                    foreach (var location in BlendShapeLocation.Locations)
                    {
                        locs.Add(Filter(location)); // Eliminate capitalization and _ mismatch
                    }
                    m_Locations.ToArray();
                }
                return m_Locations;
            }
        }

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public byte[] recordStream { get { return m_RecordStream; } set { m_RecordStream = value; } }

        PlaybackBuffer() {}

        public PlaybackBuffer(IStreamSettings streamSettings)
        {
            SetStreamSettings(streamSettings);
        }

        public void SetStreamSettings(IStreamSettings streamSettings)
        {
            m_BufferSize = streamSettings.BufferSize;
            m_ErrorCheck = streamSettings.ErrorCheck;
            m_BlendShapeCount = streamSettings.BlendShapeCount;
            m_BlendShapeSize = streamSettings.BlendShapeSize;
            m_PoseSize = streamSettings.PoseSize;
            m_HeadPoseOffset = streamSettings.HeadPoseOffset;
            m_CameraPoseOffset = streamSettings.CameraPoseOffset;
            m_FrameNumberOffset = streamSettings.FrameNumberOffset;

            m_Locations = streamSettings.locations;
            m_Mappings = streamSettings.mappings;
        }

        public static string Filter(string @string)
        {
            return @string.ToLower().Replace("_", "");
        }

        public int GetLocationIndex(string location)
        {
//            return locations.IndexOf(Filter(location));
            return Array.IndexOf(locations, Filter(location));
        }

        public void UseDefaultLocations()
        {
            var locs = new List<string>();
            foreach (var location in BlendShapeLocation.Locations)
            {
                locs.Add(Filter(location)); // Eliminate capitalization and _ mismatch
            }
            locs.Sort();
            m_Locations = locs.ToArray();
        }
    }
}
