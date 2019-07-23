using System;
using UnityEngine;

namespace PerformanceRecorder.Takes
{
    public class TakeActor : TakeAsset
    {
        [SerializeField]
        GameObject m_Prefab;

        public GameObject prefab
        {
            get { return m_Prefab; }
            set { m_Prefab = value; }
        }
    }
}
