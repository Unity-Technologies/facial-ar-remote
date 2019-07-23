using System;
using UnityEngine;

namespace PerformanceRecorder.Takes
{
    [Serializable]
    public struct NodeID : IEquatable<NodeID>
    {
        [SerializeField]
        private string m_NodeGuid;

        private static readonly NodeID s_Empty = new NodeID { m_NodeGuid = "" };

        public TakeAsset Get(TakeSystem takeSystem)
        {
            if (takeSystem == null)
                return null;
            return takeSystem.Get(this);
        }

        public void Set(TakeAsset node)
        {
            m_NodeGuid = node == null ? null : node.nodeID.m_NodeGuid;
        }

        public NodeID(string guid)
        {
            m_NodeGuid = guid;
        }

        public static NodeID empty { get { return s_Empty; } }

        static public NodeID NewID()
        {
            return new NodeID { m_NodeGuid = Guid.NewGuid().ToString() };
        }

        public bool Equals(NodeID other)
        {
            if (ReferenceEquals(this, other)) return true;
            return m_NodeGuid.Equals(other.m_NodeGuid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeID)obj);
        }

        public static bool operator!=(NodeID lhs, NodeID rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator==(NodeID lhs, NodeID rhs)
        {
            return lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            return m_NodeGuid.GetHashCode();
        }

        public override string ToString()
        {
            return m_NodeGuid;
        }
    }
}
