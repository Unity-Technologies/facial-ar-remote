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

        public List<byte[]> activeByteRecord
        {
            get
            {
                return m_ActiveRecords.Count > 0 ? m_ActiveRecords.Last() : null;
            }
        }

//        List<byte[]> m_ActiveByteRecord;

        Dictionary<PlaybackBuffer, List<byte[]>> m_RecordTakes = new Dictionary<PlaybackBuffer, List<byte[]>>();
        List<List<byte[]>> m_ActiveRecords = new List<List<byte[]>>();

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
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (m_RecordTakes.Count == 0)
                    return;

                var newBuffers = new List<PlaybackBuffer>();
                foreach (var take in m_RecordTakes)
                {
                    var playbackBuffer = take.Key;
                    var byteRecord = take.Value;
                    var record = new byte[byteRecord.Count * playbackBuffer.BufferSize];
                    for (var i = 0; i < byteRecord.Count; i++)
                    {
                        var buffer = byteRecord[i];
                        Buffer.BlockCopy(buffer, 0, record, i * playbackBuffer.BufferSize, playbackBuffer.BufferSize);
                        if (buffer[0] != playbackBuffer.ErrorCheck)
                            Debug.LogError(string.Format("Error in buffer {0}", i));
                    }
                    playbackBuffer.recordStream = record.ToArray();

                    if (playbackBuffer.recordStream.Length > 0)
                    {
                        newBuffers.Add(playbackBuffer);
                    }
                }

                m_PlaybackBuffers = newBuffers.Concat(m_PlaybackBuffers).Where(w => w.recordStream.Length > 0).ToArray();
                m_RecordTakes.Clear();
                m_ActiveRecords.Clear();
                EditorUtility.SetDirty(this);
            }
        }
#endif

        public void CreatePlaybackBuffer(IStreamSettings streamSettings, int take)
        {
            var playbackBuffer = new PlaybackBuffer(streamSettings);
            playbackBuffer.name = String.Format("{0:yyyy_MM_dd_HH_mm}-Take{1:00}", DateTime.Now, take);
            Debug.Log(string.Format("Starting take: {0}", playbackBuffer.name));
            var byteRecord = new List<byte[]>();
            m_RecordTakes.Add(playbackBuffer, byteRecord);
            m_ActiveRecords.Add(byteRecord);
        }
    }
}
