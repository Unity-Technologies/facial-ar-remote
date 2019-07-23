using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;

namespace PerformanceRecorder.Takes
{
    public class TakeSystemWindow : EditorWindow, ISearchWindowProvider
    {
        public TakeGraphView graphView { get; private set; }
        private TakeSystem m_TakeSystem;
        private GraphViewCallbacks m_GraphViewCallbacks = new GraphViewCallbacks();
        private StackNode m_InsertStack;
        private int m_InsertIndex;

        [MenuItem("Window/Take System")]
        public static void ShowWindow()
        {
            GetWindow<TakeSystemWindow>("Take System");
        }

        void OnEnable()
        {
            graphView = new TakeGraphView(this);
            graphView.name = "Take System";
            graphView.persistenceKey = "TakeSystemGraphView";
            graphView.StretchToParentSize();

            this.GetRootVisualContainer().Add(graphView);

            graphView.nodeCreationRequest += OnRequestNodeCreation;

            Reload();
            SelectionChanged();

            Undo.undoRedoPerformed += UndoRedoPerformed;
            Selection.selectionChanged += SelectionChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Selection.selectionChanged -= SelectionChanged;
        }

        void UndoRedoPerformed()
        {
            Reload();
        }

        void SelectionChanged()
        {
            var selectedTakeSystem = Selection.activeObject as TakeSystem;

            if (selectedTakeSystem == null)
            {
                if (m_TakeSystem == null)
                    Reload();
                
                return;
            }

            if (selectedTakeSystem != m_TakeSystem)
            {
                m_TakeSystem = selectedTakeSystem;
                Reload();
            }
        }

        public void Reload()
        {
            if (m_TakeSystem == null)
                return;

            m_GraphViewCallbacks.Unregister();

            graphView.Reload(m_TakeSystem.assets);

            m_GraphViewCallbacks.Register(graphView);

            // Add the minimap.
            /*
            var miniMap = new MiniMap();
            miniMap.SetPosition(new Rect(0, 372, 200, 176));
            graphView.Add(miniMap);
            */
        }


        public TakeAsset CreateTakeAsset(Type type)
        {
            var undoName = "Create Asset";

            var asset = ScriptableObject.CreateInstance(type) as TakeAsset;
            Undo.RegisterCreatedObjectUndo(asset, undoName);

            Undo.RegisterCompleteObjectUndo(m_TakeSystem, undoName);
            m_TakeSystem.Add(asset);

            AssetDatabase.AddObjectToAsset(asset, m_TakeSystem);

            return asset;
        }

        public void DestroyTakeAsset(TakeAsset node)
        {
            var undoName = "Destroy Asset";

            Undo.RegisterCompleteObjectUndo(m_TakeSystem, undoName);

            Debug.Assert(node != null);

            m_TakeSystem.Remove(node);
            //node.groupNode = null;
            Undo.DestroyObjectImmediate(node);
        }

        public TakeNode CreateNode(TakeAsset node)
        {
            if (node is TakeDevice)
            {
                TakeDevice device = node as TakeDevice;

                return CreateDeviceNode(device, "Device", device.position);
            }
            else if (node is TakeActor)
            {
                TakeActor actor = node as TakeActor;

                return CreateActorNode(actor, "Actor", actor.position);
            }

            return null;
        }

        private TakeNode CreateDeviceNode(TakeAsset asset, string title, Vector2 pos)
        {
            TakeNode node = new TakeNode();
            node.userData = asset;
            node.persistenceKey = asset.nodeID.ToString();

            var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(FaceData));
            outputPort.userData = asset;
            node.outputContainer.Add(outputPort);

            node.SetPosition(new Rect(pos.x, pos.y, 100, 100));
            node.title = title;
            node.RefreshPorts();
            node.visible = true;

            return node;
        }

        private TakeNode CreateActorNode(TakeActor actor, string title, Vector2 pos)
        {
            TakeNode node = new TakeNode();
            node.userData = actor;
            node.persistenceKey = actor.nodeID.ToString();

            var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(FaceData));
            inputPort.userData = actor;
            node.inputContainer.Add(inputPort);

            var prefabField = new ObjectField()
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false
            };
            prefabField.OnValueChanged((ev) => 
            {
                Undo.RegisterCompleteObjectUndo(actor, "Inspector");

                actor.prefab = ev.newValue as GameObject;
                EditorUtility.SetDirty(actor);
            });

            node.inputContainer.Add(prefabField);

            node.SetPosition(new Rect(pos.x, pos.y, 100, 100));
            node.title = title;
            node.RefreshPorts();
            node.visible = true;

            return node;
        }

        protected void OnRequestNodeCreation(NodeCreationContext context)
        {
            m_InsertStack = context.target as StackNode;
            m_InsertIndex = context.index;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
        }

        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();

            //Texture2D icon = EditorGUIUtility.LoadIconRequired("cs Script Icon");
            var icon = Texture2D.whiteTexture;

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));
            tree.Add(new SearchTreeEntry(new GUIContent("Device", icon)) { level = 1, userData = typeof(TakeDevice) });
            tree.Add(new SearchTreeEntry(new GUIContent("Actor", icon)) { level = 1, userData = typeof(TakeActor) });

            return tree;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (!(entry is SearchTreeGroupEntry))
            {
                var asset = CreateTakeAsset(entry.userData as Type);
                Node nodeUI = CreateNode(asset) as Node;
                if (nodeUI != null)
                {
                    if (m_InsertStack != null)
                    {
                        /*
                        MathStackNode stackNode = m_InsertStack.userData as MathStackNode;

                        stackNode.InsertNode(m_InsertIndex, node);
                        m_InsertStack.InsertElement(m_InsertIndex, nodeUI);
                        */
                    }
                    else
                    {
                        graphView.AddElement(nodeUI);

                        Vector2 pointInWindow = context.screenMousePosition - position.position;
                        Vector2 pointInGraph = nodeUI.parent.WorldToLocal(pointInWindow);

                        nodeUI.SetPosition(new Rect(pointInGraph, Vector2.zero)); // it's ok to pass zero here because width/height is dynamic
                    }
                    nodeUI.Select(graphView, false);
                }
                else
                {
                    Debug.LogError("Failed to create element for " + asset);
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
