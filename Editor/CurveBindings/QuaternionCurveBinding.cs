using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class QuaternionCurveBinding
    {
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public QuaternionCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[4]
            {
                propertyName + ".x",
                propertyName + ".y",
                propertyName + ".z",
                propertyName + ".w"
            };
            m_Curves = new AnimationCurve[4];

            for (var i = 0; i < 4; ++i)
                m_Curves[i] = new AnimationCurve();
        }

        public void AddKey(float time, Quaternion value)
        {
            for (var i = 0; i < 4; ++i)
            {
                var curve = m_Curves[i];
                curve.AddKey(time, value[i]);
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < 3; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }
}
