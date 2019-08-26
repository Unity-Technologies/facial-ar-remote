using System;
using UnityEngine;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public class BlendShapesCurveBinding
    {
        const int kBlendShapeCount = 52;
        string[] m_PropertyNames;
        AnimationCurve[] m_Curves;
        string m_Path;
        Type m_Type;

        public BlendShapesCurveBinding(string path, Type type, string propertyName)
        {
            m_Path = path;
            m_Type = type;
            m_PropertyNames = new string[kBlendShapeCount];
            m_Curves = new AnimationCurve[kBlendShapeCount];

            for (var i = 0; i < kBlendShapeCount; ++i)
            {
                m_PropertyNames[i] = propertyName + "." + ((BlendShapeLocation)i).ToString();
                m_Curves[i] = new AnimationCurve();
            }
        }

        public void AddKey(float time, ref BlendShapeValues value, bool stepped = false)
        {
            for (var i = 0; i < kBlendShapeCount; ++i)
            {
                var curve = m_Curves[i];
                var keyframe = new Keyframe
                {
                    time = time,
                    value = value[i]
                };

                if (stepped)
                {
                    keyframe.outTangent = Mathf.Infinity;
                    keyframe.inTangent = Mathf.Infinity;
                }

                curve.AddKey(keyframe);

                if (stepped)
                {
                    AnimationUtility.SetKeyBroken(curve, curve.length - 1, true);
                    AnimationUtility.SetKeyRightTangentMode(curve, curve.length - 1, AnimationUtility.TangentMode.Constant);
                    AnimationUtility.SetKeyRightTangentMode(curve, curve.length - 1, AnimationUtility.TangentMode.Constant);
                }
            }
        }

        public void SetCurves(AnimationClip clip)
        {
            for (var i = 0; i < kBlendShapeCount; ++i)
                clip.SetCurve(m_Path, m_Type, m_PropertyNames[i], m_Curves[i]);
        }
    }
}
