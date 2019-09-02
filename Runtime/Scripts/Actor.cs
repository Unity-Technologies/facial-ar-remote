using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [ExecuteAlways]
    public abstract class Actor : MonoBehaviour
    {
#if UNITY_EDITOR
        public static event Action<Actor> actorEnabled;
        public static event Action<Actor> actorDisabled;

        protected virtual void OnEnable()
        {
            actorEnabled.Invoke(this);
        }

        protected virtual void OnDisable()
        {
            actorDisabled.Invoke(this);
        }
#else
        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }
#endif
    }
}
