using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public enum PreviewState
    {
        None,
        Playback,
        LiveStream
    }

    [Serializable]
    public abstract class ActorServer
    {
        static readonly string k_DefaultDirectory = "Assets/";
        public static event Action<ActorServer> recordingStateChanged;

        [SerializeField]
        string m_Ip = "192.168.0.1";
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        bool m_IsServer = false;
        HashSet<IPacketRecorder> m_Recorders = new HashSet<IPacketRecorder>();
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        PacketStream m_PacketStream = new PacketStream();
        TakePlayer m_Player = new TakePlayer();
        PreviewState m_PrevState = PreviewState.None;
        [SerializeField]
        Actor m_Actor;
        [SerializeField]
        AnimationClip m_Clip;
        [SerializeField]
        string m_Directory = k_DefaultDirectory;
        bool m_ChangingState = false;
        bool m_IsRecording = false;

        public string ip
        {
            get { return m_Ip; }
            set { m_Ip = value; }
        }

        public int port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public bool isServer
        {
            get { return m_IsServer; }
            set { m_IsServer = value; }
        }

        public TakePlayer player
        {
            get { return m_Player; }
        }

        public PreviewState state
        {
            get { return m_PrevState; }
        }

        public Actor actor
        {
            get { return m_Actor; }
            set { m_Actor = value; }
        }

        public AnimationClip clip
        {
            get { return m_Clip; }
            set { m_Clip = value; }
        }

        public string directory
        {
            get { return m_Directory; }
            set { m_Directory = value; }
        }

        protected PacketReader GetReader()
        {
            return m_PacketStream.reader;
        }

        protected void AddRecorder(IPacketRecorder recorder)
        {
            m_Recorders.Add(recorder);
        }

        public ActorServer()
        {
            m_PacketStream.reader.commandChanged += CommandChanged;
        }

        ~ActorServer()
        {
            Dispose();
        }

        public void Dispose()
        {
            StopRecorders();

            m_Player.Stop();
            m_NetworkStreamSource.StopConnections();
            m_PacketStream.Stop();
        }

        void StartRecorders()
        {
            foreach (var recorder in m_Recorders)
                recorder.StartRecording();
        }

        void StopRecorders()
        {
            foreach (var recorder in m_Recorders)
                recorder.StopRecording();
        }

        void SetPreviewState(PreviewState newState)
        {
            if (m_ChangingState)
                return;
            
            if (m_PrevState == newState)
                return;
            
            m_ChangingState = true;

            if (m_PrevState == PreviewState.LiveStream)
            {
                StopRecording();
            }
            else if (m_PrevState == PreviewState.Playback)
            {
                if (m_Player.isPlaying)
                    m_Player.Stop();
            }

            if (newState == PreviewState.LiveStream)
            {

            }
            else if (newState == PreviewState.Playback)
            {
                var animator = m_Actor.GetComponent<Animator>();
                m_Player.Play(animator, m_Clip);
            }

            m_PrevState = newState;
            m_ChangingState = false;
        }

        public void Connect()
        {
            isServer = true;
            
            if (isServer)
                m_NetworkStreamSource.StartServer(port);
            else
                m_NetworkStreamSource.ConnectToServer(ip, port);
            
            m_PacketStream.streamSource = m_NetworkStreamSource;
            m_PacketStream.Start();
            SetPreviewState(PreviewState.LiveStream);
        }

        public void Disconnect()
        {
            m_NetworkStreamSource.StopConnections();
            m_PacketStream.Stop();
            SetPreviewState(PreviewState.None);
        }

        public void Update()
        {
            m_PacketStream.reader.Receive();
            
            if (m_PrevState == PreviewState.Playback)
                m_Player.Update();    
        }

        public void StartRecording()
        {
            if (m_PrevState != PreviewState.LiveStream)
                return;

            StartRecorders();

            m_IsRecording = true;

            recordingStateChanged(this);
        }

        public bool IsRecording()
        {
            return m_IsRecording;
        }

        public void StopRecording()
        {
            if (!m_IsRecording)
                return;
            
            StopRecorders();

            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(directory + GenerateFileName());
            var path = uniqueAssetPath + ".arstream";

            using (var stream = File.Create(path))
            using (var writer = new ARStreamWriter(stream))
            {
                foreach (var recorder in m_Recorders)
                    writer.Write(recorder);
            }

            AssetDatabase.Refresh();

            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            EditorGUIUtility.PingObject(clip);

            m_IsRecording = false;
            
            recordingStateChanged(this);
        }

        string GenerateFileName()
        {
            return string.Format("{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
        }

        public void StartLiveStream()
        {
            SetPreviewState(PreviewState.LiveStream);
        }

        public void StopLiveStream()
        {
            if (m_PrevState != PreviewState.LiveStream)
                return;

            SetPreviewState(PreviewState.None);
        }

        public void StartPlayback()
        {
            if (state == PreviewState.Playback)
                m_Player.Play(actor.GetComponent<Animator>(), m_Clip);
            else
                SetPreviewState(PreviewState.Playback);
        }

        public void StopPlayback()
        {
            if (m_PrevState != PreviewState.Playback)
                return;
            
            SetPreviewState(PreviewState.None);
        }

        public void PausePlayback()
        {
            m_Player.Pause();
        }

        void CommandChanged(Command data)
        {
            switch (data.type)
            {
                case CommandType.StartRecording:
                    StartRecording();
                    break;
                case CommandType.StopRecording:
                    StopRecording();
                    break;
            }
        }
    }
}
