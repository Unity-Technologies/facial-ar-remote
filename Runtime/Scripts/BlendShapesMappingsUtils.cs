using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public static class BlendShapesMappingsUtils
    {
        public static void Prepare(Transform transform, BlendShapeMappings mappings, ref List<SkinnedMeshRenderer> renderers, ref BlendShapeMap[] maps)
        {
            renderers.Clear();
            maps = null;

            if (mappings != null)
            {
                maps = mappings.maps;

                foreach (var map in maps)
                    renderers.Add(transform.GetComponent<SkinnedMeshRenderer>(map.path));
            }
        }
        
        public static T GetComponent<T>(this Transform transform, string path)
        {
            var target = transform.Find(path);

            if (target == null)
                return default(T);

            return target.GetComponent<T>();
        }
    }
}
