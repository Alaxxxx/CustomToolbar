using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CustomToolbar.Editor.Core
{
      public static class ToolbarCallback
      {
            public readonly static Type MToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            public readonly static Type MGUIViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
            public readonly static Type MiWindowBackendType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");
            public static PropertyInfo MWindowBackend = MGUIViewType.GetProperty("windowBackend", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static PropertyInfo MViewVisualTree = MiWindowBackendType.GetProperty("visualTree",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static FieldInfo MimguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public static ScriptableObject MCurrentToolbar;

            public static Action OnToolbarGUI;
            public static Action OnToolbarGUILeft;
            public static Action OnToolbarGUIRight;

            static ToolbarCallback()
            {
                  EditorApplication.update -= OnUpdate;
                  EditorApplication.update += OnUpdate;
            }

            private static void OnUpdate()
            {
                  if (MCurrentToolbar == null)
                  {
                        Object[] toolbars = Resources.FindObjectsOfTypeAll(MToolbarType);
                        MCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

                        if (MCurrentToolbar != null)
                        {
                              FieldInfo root = MCurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                              object rawRoot = root!.GetValue(MCurrentToolbar);
                              var mRoot = rawRoot as VisualElement;
                              registerCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                              registerCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                              void registerCallback(string root, Action cb)
                              {
                                    VisualElement toolbarZone = mRoot.Q(root);

                                    var parent = new VisualElement
                                    {
                                                style =
                                                {
                                                            flexGrow = 1,
                                                            flexDirection = FlexDirection.Row,
                                                }
                                    };
                                    var container = new IMGUIContainer();
                                    container.onGUIHandler += () => { cb?.Invoke(); };
                                    parent.Add(container);
                                    toolbarZone.Add(parent);
                              }
                        }
                  }
            }

            private static void OnGUI()
            {
                  Action handler = OnToolbarGUI;

                  if (handler != null)
                  {
                        handler();
                  }
            }
      }
}