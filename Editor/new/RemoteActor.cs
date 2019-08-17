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
    public class RemoteActor
    {
        static readonly string k_DefaultDirectory = "Assets/";
        public delegate void RecordingStateCallback(RemoteActor actor);
        public static event RecordingStateCallback recordingStateChanged;

        [SerializeField]
        string m_Ip = "192.168.0.1";
        [SerializeField]
        int m_Port = 9000;
        [SerializeField]
        bool m_IsServer = false;
        NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
        AdapterSource m_AdapterSource = new AdapterSource();
        PacketStream m_PacketStream = new PacketStream();
        FaceDataRecorder m_Recoder = new FaceDataRecorder();
        TakePlayer m_Player = new TakePlayer();
        PreviewState m_PrevState = PreviewState.None;
        [SerializeField]
        BlendShapesController m_Controller;
        [SerializeField]
        AnimationClip m_Clip;
        [SerializeField]
        string m_Directory = k_DefaultDirectory;
        bool m_ChangingState = false;

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

        public BlendShapesController controller
        {
            get { return m_Controller; }
            set { m_Controller = value; }
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

        public RemoteActor()
        {
            m_PacketStream.reader.faceDataChanged += FaceDataChanged;
            m_PacketStream.reader.commandChanged += CommandChanged;
        }

        ~RemoteActor()
        {
            Dispose();
        }

        public void Dispose()
        {
            m_Recoder.StopRecording();
            m_Player.Stop();
            m_NetworkStreamSource.StopConnections();
            m_PacketStream.Stop();
            m_PacketStream.reader.faceDataChanged -= FaceDataChanged;
            m_PacketStream.reader.commandChanged -= CommandChanged;
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
                var animator = m_Controller.GetComponent<Animator>();
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
            
            //m_AdapterSource.streamSource = m_NetworkStreamSource;
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

            m_Recoder.StartRecording();

            recordingStateChanged(this);
        }

        public bool IsRecording()
        {
            return m_Recoder.isRecording;
        }

        public void StopRecording()
        {
            if (!m_Recoder.isRecording)
                return;
            
            m_Recoder.StopRecording();

            recordingStateChanged(this);
        }

        public void WriteRecording(Stream stream)
        {
            using (var writer = new ARStreamWriter(stream))
            {
                writer.Write(m_Recoder);
            }
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
                m_Player.Play(controller.GetComponent<Animator>(), m_Clip);
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

        void FaceDataChanged(FaceData data)
        {
            if (m_Controller == null)
                return;

            if (state != PreviewState.LiveStream)
                return;

            m_Controller.blendShapeInput = data.blendShapeValues;
            m_Controller.UpdateBlendShapes();

            if (m_Recoder.isRecording)
                m_Recoder.Record(data);
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
