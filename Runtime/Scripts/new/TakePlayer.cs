using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace PerformanceRecorder
{
    public class TakePlayer
    {
        PlayableGraph m_Graph;
        AnimationClipPlayable m_ClipPlayable;

        public TakePlayer()
        {
            
        }

        ~TakePlayer()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (m_Graph.IsValid())
                m_Graph.Destroy();
        }

        public void Play(Animator animator, AnimationClip clip)
        {
            Stop();
            m_ClipPlayable = AnimationPlayableUtilities.PlayClip(animator, clip, out m_Graph);
            //m_Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        public void Pause()
        {
            if (!m_Graph.IsValid())
                return;
            
            m_Graph.Stop();
        }

        public void Stop()
        {
            Destroy();
        }

        public void Update()
        {
            if (!m_Graph.IsValid())
                return;

            if (m_Graph.IsPlaying())
                m_Graph.Evaluate();
        }
    }
}
