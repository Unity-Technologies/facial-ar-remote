using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [InitializeOnLoad]
    public static class EditorCallbacks
    {
        static EditorCallbacks()
        {
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload += OnAssemblyReload;
        }

        static void OnAssemblyReload()
        {
            var blendShapeControllers = UnityEngine.Object.FindObjectsOfType<BlendShapesController>();

            foreach (var blendShapeController in blendShapeControllers)
                blendShapeController.UpdateBlendShapeIndices();
        }
    }
}
