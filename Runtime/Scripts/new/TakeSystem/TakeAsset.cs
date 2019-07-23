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
        [SerializeField]
        private Vector2 m_Position;

        public TakeSystem takeSystem
        {
            get { return m_TakeSystem; }
            internal set { m_TakeSystem = value; }
        }

        public NodeID nodeID
        {
            get { return m_NodeID; }
        }

        public Vector2 position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        protected TakeAsset()
        {
            m_NodeID = NodeID.NewID();
        }
    }
}
