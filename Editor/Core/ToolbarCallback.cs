using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CustomToolbar.Editor.Core
{
      [InitializeOnLoad]
      public static class ToolbarCallback
      {
            private readonly static Type SToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            private readonly static FieldInfo SRootField = SToolbarType?.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            private static ScriptableObject sCurrentToolbar;

            public static Action OnToolbarGUILeftOfCenter;
            public static Action OnToolbarGUIRightOfCenter;

            static ToolbarCallback()
            {
                  EditorApplication.update -= TryInitialize;
                  EditorApplication.update += TryInitialize;
            }

            private static void TryInitialize()
            {
                  if (sCurrentToolbar == null)
                  {
                        Object[] toolbars = Resources.FindObjectsOfTypeAll(SToolbarType);

                        if (toolbars.Length == 0)
                        {
                              return;
                        }

                        sCurrentToolbar = (ScriptableObject)toolbars[0];
                  }

                  InjectToolbarElements();

                  EditorApplication.update -= TryInitialize;
            }

            private static void InjectToolbarElements()
            {
                  var root = SRootField?.GetValue(sCurrentToolbar) as VisualElement;

                  if (root == null)
                  {
                        return;
                  }

                  VisualElement playModeButtons = root.Q("ToolbarZonePlayMode");

                  if (playModeButtons == null)
                  {
                        Debug.LogWarning("CustomToolbar: Could not find 'ToolbarZonePlayMode'. Centered elements will not be drawn.");

                        return;
                  }

                  VisualElement parent = playModeButtons.parent;

                  if (parent == null)
                  {
                        return;
                  }

                  var leftContainer = new IMGUIContainer(static () => OnToolbarGUILeftOfCenter?.Invoke())
                  {
                              style =
                              {
                                          alignContent = Align.Center,
                                          alignItems = Align.Center,
                                          alignSelf = Align.Stretch,
                                          display = DisplayStyle.Flex,
                                          flexDirection = FlexDirection.Row,
                                          justifyContent = Justify.FlexEnd
                              }
                  };
                  parent.Insert(parent.IndexOf(playModeButtons), leftContainer);

                  var rightContainer = new IMGUIContainer(static () => OnToolbarGUIRightOfCenter?.Invoke())
                  {
                              style =
                              {
                                          alignContent = Align.Center,
                                          alignItems = Align.Center,
                                          alignSelf = Align.Stretch,
                                          display = DisplayStyle.Flex,
                                          flexDirection = FlexDirection.Row,
                                          justifyContent = Justify.FlexStart
                              }
                  };
                  parent.Insert(parent.IndexOf(playModeButtons) + 1, rightContainer);
            }
      }
}