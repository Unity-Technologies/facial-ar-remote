using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class QuaternionCurveBinding
    {
        const int kCurveCount = 4;
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public QuaternionCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[kCurveCount]
            {
                propertyName + ".x",
                propertyName + ".y",
                propertyName + ".z",
                propertyName + ".w"
            };
            m_Curves = new AnimationCurve[kCurveCount];

            for (var i = 0; i < kCurveCount; ++i)
                m_Curves[i] = new AnimationCurve();
        }

        public void AddKey(float time, Quaternion value)
        {
            for (var i = 0; i < kCurveCount; ++i)
            {
                var curve = m_Curves[i];
                curve.AddKey(time, value[i]);
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < kCurveCount; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }
}
