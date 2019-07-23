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
    public class TakeGraphView : GraphView
    {
        TakeSystemWindow m_TakeSystemWindow;
        /*
        private static readonly Vector2 s_CopyOffset = new Vector2(50, 50);
        SimpleBlackboard m_Blackboard;

        public SimpleGraphViewWindow window { get { return m_SimpleGraphViewWindow; } }
        public SimpleBlackboard blackboard { get { return m_Blackboard; } }
        */

        public TakeGraphView(TakeSystemWindow takeSystemWindow)
        {
            m_TakeSystemWindow = takeSystemWindow;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // FIXME: add a coordinator so that ContentDragger and SelectionDragger cannot be active at the same time.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            Insert(0, new GridBackground());

            focusIndex = 0;

            /*
            serializeGraphElements = SerializeGraphElementsImplementation;
            canPasteSerializedData = CanPasteSerializedDataImplementation;
            unserializeAndPaste = UnserializeAndPasteImplementation;
            */

            /*
            m_Blackboard = new SimpleBlackboard(simpleGraphViewWindow.mathBook);

            this.Add(m_Blackboard);

            m_Blackboard.style.width = 200;
            */
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            /*
            if (evt.target == this || selection.FindAll(s => { VisualElement ve = s as VisualElement; return ve != null && ve.userData is MathNode; }).Contains(evt.target as ISelectable))
            {
                evt.menu.AppendAction("Group Selection", AddToGroupNode, (a) =>
                {
                    List<ISelectable> filteredSelection = new List<ISelectable>();

                    foreach (ISelectable selectedObject in selection)
                    {
                        VisualElement ve = selectedObject as VisualElement;

                        // Disabled if at least one group is selected or if the selected element is in a stack
                        if (ve.userData is MathGroupNode || ve.parent is StackNode)
                        {
                            return DropdownMenu.MenuAction.StatusFlags.Disabled;
                        }
                        else if (ve.userData is MathNode)
                        {
                            filteredSelection.Add(selectedObject);
                        }
                    }

                    if (filteredSelection.Count > 0)
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Normal;
                    }
                    else
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Disabled;
                    }
                });

                evt.menu.AppendAction("Ungroup Selection", RemoveFromGroupNode, (a) =>
                {
                    List<ISelectable> filteredSelection = new List<ISelectable>();

                    foreach (ISelectable selectedObject in selection)
                    {
                        VisualElement ve = selectedObject as VisualElement;

                        // Disabled if at least one group is selected
                        if (ve.userData is MathGroupNode)
                        {
                            return DropdownMenu.MenuAction.StatusFlags.Disabled;
                        }
                        else if (ve.userData is MathNode)
                        {
                            var selectedNode = selectedObject as Node;

                            if (selectedNode.GetContainingScope() is Group)
                            {
                                filteredSelection.Add(selectedObject);
                            }
                        }
                    }

                    if (filteredSelection.Count > 0)
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Normal;
                    }
                    else
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Disabled;
                    }
                });

                evt.menu.AppendAction("Scope Selection", AddToScope, (a) =>
                {
                    List<ISelectable> filteredSelection = new List<ISelectable>();

                    foreach (ISelectable selectedObject in selection)
                    {
                        GraphElement graphElement = selectedObject as GraphElement;

                        // Disabled if at least one group is selected
                        if (graphElement.userData is MathNode)
                        {
                            Scope scope = graphElement.GetContainingScope();

                            if (scope != null)
                            {
                                return DropdownMenu.MenuAction.StatusFlags.Disabled;
                            }
                            filteredSelection.Add(selectedObject);
                        }
                    }

                    if (filteredSelection.Count > 0)
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Normal;
                    }
                    else
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Disabled;
                    }
                });

                evt.menu.AppendAction("Unscope Selection", RemoveFromScope, (a) =>
                {
                    List<ISelectable> filteredSelection = new List<ISelectable>();

                    foreach (ISelectable selectedObject in selection)
                    {
                        GraphElement graphElement = selectedObject as GraphElement;

                        if (graphElement.userData is MathNode)
                        {
                            Scope scope = graphElement.GetContainingScope();

                            if (scope != null && !(scope is Group))
                            {
                                filteredSelection.Add(graphElement);
                            }
                        }
                    }

                    if (filteredSelection.Count > 0)
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Normal;
                    }
                    else
                    {
                        return DropdownMenu.MenuAction.StatusFlags.Disabled;
                    }
                });
                evt.menu.AppendSeparator();
            }

            if (evt.target == this)
            {
                evt.menu.AppendAction("Create Group", CreateGroupNode, DropdownMenu.MenuAction.AlwaysEnabled);
                evt.menu.AppendAction("Create Stack", CreateStackNode, DropdownMenu.MenuAction.AlwaysEnabled);
            }
            */

            base.BuildContextualMenu(evt);
        }

        const string m_SerializedDataMimeType = "application/vnd.unity.simplegraphview.elements";

        /*
        string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
        {
            List<MathNode> data = new List<MathNode>();
            foreach (var element in elements)
            {
                data.Add(element.userData as MathNode);
            }
            CopyPasteData<MathNode> copyPasteData = new CopyPasteData<MathNode>(data);
            return m_SerializedDataMimeType + " " + JsonUtility.ToJson(copyPasteData);
        }
        */

        /*
        bool CanPasteSerializedDataImplementation(string data)
        {
            if (data.StartsWith(m_SerializedDataMimeType))
            {
                return true;
            }
            return false;
        }
        */

        /*
        void UnserializeAndPasteImplementation(string operationName, string serializedData)
        {
            CopyPasteData<MathNode> data = JsonUtility.FromJson<CopyPasteData<MathNode>>(serializedData.Substring(m_SerializedDataMimeType.Length + 1));

            if (data != null && data.GetNodes().Count > 0)
            {
                var nodeIDMap = new Dictionary<MathNodeID, MathNodeID>();

                foreach (MathNode mathNode in data.GetNodes())
                {
                    MathNodeID oldID = mathNode.nodeID;

                    nodeIDMap[oldID] = mathNode.RewriteID();

                    m_SimpleGraphViewWindow.AddNode(mathNode);
                    mathNode.m_Position = mathNode.m_Position + s_CopyOffset;
                }

                // Remap ids
                foreach (MathNode mathNode in data.GetNodes())
                {
                    mathNode.RemapReferences(nodeIDMap);
                }

                Reload(data.GetNodes(), true);
            }
        }

        private void CreateGroupNode(DropdownMenu.MenuAction a)
        {
            Vector2 pos = a.eventInfo.localMousePosition;

            var groupNode = ScriptableObject.CreateInstance<MathGroupNode>();
            Vector2 localPos = VisualElementExtensions.ChangeCoordinatesTo(this, contentViewContainer, pos);

            groupNode.m_Title = "New Group";
            groupNode.m_Position = localPos;

            m_SimpleGraphViewWindow.AddNode(groupNode);

            GraphElement graphGroup = m_SimpleGraphViewWindow.CreateNode(groupNode);

            AddElement(graphGroup);
        }
        */

        private void AddToGroupNode(DropdownMenu.MenuAction a)
        {
            /*
            var groupNode = ScriptableObject.CreateInstance<MathGroupNode>();

            groupNode.m_Title = "New Group";

            m_SimpleGraphViewWindow.AddNode(groupNode);

            var graphGroup = m_SimpleGraphViewWindow.CreateNode(groupNode) as Group;

            AddElement(graphGroup);

            foreach (ISelectable s in selection)
            {
                var node = s as Node;

                // Do not add edges
                if (node == null)
                    continue;

                graphGroup.AddElement(node);
            }
            */
        }

        private void RemoveFromGroupNode(DropdownMenu.MenuAction a)
        {
            /*
            foreach (ISelectable s in selection)
            {
                var node = s as Node;

                if (node == null)
                    continue;

                Group group = node.GetContainingScope() as Group;

                if (group != null)
                {
                    group.RemoveElement(node);
                }
            }
            */
        }

        private void AddToScope(DropdownMenu.MenuAction a)
        {
            /*
            var scope = ScriptableObject.CreateInstance<MathGroupNode>();

            scope.m_IsScope = true;

            m_SimpleGraphViewWindow.AddNode(scope);

            var graphScope = m_SimpleGraphViewWindow.CreateNode(scope) as Scope;

            AddElement(graphScope);

            foreach (ISelectable s in selection)
            {
                var element = s as GraphElement;

                // Do not add edges
                if (element.userData is MathNode)
                {
                    MathNode mathNode = element.userData as MathNode;

                    mathNode.m_GroupNodeID = scope.nodeID;
                    graphScope.AddElement(element);
                }
            }
        }

        private void RemoveFromScope(DropdownMenu.MenuAction a)
        {
            foreach (ISelectable s in selection)
            {
                var element = s as GraphElement;

                if (element.userData is MathNode)
                {
                    Scope scope = element.GetContainingScope();

                    if (scope != null && !(scope is Group))
                    {
                        MathNode mathNode = element.userData as MathNode;

                        mathNode.m_GroupNodeID = MathNodeID.empty;
                        scope.RemoveElement(element);
                    }
                }
            }

            window.RemoveEmptyScopes();
            */
        }

        private void CreateStackNode(DropdownMenu.MenuAction a)
        {
            /*
            Vector2 pos = a.eventInfo.localMousePosition;

            var stackNode = ScriptableObject.CreateInstance<MathStackNode>();
            Vector2 localPos = VisualElementExtensions.ChangeCoordinatesTo(this, contentViewContainer, pos);

            stackNode.name = "Stack";
            stackNode.m_Position = localPos;

            m_SimpleGraphViewWindow.AddNode(stackNode);

            GraphElement graphStackNode = m_SimpleGraphViewWindow.CreateNode(stackNode);

            AddElement(graphStackNode);
            */
        }

        private void AddToStackNode(DropdownMenu.MenuAction a)
        {
            /*
            var stackNode = ScriptableObject.CreateInstance<MathStackNode>();

            m_SimpleGraphViewWindow.AddNode(stackNode);

            var graphStackNode = m_SimpleGraphViewWindow.CreateNode(stackNode) as StackNode;

            AddElement(graphStackNode);

            ISelectable[] selectedElement = selection.ToArray();

            foreach (ISelectable s in selectedElement)
            {
                var node = s as SimpleNode;

                // Do not add edges
                if (node == null)
                    continue;

                graphStackNode.AddElement(node);

                node.Select(this, true);
            }
            */
        }

        public void Reload(List<TakeAsset> nodesToReload, bool select = false)
        {
            var nodes = new Dictionary<TakeAsset, GraphElement>();

            if (select)
            {
                ClearSelection();
            }

            // Create the nodes.
            foreach (TakeAsset takeAsset in nodesToReload)
            {
                GraphElement node = m_TakeSystemWindow.CreateNode(takeAsset);

                if (node is Group)
                {
                    node.name = "SimpleGroup";
                }
                else if (node is Scope)
                {
                    node.name = "SimpleScope";
                }
                else
                {
                    node.name = "TakeNode";
                }

                if (node == null)
                {
                    Debug.LogError("Could not create node " + takeAsset);
                    continue;
                }

                nodes[takeAsset] = node;

                AddElement(node);

                // Do not select node in groups as their containing group will be selected
                /*
                if (select && takeAsset.groupNode == null)
                {
                    AddToSelection(node);
                }
                */
            }

            // Assign scopes
            /*
            foreach (TakeAsset takeAsset in nodesToReload)
            {
                if (takeAsset.groupNode == null)
                    continue;

                Scope graphScope = nodes[takeAsset.groupNode] as Scope;

                graphScope.AddElement(nodes[takeAsset]);
            }
            */

            // Add to stacks
            /*
            foreach (TakeAsset takeAsset in nodesToReload)
            {
                MathStackNode stack = takeAsset as MathStackNode;

                if (stack == null)
                    continue;

                StackNode graphStackNode = nodes[stack] as StackNode;

                for (int i = 0; i < stack.nodeCount; ++i)
                {
                    TakeAsset stackMember = stack.GetNode(i);
                    if (stackMember == null)
                    {
                        Debug.LogWarning("null stack member! Item " + i + " of stack " + stack.name + " is null. Possibly a leftover from bad previous manips.");
                    }
                    graphStackNode.AddElement(nodes[stackMember]);
                }
            }
            */

            // Connect the presenters.
            foreach (var takeAsset in nodesToReload)
            {
                if (takeAsset is TakeDevice)
                {
                    TakeDevice device = takeAsset as TakeDevice;

                    if (!nodes.ContainsKey(takeAsset))
                    {
                        Debug.LogError("No element found for " + takeAsset);
                        continue;
                    }

                    /*
                    var graphNode = nodes[takeAsset] as Node;

                    if (mathOperator.left != null && nodes.ContainsKey(mathOperator.left))
                    {
                        var outputPort = (nodes[mathOperator.left] as Node).outputContainer[0] as Port;
                        var inputPort = graphNode.inputContainer[0] as Port;

                        Edge edge = inputPort.ConnectTo(outputPort);
                        edge.persistenceKey = mathOperator.left.nodeID + "_edge";
                        AddElement(edge);
                    }
                    else if (mathOperator.left != null)
                    {
                        //add.m_Left = null;
                        Debug.LogWarning("Invalid left operand for operator " + mathOperator.ToString() + " , " + mathOperator.left.ToString());
                    }

                    if (mathOperator.right != null && nodes.ContainsKey(mathOperator.right))
                    {
                        var outputPort = (nodes[mathOperator.right] as Node).outputContainer[0] as Port;
                        var inputPort = graphNode.inputContainer[1] as Port;

                        Edge edge = inputPort.ConnectTo(outputPort);
                        edge.persistenceKey = mathOperator.right.nodeID + "_edge";
                        AddElement(edge);
                    }
                    else if (mathOperator.right != null)
                    {
                        //add.m_Right = null;
                        Debug.LogWarning("Invalid right operand for operator " + mathOperator.ToString() + " , " + mathOperator.right.ToString());
                    }
                    */
                }
                else if (takeAsset is TakeActor)
                {
                    TakeActor actor = takeAsset as TakeActor;

                    if (!nodes.ContainsKey(takeAsset))
                    {
                        Debug.LogError("No element found for " + takeAsset);
                        continue;
                    }

                    /*
                    var graphNode = nodes[takeAsset] as Node;

                    for (int i = 0; i < mathFunction.parameterCount; ++i)
                    {
                        TakeAsset param = mathFunction.GetParameter(i);

                        if (param != null && nodes.ContainsKey(param))
                        {
                            var outputPort = (nodes[param] as Node).outputContainer[0] as Port;
                            var inputPort = graphNode.inputContainer[i] as Port;

                            Edge edge = inputPort.ConnectTo(outputPort);
                            edge.persistenceKey = param.nodeID + "_edge";
                            AddElement(edge);
                        }
                        else if (param != null)
                        {
                            Debug.LogWarning("Invalid parameter for function" + mathFunction.ToString() + " , " +
                                param.ToString());
                        }
                    }
                    */
                }
            }
        }
    }
}
