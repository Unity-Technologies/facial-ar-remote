using System.Collections.Generic;
using UnityEditor;

namespace Unity.Labs.FacialRemote
{
    public interface IUseEditorCallbackTicker
    {
        bool Attached { get; }

        void EditorTick();
        void Attach();
        void Detach();
    }

    public static class EditorCallbackTicker
    {
        static HashSet<IUseEditorCallbackTicker> s_AttachedCallbackTickers;
        static bool s_Enabled;

        public static bool enabled { get { return s_Enabled; } }

        static void EnableTicker()
        {
            EditorApplication.delayCall += EditorTick;
            s_Enabled = false;
        }

        static void DisableTicker()
        {
            EditorApplication.delayCall -= EditorTick;
            s_Enabled = true;
        }

        static void EditorTick()
        {
            if (s_AttachedCallbackTickers == null)
            {
                s_AttachedCallbackTickers = new HashSet<IUseEditorCallbackTicker>();
                DisableTicker();
            }

            if (s_AttachedCallbackTickers.Count == 0)
                DisableTicker();

            foreach (var attachedCallbackTicker in s_AttachedCallbackTickers)
            {
                attachedCallbackTicker.EditorTick();
            }
        }

        public static void AttachObject(IUseEditorCallbackTicker obj)
        {
            if (s_AttachedCallbackTickers == null)
            {
                s_AttachedCallbackTickers = new HashSet<IUseEditorCallbackTicker>();
            }

            s_AttachedCallbackTickers.Add(obj);

            if (!s_Enabled)
            {
                EnableTicker();
            }
        }

        public static void DetachObject(IUseEditorCallbackTicker obj)
        {
            if (s_AttachedCallbackTickers == null)
            {
                s_AttachedCallbackTickers = new HashSet<IUseEditorCallbackTicker>();
            }
            else if (s_AttachedCallbackTickers.Contains(obj))
            {
                s_AttachedCallbackTickers.Remove(obj);
            }

            if (s_Enabled && s_AttachedCallbackTickers.Count == 0)
                DisableTicker();
        }
    }
}
