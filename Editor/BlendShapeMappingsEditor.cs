using System;
using System.IO;
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
        string[] m_LocationNames = {};

        void OnEnable()
        {
            m_PrefabProp = serializedObject.FindProperty("m_Prefab");
            m_MapsProp = serializedObject.FindProperty("m_Maps");

            PrepareLocationNames();
        }

        void PrepareLocationNames()
        {
            var names = new List<string>(53);

            for (var i = 0; i < 53; ++i)
                names.Add(((BlendShapeLocation)i).ToString());

            m_LocationNames = names.ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_PrefabProp);

                var prefab = m_PrefabProp.objectReferenceValue as GameObject;

                using (new EditorGUI.DisabledGroupScope(prefab == null))
                {
                    if (GUILayout.Button("Build", EditorStyles.miniButton, GUILayout.Width(35f)))
                    {
                        if (prefab != null)
                            Build(prefab);
                    }
                }
            }

            for (var i = 0; i < m_MapsProp.arraySize; ++i)
                DoMapGUI(m_MapsProp.GetArrayElementAtIndex(i));

            serializedObject.ApplyModifiedProperties();
        }

        void DoMapGUI(SerializedProperty mapProp)
        {
            var pathProp = mapProp.FindPropertyRelative("m_Path");
            var indicesProp = mapProp.FindPropertyRelative("m_Indices");
            var locationsProp = mapProp.FindPropertyRelative("m_Locations");
            var mesh = default(Mesh);

            var prefab = m_PrefabProp.objectReferenceValue as GameObject;
            if (prefab != null)
            {
                var targetTransform = prefab.transform.Find(pathProp.stringValue);

                if (targetTransform != null)
                {
                    var skinnedMeshRenderer = targetTransform.GetComponent<SkinnedMeshRenderer>();

                    if (skinnedMeshRenderer != null)
                        mesh = skinnedMeshRenderer.sharedMesh;
                }
            }

            var blendShapeNames = default(List<string>);

            if (mesh != null)
            {
                blendShapeNames = GetBlendShapeNames(mesh);
            }

            var transformName = Path.GetFileName(pathProp.stringValue);

            EditorGUILayout.LabelField(transformName, EditorStyles.boldLabel);

            ++EditorGUI.indentLevel;

            for (var i = 0; i < indicesProp.arraySize; ++i)
            {                
                var indexProp = indicesProp.GetArrayElementAtIndex(i);
                var label = default(GUIContent);

                if (blendShapeNames != null)
                    label = new GUIContent(blendShapeNames[indexProp.intValue]);
                else
                    label = new GUIContent("Index: " + indexProp.intValue.ToString());

                var locationProp = locationsProp.GetArrayElementAtIndex(i);
                locationProp.intValue = EditorGUILayout.Popup(label, locationProp.intValue, m_LocationNames);
            }

            --EditorGUI.indentLevel;
        }

        void Build(GameObject prefab)
        {
            Debug.Assert(prefab != null);

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
            var blendShapeNames = PrepareNames(GetBlendShapeNames(mesh)).ToArray();
            var locationNames = PrepareNames(new List<string>(m_LocationNames)).ToArray();

            indicesProp.arraySize = blendShapeNames.Length;
            locationsProp.arraySize = blendShapeNames.Length;

            for (var i = 0; i < blendShapeNames.Length; ++i)
            {
                indicesProp.GetArrayElementAtIndex(i).intValue = i;

                var match = default(string);
                var matchIndex = -1;
                
                if (FindMatch(blendShapeNames[i], locationNames, out match))
                    matchIndex = Array.IndexOf(locationNames, match);

                if (matchIndex == -1)
                    matchIndex = (int)BlendShapeLocation.Invalid;

                locationsProp.GetArrayElementAtIndex(i).enumValueIndex = matchIndex;
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

        List<string> PrepareNames(List<string> names)
        {
            names = new List<string>(names);
            
            for (var i = 0; i < names.Count; ++i)
            {
                var name = names[i];
                var startIndex = name.LastIndexOf('.');
                names[i] = RemoveSpecialCharacters(name.Substring(startIndex + 1).ToLower());
            }

            return names;
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

            others.Sort( (s1, s2) =>
            {

                var first = LevenshteinDistance.Compute(str, s1);
                var second = LevenshteinDistance.Compute(str, s2);

                return first.CompareTo(second);
            });

            match = others[0];

            var distance = LevenshteinDistance.Compute(str, match);
            var percentage = (1f - ((float)distance / (float)Mathf.Max(match.Length, str.Length)));

            return percentage >= 0.6f;
        }
    }
}
