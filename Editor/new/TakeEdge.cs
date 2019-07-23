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
    public class TakeEdge : Edge
    {
        public TakeEdge()
        {
            clippingOptions = VisualElement.ClippingOptions.NoClipping;
        }
    }
}
