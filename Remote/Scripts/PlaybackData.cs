using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    [CreateAssetMenu(fileName = "PlaybackData", menuName = "FacialRemote/PlaybackData")]
    public class PlaybackData : ScriptableObject
    {
        [SerializeField]
        PlaybackBuffer[] m_PlaybackBuffers;

        public PlaybackBuffer[] playbackBuffers { get { return m_PlaybackBuffers; } }

        PlaybackBuffer m_ActiveBuffer;
        public List<byte[]> activeByteRecord { get; private set; }

        [SerializeField]
        [HideInInspector]
        byte[] m_LastRecord;

        void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EditorStateChange;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EditorStateChange;
#endif
        }

#if UNITY_EDITOR
        void EditorStateChange(PlayModeStateChange state)
        {
            byte errorCheck = 42;
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (m_ActiveBuffer != null && activeByteRecord != null)
                {
                    m_LastRecord = new byte[activeByteRecord.Count * m_ActiveBuffer.BufferSize];
                    var buffer = new byte[m_ActiveBuffer.BufferSize];
                    for (var i = 0; i < activeByteRecord.Count; i++)
                    {
                        buffer = activeByteRecord[i];
                        Buffer.BlockCopy(buffer, 0, m_LastRecord, i * m_ActiveBuffer.BufferSize, m_ActiveBuffer.BufferSize);
                        if (buffer[0] != errorCheck)
                            Debug.LogError(string.Format("Error in buffer {0}", i));
                    }
                      //  activeByteQueue.SelectMany(s => s).ToArray();
                    m_ActiveBuffer.recordStream = m_LastRecord.ToArray();
                    EditorUtility.SetDirty(this);

                    if (m_ActiveBuffer.recordStream.Length < 1 && m_PlaybackBuffers.Contains(m_ActiveBuffer))
                    {
                        m_PlaybackBuffers = m_PlaybackBuffers.Where(s => s != m_ActiveBuffer).ToArray();
                    }

                    m_ActiveBuffer = null;
                    activeByteRecord.Clear();
                }
            }
        }
#endif

        public void CreatePlaybackBuffer(IStreamSettings streamSettings)
        {
            var buffers = m_PlaybackBuffers.ToList();
            var buffer = new PlaybackBuffer(streamSettings);
            buffer.name = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            buffers.Add(buffer);
            m_PlaybackBuffers = buffers.ToArray();
            m_ActiveBuffer = m_PlaybackBuffers[m_PlaybackBuffers.Length-1];
            activeByteRecord = new List<byte[]>();
        }
    }
}
