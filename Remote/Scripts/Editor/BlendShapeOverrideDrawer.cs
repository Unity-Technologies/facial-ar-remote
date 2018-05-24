using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote.Unity.Labs.FacialRemote
{
    [CustomPropertyDrawer(typeof(BlendShapeOverride))]
    public class BlendShapeOverrideInspector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                    new GUIContent(property.FindPropertyRelative("m_Name").stringValue));

                var lineHeight = EditorGUIUtility.singleLineHeight;

                var overrideRect = new Rect(position.x, position.y, position.width, lineHeight);
                var smoothingRect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);
                var thresholdRect = new Rect(position.x, position.y + lineHeight * 2, position.width, lineHeight);
                var coefficientRect = new Rect(position.x, position.y + lineHeight * 3, position.width, lineHeight);
                var maxRect = new Rect(position.x, position.y + lineHeight * 4, position.width, lineHeight);

                var useOverride = property.FindPropertyRelative("m_UseOverride");
                var overrideLabel = new GUIContent(useOverride.displayName);
                EditorGUI.PropertyField(overrideRect, useOverride, overrideLabel);

                var guiEnabled = GUI.enabled;

                if (guiEnabled)
                    GUI.enabled = useOverride.boolValue;

                var blendShapeSmoothing = property.FindPropertyRelative("m_BlendShapeSmoothing");
                var blendShapeSmoothingLabel = new GUIContent(blendShapeSmoothing.displayName);
                EditorGUI.PropertyField(smoothingRect, blendShapeSmoothing, blendShapeSmoothingLabel);

                var threshold = property.FindPropertyRelative("m_BlendShapeThreshold");
                var thresholdLabel = new GUIContent(threshold.displayName);
                EditorGUI.PropertyField(thresholdRect, threshold, thresholdLabel);

                var coefficient = property.FindPropertyRelative("m_BlendShapeCoefficient");
                var coefficientLabel = new GUIContent(coefficient.displayName);
                EditorGUI.PropertyField(coefficientRect, coefficient, coefficientLabel);

                var max = property.FindPropertyRelative("m_BlendShapeMax");
                var maxLabel = new GUIContent(max.displayName);
                EditorGUI.PropertyField(maxRect, max, maxLabel);

                GUI.enabled = guiEnabled;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 5 + 2;
        }
    }
}
