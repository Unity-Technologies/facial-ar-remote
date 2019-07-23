using System.Collections.Generic;
using UnityEngine;

namespace PerformanceRecorder.Takes
{
    [CreateAssetMenu(fileName = "Take System", menuName = "AR Face Capture/Take System", order = 1)]
    public class TakeSystem : ScriptableObject
    {
        [SerializeField]
        private List<TakeAsset> m_Assets = new List<TakeAsset>();

        public TakeAsset[] assets
        {
            get { return m_Assets.ToArray(); }
        }

        public TakeAsset Get(NodeID id)
        {
            return m_Assets != null ? m_Assets.Find(node => node.nodeID.Equals(id)) : null;
        }

        public void Add(TakeAsset node)
        {
            if (node == null)
                return;

            m_Assets.Add(node);
            node.takeSystem = this;
        }

        public void Remove(TakeAsset node)
        {
            if (node == null)
                return;
            
            m_Assets.Remove(node);
            node.takeSystem = null;
        }
    }
}
