using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class FloatCurveBinding
    {
        string m_PropertyName;
        AnimationCurve m_Curve;
        string m_Path;
        Type m_Type;

        public FloatCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyName = propertyName;
            m_Curve = new AnimationCurve();
        }

        public void AddKey(float time, float value)
        {
            m_Curve.AddKey(time, value);
        }

        public void SetCurves(AnimationClip clip)
        {
            clip.SetCurve(m_Path, m_Type, m_PropertyName, m_Curve);
        }
    }
}
