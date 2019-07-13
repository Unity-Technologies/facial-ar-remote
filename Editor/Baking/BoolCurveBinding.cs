using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class BoolCurveBinding
    {
        AnimationCurve m_Curve;
        EditorCurveBinding m_Binding;

        public BoolCurveBinding(string path, Type type, string propertyName)
        {
            m_Binding = EditorCurveBinding.DiscreteCurve(path, type, propertyName);
            m_Curve = new AnimationCurve();
        }

        public void AddKey(float time, bool value)
        {
            m_Curve.AddKey(time, value ? 1f : 0);
        }

        public void SetCurves(AnimationClip clip)
        {
            AnimationUtility.SetEditorCurve(clip, m_Binding, m_Curve);
        }
    }
}
