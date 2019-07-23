using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace PerformanceRecorder.Takes
{
    internal static class NodeAdapterExtensions
    {
        internal static bool Adapt(this NodeAdapter value, PortSource<FaceData> a, PortSource<FaceData> b)
        {
            return true;
        }
    }
}
