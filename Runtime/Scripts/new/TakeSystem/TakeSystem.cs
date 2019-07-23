using System.Collections.Generic;
using UnityEngine;

namespace PerformanceRecorder.Takes
{
    [CreateAssetMenu(fileName = "Take System", menuName = "AR Face Capture/Take System", order = 1)]
    public class TakeSystem : ScriptableObject
    {
        [SerializeField]
        private List<TakeAsset> m_Nodes = new List<TakeAsset>();

        public List<TakeAsset> nodes
        {
            get { return m_Nodes; }
        }

        public TakeAsset Get(NodeID id)
        {
            return m_Nodes != null ? m_Nodes.Find(node => node.nodeID.Equals(id)) : null;
        }

        public void Add(TakeAsset node)
        {
            if (node == null)
                return;

            m_Nodes.Add(node);
            node.takeSystem = this;
        }

        public void Remove(TakeAsset node)
        {
            if (node == null)
                return;
            
            m_Nodes.Remove(node);
            node.takeSystem = null;
        }
    }
}
