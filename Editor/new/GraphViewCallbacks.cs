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
    public class GraphViewCallbacks
    {
        private TakeSystem m_TakeSystem;
        private TakeGraphView m_TakeGraphView;

        public void Init(TakeSystem takeSystem, TakeGraphView takeGraphView)
        {
            m_TakeSystem = takeSystem;
            m_TakeGraphView = takeGraphView;

            m_TakeGraphView.graphViewChanged = GraphViewChanged;
            m_TakeGraphView.groupTitleChanged = OnGroupTitleChanged;
            m_TakeGraphView.elementsAddedToGroup = OnElementsAddedToGroup;
            m_TakeGraphView.elementsRemovedFromGroup = OnElementsRemovedFromGroup;

            m_TakeGraphView.elementsInsertedToStackNode = OnElementsInsertedToStackNode;
            m_TakeGraphView.elementsRemovedFromStackNode = OnElementsRemovedFromStackNode;

            takeGraphView.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            takeGraphView.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);

            //m_GraphView.elementDeleted = ElementDeletedCallback;
            //m_GraphView.edgeConnected = EdgeConnected;
            //m_GraphView.edgeDisconnected = EdgeDisconnected;
        }

        private GraphViewChange GraphViewChanged(GraphViewChange graphViewChange)
        {
            /*
            bool needToComputeOutputs = false;
            */

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    if (element is Node || element is Group || element is BlackboardField)
                        ElementDeletedCallback(element);
                    else if (element is Edge)
                        EdgeDisconnected(element as Edge);
                }
                /*
                m_GraphView.window.RemoveEmptyScopes();
                needToComputeOutputs = true;
                */
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    EdgeConnected(edge);
                }
                /*
                needToComputeOutputs = true;
                */
            }

            if (graphViewChange.movedElements != null)
            {
                /*
                foreach (GraphElement element in graphViewChange.movedElements)
                {
                    TakeNode takeNode = element.userData as TakeNode;
                    if (takeNode == null)
                        continue;

                    takeNode.m_Position = element.layout.position;
                }
                */
            }

            /*
            if (needToComputeOutputs)
            {
                m_TakeSystem.inputOutputs.ComputeOutputs();
            }
            */

            return graphViewChange;
        }

        private void ElementDeletedCallback(VisualElement ve)
        {
            if (m_TakeSystem == null)
                return;

            if (ve.userData is TakeAsset)
            {
                m_TakeGraphView.window.DestroyNode(ve.userData as TakeAsset);
            }
            /*
            else if (ve.userData is MathBookField)
            {
                var mathBookField = ve.userData as MathBookField;

                if (mathBookField != null)
                {
                    m_TakeSystem.inputOutputs.RemoveField(mathBookField);

                    // Removes the containing row from its parent section
                    BlackboardRow row = ve.GetFirstAncestorOfType<BlackboardRow>();

                    if (row != null)
                    {
                        row.RemoveFromHierarchy();
                    }
                }
            }
            */
        }

        private void OnGroupTitleChanged(Group graphGroupNode, string title)
        {
            /*
            var mathGroupNode = graphGroupNode.userData as MathGroupNode;

            if (mathGroupNode != null)
            {
                mathGroupNode.m_Title = graphGroupNode.title;
            }
            */
        }

        private void OnElementsAddedToGroup(Group graphGroup, IEnumerable<GraphElement> element)
        {
            /*
            var mathGroupNode = graphGroup.userData as MathGroupNode;

            if (mathGroupNode != null)
            {
                foreach (var takeNode in element.Select(e => e.userData).OfType<TakeNode>())
                {
                    takeNode.groupNode = mathGroupNode;
                }
            }

            m_GraphView.window.RemoveEmptyScopes();
            */
        }

        private void OnElementsRemovedFromGroup(Group graphGroup, IEnumerable<GraphElement> element)
        {
            /*
            foreach (var takeNode in element.Select(e => e.userData).OfType<TakeNode>())
            {
                takeNode.groupNode = null;
            }
            */
        }

        private void OnElementsInsertedToStackNode(StackNode graphStackNode, int index, IEnumerable<GraphElement> elements)
        {
            /*
            var mathStackNode = graphStackNode.userData as MathStackNode;

            if (mathStackNode != null)
            {
                mathStackNode.InsertNodes(index, elements.Select(e => e.userData as TakeNode));
            }
            */
        }

        private void OnElementsRemovedFromStackNode(StackNode graphStackNode, IEnumerable<GraphElement> elements)
        {
            /*
            var mathStackNode = graphStackNode.userData as MathStackNode;

            if (mathStackNode != null)
            {
                mathStackNode.RemoveNodes(elements.Select(e => e.userData as TakeNode));
            }
            */
        }

        private void SetEdgeConnection(Edge edge, TakeAsset takeAsset)
        {
            if (edge == null || edge.input == null || edge.output == null)
                return;

            /*
            if (edge.input.userData is MathOperator || edge.input.userData is MathFunction)
            {
                int inputIndex = 0;
                foreach (var input in edge.input.parent.Children())
                {
                    if (input == edge.input)
                        break;
                    inputIndex++;
                }

                var mathOperation = edge.input.userData as MathOperator;

                if (mathOperation)
                {
                    if (inputIndex == 0)
                        mathOperation.left = takeNode;
                    else
                        mathOperation.right = takeNode;
                }
                else
                {
                    var mathFunction = edge.input.userData as MathFunction;

                    mathFunction.SetParameter(inputIndex, takeNode);
                }
            }
            else if (edge.input.userData is MathResult)
            {
                var resultNode = edge.input.userData as MathResult;

                resultNode.root = takeNode;
            }

            if (takeNode != null)
                edge.persistenceKey = takeNode.nodeID + "_edge";
            */
        }

        private void EdgeConnected(Edge edge)
        {
            if (edge == null || edge.output == null)
                return;

            var outputTakeAsset = edge.output.userData as TakeAsset;
            SetEdgeConnection(edge, outputTakeAsset);
        }

        private void EdgeDisconnected(Edge edge)
        {
            SetEdgeConnection(edge, null);
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent e)
        {
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (selection != null && (selection.OfType<BlackboardField>().Count() >= 0))
            {
                DragAndDrop.visualMode = e.ctrlKey ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Move;
            }
        }

        private void OnDragPerformEvent(DragPerformEvent e)
        {
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (selection == null)
            {
                return;
            }

            IEnumerable<BlackboardField> fields = selection.OfType<BlackboardField>();

            if (fields.Count() == 0)
                return;

            Vector2 localPos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(m_TakeGraphView.contentViewContainer, e.localMousePosition);

            /*
            foreach (BlackboardField field in fields)
            {
                MathBookField bookField = field.userData as MathBookField;

                if (bookField == null)
                    continue;

                TakeNode fieldNode = null;

                if (bookField.direction == MathBookField.Direction.Input)
                {
                    var varFieldNode = ScriptableObject.CreateInstance<MathBookInputNode>();

                    varFieldNode.fieldName = bookField.name;
                    fieldNode = varFieldNode;
                }
                else
                {
                    var resFieldNode = ScriptableObject.CreateInstance<MathBookOutputNode>();

                    resFieldNode.fieldName = bookField.name;
                    fieldNode = resFieldNode;
                }

                fieldNode.m_Position = localPos;
                m_GraphView.window.AddNode(fieldNode);

                var visualNode = m_GraphView.window.CreateNode(fieldNode) as Node;

                m_GraphView.AddElement(visualNode);

                localPos += new Vector2(0, 25);
            }
            */
        }
    }
}
