using System;
using UnityEngine;
using PerformanceRecorder;

public class TestComponent : MonoBehaviour
{
    [SerializeField]
    AssetStreamSource m_AssetStreamSource = new AssetStreamSource();
    [SerializeField]
    NetworkStreamSource m_NetworkStreamSource = new NetworkStreamSource();
    [SerializeField]
    StreamReader m_StreamReader = new StreamReader();
    int m_TakeCount = 0;

    void OnEnable()
    {
        m_NetworkStreamSource.StartServer();
        m_StreamReader.streamSource = m_NetworkStreamSource;
        m_StreamReader.StartLiveStream();
    }

    void OnDisable()
    {
        m_StreamReader.Dispose();
        m_NetworkStreamSource.StopConnections();
    }

    [ContextMenu("Start Recording")]
    public void StartRecording()
    {
        m_StreamReader.StartRecording();
    }

    [ContextMenu("Stop Recording")]
    public void StopRecording()
    {
        m_StreamReader.StopRecording();
        m_StreamReader.SaveRecording("Assets/" + GenerateFileName() +".arstream");
    }

    string GenerateFileName()
    {
        return string.Format("{0:yyyy_MM_dd_HH_mm}-Take{1:00}", DateTime.Now, ++m_TakeCount);
    }
}
