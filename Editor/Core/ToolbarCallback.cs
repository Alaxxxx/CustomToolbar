using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CustomToolbar.Editor.Core
{
      /// <summary>
      /// Provides a callback system for injecting custom GUI elements into Unity's toolbar.
      /// This class uses reflection to access Unity's internal toolbar system and allows
      /// adding custom controls to the left and right of the play mode buttons.
      /// </summary>
      [InitializeOnLoad]
      public static class ToolbarCallback
      {
            // Type reference to Unity's internal Toolbar class obtained through reflection
            private readonly static Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

            // Field info for accessing the private "m_Root" field of the toolbar, which contains the root visual element
            private readonly static FieldInfo RootField = ToolbarType?.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);

            // Reference to the current active toolbar instance as a ScriptableObject
            private static ScriptableObject currentToolbar;

            // Action invoked when custom GUI should be drawn to the left or right of the center play mode buttons
            public static Action OnToolbarGUILeftOfCenter;
            public static Action OnToolbarGUIRightOfCenter;

            // Static constructor that runs when the class is first accessed.
            // Sets up the initialization process by subscribing to EditorApplication.update.
            static ToolbarCallback()
            {
                  // Remove any existing subscription to prevent duplicates
                  EditorApplication.update -= TryInitialize;

                  // Subscribe to the update event to attempt initialization on each editor update
                  EditorApplication.update += TryInitialize;
            }

            // Attempts to initialize the toolbar customization system.
            private static void TryInitialize()
            {
                  // Check if we haven't found the toolbar instance yet
                  if (currentToolbar == null)
                  {
                        // Find all instances of the internal Toolbar type in the current scene
                        Object[] toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);

                        // If no toolbar instances are found, exit and try again on the next update
                        if (toolbars.Length == 0)
                        {
                              return;
                        }

                        // Store reference to the first (and only) toolbar instance
                        currentToolbar = (ScriptableObject)toolbars[0];
                  }

                  // Proceed with injecting our custom elements into the toolbar
                  InjectToolbarElements();

                  // Remove ourselves from the update loop since initialization is complete
                  EditorApplication.update -= TryInitialize;
            }

            // Injects custom IMGUI containers into Unity's toolbar layout.
            private static void InjectToolbarElements()
            {
                  // Exit if the toolbar type or root field is not found
                  if (RootField?.GetValue(currentToolbar) is not VisualElement root)
                  {
                        return;
                  }

                  // Find the play mode buttons container by its USS class name
                  VisualElement playModeButtons = root.Q("ToolbarZonePlayMode");

                  // If the play mode buttons container is not found,
                  // A warning is logged to hint that the USS class name might have changed
                  if (playModeButtons == null)
                  {
                        Debug.LogWarning("CustomToolbar: Could not find 'ToolbarZonePlayMode'. Centered elements will not be drawn.");

                        return;
                  }

                  // Get the parent container that holds the play mode buttons
                  VisualElement parent = playModeButtons.parent;

                  // If the parent container is null, exit
                  if (parent == null)
                  {
                        return;
                  }

                  // Create a container for custom GUI elements positioned to the left of play mode buttons
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

                  // Insert the left container before the play mode buttons
                  parent.Insert(parent.IndexOf(playModeButtons), leftContainer);

                  // Create a container for custom GUI elements positioned to the right of play mode buttons
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

                  // Insert the right container after the play mode buttons
                  parent.Insert(parent.IndexOf(playModeButtons) + 1, rightContainer);
            }
      }
}