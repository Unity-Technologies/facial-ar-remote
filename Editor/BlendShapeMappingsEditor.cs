using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [CustomEditor(typeof(BlendShapeMappings))]
    public class BlendShapeMappingsEditor : Editor
    {
        SerializedProperty m_PrefabProp;
        SerializedProperty m_MapsProp;
        List<string> m_LocationNames = new List<string>();

        void OnEnable()
        {
            m_PrefabProp = serializedObject.FindProperty("m_Prefab");
            m_MapsProp = serializedObject.FindProperty("m_Maps");

            PrepareLocationNames();
        }

        void PrepareLocationNames()
        {
            m_LocationNames.Clear();

            for (var i = 0; i < 52; ++i)
                m_LocationNames.Add(((BlendShapeLocation)i).ToString());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_PrefabProp);

                if (GUILayout.Button("Build", EditorStyles.miniButton, GUILayout.Width(35f)))
                {
                    Build();
                }
            }

            for (var i = 0; i < m_MapsProp.arraySize; ++i)
                DoMapGUI(m_MapsProp.GetArrayElementAtIndex(i));

            serializedObject.ApplyModifiedProperties();
        }

        void DoMapGUI(SerializedProperty mapProp)
        {
            var prefab = m_PrefabProp.objectReferenceValue as GameObject;

            if (prefab == null)
                return;

            var pathProp = mapProp.FindPropertyRelative("m_Path");
            var locationsProp = mapProp.FindPropertyRelative("m_Locations");
            var indicesProp = mapProp.FindPropertyRelative("m_Indices");

            var targetTransform = prefab.transform.Find(pathProp.stringValue);

            if (targetTransform == null)
                return;

            var skinnedMeshRenderer = targetTransform.GetComponent<SkinnedMeshRenderer>();

            if (skinnedMeshRenderer == null)
                return;

            var blendShapeNames = new List<string>(53);
            blendShapeNames.Add("None");
            blendShapeNames.AddRange(GetBlendShapeNames(skinnedMeshRenderer.sharedMesh));

            EditorGUILayout.LabelField(pathProp.stringValue, EditorStyles.boldLabel);

            ++EditorGUI.indentLevel;

            for (var i = 0; i < locationsProp.arraySize; ++i)
            {
                var locationIndex = locationsProp.GetArrayElementAtIndex(i).enumValueIndex;
                var index = indicesProp.GetArrayElementAtIndex(i).intValue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    index = EditorGUILayout.Popup(new GUIContent(m_LocationNames[locationIndex]), index + 1, blendShapeNames.ToArray()) - 1;
                    indicesProp.GetArrayElementAtIndex(i).intValue = index;
                }
            }

            --EditorGUI.indentLevel;
        }

        void Build()
        {
            var prefab = m_PrefabProp.objectReferenceValue as GameObject;

            if (prefab == null)
                return;

            var skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where( s => s.sharedMesh != null && s.sharedMesh.blendShapeCount > 0 )
                .ToArray();

            m_MapsProp.arraySize = skinnedMeshRenderers.Length;

            for (var i = 0; i < skinnedMeshRenderers.Length; ++i)
            {
                var mapProp = m_MapsProp.GetArrayElementAtIndex(i);
                var skinnedMeshRenderer = skinnedMeshRenderers[i];
                var path = AnimationUtility.CalculateTransformPath(skinnedMeshRenderer.transform, prefab.transform);

                mapProp.FindPropertyRelative("m_Path").stringValue = path;

                BuildFromMesh(mapProp, skinnedMeshRenderer.sharedMesh);
            }
        }

        void BuildFromMesh(SerializedProperty mapProp, Mesh mesh)
        {
            Debug.Assert(mesh != null);

            var locationsProp = mapProp.FindPropertyRelative("m_Locations");
            var indicesProp = mapProp.FindPropertyRelative("m_Indices");
            var blendShapeNames = GetBlendShapeNames(mesh);

            PrepareNames(blendShapeNames);

            locationsProp.arraySize = m_LocationNames.Count;
            indicesProp.arraySize = m_LocationNames.Count;

            for (var i = 0; i < m_LocationNames.Count; ++i)
            {
                locationsProp.GetArrayElementAtIndex(i).enumValueIndex = i;

                var match = default(string);
                var matchIndex = -1;
                
                if (FindMatch(m_LocationNames[i], blendShapeNames.ToArray(), out match))
                    matchIndex = blendShapeNames.IndexOf(match);

                indicesProp.GetArrayElementAtIndex(i).intValue = matchIndex;
            }
        }

        List<string> GetBlendShapeNames(Mesh mesh)
        {
            var names = new List<string>(mesh.blendShapeCount);
            
            if (mesh == null)
                return names;

            for (var i = 0; i < mesh.blendShapeCount; ++i)
                names.Add(mesh.GetBlendShapeName(i));

            return names;
        }

        void PrepareNames(List<string> names)
        {
            for (var i = 0; i < names.Count; ++i)
            {
                var name = names[i];
                var startIndex = name.LastIndexOf('.');
                names[i] = RemoveSpecialCharacters(name.Substring(startIndex + 1).ToLower());
            }
        }

        string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();
            foreach (char c in str) {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        bool FindMatch(string str, string[] otherArray, out string match)
        {
            Debug.Assert(otherArray.Length > 0);

            var others = new List<string>(otherArray);
            var lowerStr = str.ToLower();

            others.Sort( (s1, s2) =>
            {

                var first = LevenshteinDistance.Compute(lowerStr, s1);
                var second = LevenshteinDistance.Compute(lowerStr, s2);

                return first.CompareTo(second);
            });

            match = others[0];

            var distance = LevenshteinDistance.Compute(lowerStr, match);
            var percentage = (1f - ((float)distance / (float)Mathf.Max(match.Length, lowerStr.Length)));

            return percentage >= 0.6f;
        }
    }
}
