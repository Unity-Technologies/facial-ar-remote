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
    public class TakeNode : Node
    {
        /*
        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return Port.Create<SimpleEdge>(orientation, direction, capacity, type);
        }
        */

        public override void SetPosition(Rect newPos)
        {
            /*
            MathNode mathNode = userData as MathNode;

            if (mathNode == null)
            {
                return;
            }

            mathNode.m_Position = newPos.position;
            */
            base.SetPosition(newPos);
        }

        /*
        void AddNoteMenuItems(DropdownMenu menu, string menuText, string category, Action<VisualElement, SpriteAlignment> createMethod)
        {
            Array spriteAlignments = Enum.GetValues(typeof(SpriteAlignment));
            Array.Reverse(spriteAlignments);
            foreach (SpriteAlignment align in spriteAlignments)
            {
                SpriteAlignment alignment = align;
                menu.AppendAction(menuText + "/" + category + "/" + alignment, (a) => createMethod(this, alignment), DropdownMenu.MenuAction.AlwaysEnabled);
            }
        }
        */

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            /*
            if (evt.target is SimpleNode)
            {
                AddNoteMenuItems(evt.menu, "Attach Badge", "Comment Badge", NoteManager.CreateCommentNote);
                AddNoteMenuItems(evt.menu, "Attach Badge", "Error Badge", NoteManager.CreateErrorNote);
            }
            */

            base.BuildContextualMenu(evt);
        }
    }
}
