using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref = "IStreamSettings" />
    /// <summary>
    /// Holds the data needed to process facial data to and from a byte stream.
    /// This data needs to match on the server and client side.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Stream Settings", menuName = "AR Face Capture/Stream Settings")]
    public class StreamSettings : ScriptableObject, IStreamSettings
    {
#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("Error check byte value.")]
        byte m_ErrorCheck = 42;

        [SerializeField]
        [Tooltip("Number of blend shapes in the stream.")]
        int m_BlendShapeCount = 51;

        [SerializeField]
        [Tooltip("Size of blend shapes in the byte array.")]
        int m_BlendShapeSize;

        [SerializeField]
        [Tooltip("Size of frame number value in byte array.")]
        int m_FrameNumberSize;

        [SerializeField]
        [Tooltip("Size of frame time value in byte array.")]
        int m_FrameTimeSize;

        [SerializeField]
        [Tooltip("Location of head pose in byte array.")]
        int m_HeadPoseOffset;

        [SerializeField]
        [Tooltip("Location of camera pose in byte array.")]
        int m_CameraPoseOffset;

        [SerializeField]
        [Tooltip("Location of frame number value in byte array.")]
        int m_FrameNumberOffset;

        [SerializeField]
        [Tooltip("Location of frame time value in byte array.")]
        int m_FrameTimeOffset;

        [SerializeField]
        [Tooltip("Total size of buffer of byte array for single same of data.")]
        int m_BufferSize;

        [SerializeField]
        [Tooltip("The identifying strings of the blend shape locations.")]
        string[] m_Locations;

        [SerializeField]
        int m_InputStateOffset;
        
        [SerializeField]
        int m_InputScreenPositionSize;
        
        [SerializeField]
        int m_InputStateSize;
        
        [SerializeField]
        int m_InputScreenPositionOffset;
#pragma warning restore CS0649

        public byte ErrorCheck { get { return m_ErrorCheck; } }
        public int BlendShapeCount { get { return m_BlendShapeCount; } }
        public int BlendShapeSize { get { return m_BlendShapeSize; } }
        public int FrameNumberSize { get { return m_FrameNumberSize; } }
        public int FrameTimeSize { get { return m_FrameTimeSize; } }
        public int HeadPoseOffset { get { return m_HeadPoseOffset; } }
        public int CameraPoseOffset  { get { return m_CameraPoseOffset; } }
        public int FrameNumberOffset  { get { return m_FrameNumberOffset; } }
        public int FrameTimeOffset { get { return m_FrameTimeOffset;  } }

        // ARKit 2.0 buffer layout
        // 0 - Error check
        // 1-208 - Blend Shapes
        // 209-236 - Head Pose
        // 237-264 - Camera Pose
        // 265-268 - Frame Number
        // 269-273 - Frame Time
        // Input
        // n - 2 - Face tracking active state
        // n - 1 - Camera tracking active state
        public int bufferSize { get { return m_BufferSize; } }

        public string[] locations { get { return m_Locations; } }

        public int inputStateOffset
        {
            get { return m_InputStateOffset; }
        }

        public int inputStateSize
        {
            get { return m_InputStateSize; }
        }

        public int inputScreenPositionOffset
        {
            get { return m_InputScreenPositionOffset; }
        }

        public int inputScreenPositionSize
        {
            get { return m_InputScreenPositionSize; }
        }

        void OnValidate()
        {
            const int poseSize = BlendShapeUtils.PoseSize;
            m_BlendShapeSize = sizeof(float) * m_BlendShapeCount;
            m_FrameNumberSize = sizeof(int);
            m_FrameTimeSize = sizeof(float);
            m_HeadPoseOffset = BlendShapeSize + 1;
            m_CameraPoseOffset = HeadPoseOffset + poseSize;
            m_FrameNumberOffset = CameraPoseOffset + poseSize;
            m_FrameTimeOffset = FrameNumberOffset + FrameNumberSize;

            m_InputStateSize = sizeof(int);
            m_InputStateOffset = m_FrameTimeOffset + m_FrameTimeSize;

            m_InputScreenPositionSize = sizeof(float) * 2;
            m_InputScreenPositionOffset = m_InputStateOffset + m_InputStateSize;
            
            // Error check + Blend Shapes + HeadPose + CameraPose + FrameNumber + FrameTime + <INPUT> + Face Active + Camera Active
            m_BufferSize = 1 + BlendShapeSize + poseSize * 2 + FrameNumberSize + FrameTimeSize + m_InputStateSize 
                + m_InputScreenPositionSize + 1 + 1;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
