using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.Core
{
      /// <summary>
      /// Provides a callback system for injecting custom GUI elements into Unity's toolbar.
      /// This class uses reflection to access Unity's internal toolbar system and allows
      /// adding custom controls to the left and right of the play mode buttons.
      /// </summary>
      [InitializeOnLoad]
      public static class ToolbarCallback
      {
            // Unity's internal toolbar reference
            private readonly static Type UnityToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            private readonly static FieldInfo UnityToolbarRootField = UnityToolbarType?.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            private static ScriptableObject currentToolbar;

            // GUI callbacks for custom elements
            public static Action OnToolbarGUILeftOfCenter;
            public static Action OnToolbarGUIRightOfCenter;

            static ToolbarCallback()
            {
                  EditorApplication.update -= TryInitialize;

                  EditorApplication.update += TryInitialize;
            }

            private static void TryInitialize()
            {
                  if (currentToolbar == null)
                  {
                        Object[] toolbars = Resources.FindObjectsOfTypeAll(UnityToolbarType);

                        // Prevent a bug where the toolbar is not found but should be present
                        if (toolbars.Length == 0)
                        {
                              return;
                        }

                        currentToolbar = (ScriptableObject)toolbars[0];
                  }

                  InjectToolbarElements();

                  EditorApplication.update -= TryInitialize;
            }

            private static void InjectToolbarElements()
            {
                  if (UnityToolbarRootField?.GetValue(currentToolbar) is not VisualElement root)
                  {
                        return;
                  }

                  // Find the play mode buttons container by its USS class name
                  VisualElement zoneLeft = root.Q("ToolbarZoneLeftAlign");
                  VisualElement zoneRight = root.Q("ToolbarZoneRightAlign");

                  if (zoneLeft == null || zoneRight == null)
                  {
                        Debug.LogError("[CUSTOM TOOLBAR]: Could not find Toolbar containers. Elements will not be drawn.");
                        Debug.LogWarning("[CUSTOM TOOLBAR]: USS class 'ToolbarZoneLeftAlign' and 'ToolbarZoneRightAlign' might have changed and needs to be updated.");

                        return;
                  }

                  // Create a container for custom GUI elements positioned to the left of play mode buttons
                  var leftContainer = new IMGUIContainer(static () => OnToolbarGUILeftOfCenter?.Invoke())
                  {
                              style =
                              {
                                          alignContent = Align.Center,
                                          alignItems = Align.Center,
                                          alignSelf = Align.Center,
                                          display = DisplayStyle.Flex,
                                          flexDirection = FlexDirection.Row,
                                          justifyContent = Justify.FlexEnd,
                                          flexGrow = 1
                              }
                  };

                  // Insert the left container before the play mode buttons
                  zoneLeft.Add(leftContainer);

                  // Create a container for custom GUI elements positioned to the right of play mode buttons
                  var rightContainer = new IMGUIContainer(static () => OnToolbarGUIRightOfCenter?.Invoke())
                  {
                              style =
                              {
                                          alignContent = Align.Center,
                                          alignItems = Align.Center,
                                          alignSelf = Align.Center,
                                          display = DisplayStyle.Flex,
                                          flexDirection = FlexDirection.Row,
                                          justifyContent = Justify.FlexStart,
                                          flexGrow = 1
                              }
                  };

                  // Insert the right container after the play mode buttons
                  zoneRight.Add(rightContainer);
            }
      }
}