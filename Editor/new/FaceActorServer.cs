using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Labs.FacialRemote;

namespace PerformanceRecorder
{
    public class FaceActorServer : ActorServer
    {
        FaceDataRecorder m_FaceDataRecoder = new FaceDataRecorder();

        public FaceActorServer() : base()
        {
            AddRecorder(m_FaceDataRecoder);
            
            GetReader().faceDataChanged += FaceDataChanged;
        }

        void FaceDataChanged(FaceData data)
        {
            var blendShapeController = actor as BlendShapesController;

            if (blendShapeController == null)
                return;

            if (state != PreviewState.LiveStream)
                return;

            blendShapeController.blendShapeInput = data.blendShapeValues;
            blendShapeController.UpdateBlendShapes();

            if (m_FaceDataRecoder.isRecording)
                m_FaceDataRecoder.Record(data);
        }
    }
}
