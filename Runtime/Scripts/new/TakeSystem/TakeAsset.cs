using System;
using UnityEngine;

namespace PerformanceRecorder.Takes
{
    public abstract class TakeAsset : ScriptableObject
    {
        [SerializeField]
        public TakeSystem m_TakeSystem;
        [SerializeField]
        private NodeID m_NodeID = NodeID.empty;

        public TakeSystem takeSystem
        {
            get { return m_TakeSystem; }
            set { m_TakeSystem = value; }
        }

        public NodeID nodeID
        {
            get { return m_NodeID; }
        }

        public Vector2 m_Position;

        protected TakeAsset()
        {
            m_NodeID = NodeID.NewID();
        }
    }
}
