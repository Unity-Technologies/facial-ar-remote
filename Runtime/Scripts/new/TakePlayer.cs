using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace PerformanceRecorder
{
    public class TakePlayer
    {
        AnimationClip m_Clip;
        PlayableGraph m_Graph;
        AnimationClipPlayable m_ClipPlayable;

        public bool isPlaying
        {
            get
            {
                if (m_Graph.IsValid())
                    return m_Graph.IsPlaying();

                return false;
            }
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
            if (animator == null)
                throw new NullReferenceException("Animator is null");

            if (m_Clip != clip)
            {
                Stop();
                m_Clip = clip;
            }
            if (m_Graph.IsValid())
            {
                m_Graph.Play();

                //EditMode needs a little kick in order to resume properly. Works fine in PlayMode.
                if (!Application.isPlaying)
                {
                    animator.enabled = false;
                    animator.enabled = true;
                }
            }
            else if (m_Clip != null)
            {
                m_ClipPlayable = AnimationPlayableUtilities.PlayClip(animator, m_Clip, out m_Graph);
            }
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
