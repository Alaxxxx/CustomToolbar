using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.Settings;
using CustomToolbar.Editor.Settings.Data;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      /// <summary>
      /// Initializes and manages custom toolbar elements based on configuration settings.
      /// This class creates toolbar elements from configuration data and handles their lifecycle.
      /// </summary>
      [InitializeOnLoad]
      public static class ToolbarInitializer
      {
            // Container for toolbar elements for each side
            private readonly static List<BaseToolbarElement> LeftElements = new();
            private readonly static List<BaseToolbarElement> RightElements = new();

            // Static constructor that runs when the class is first accessed.
            // Initializes the toolbar system by loading configuration and setting up callbacks.
            static ToolbarInitializer()
            {
                  // Get the toolbar configuration from settings
                  ToolbarConfiguration config = ToolbarSettings.Instance;

                  // Create actual toolbar element instances from the configuration data
                  CreateElementsFromConfig(config);

                  // Register the toolbar drawing callbacks
                  ToolbarCallback.OnToolbarGUILeftOfCenter = DrawToolbar(LeftElements);
                  ToolbarCallback.OnToolbarGUIRightOfCenter = DrawToolbar(RightElements);

                  // Set up event subscriptions for all created elements
                  SubscribeToEditorEvents();
            }

            // Creates toolbar element instances from the configuration data.
            // Processes each enabled group and element, instantiating them via reflection.
            private static void CreateElementsFromConfig(ToolbarConfiguration config)
            {
                  if (config == null)
                  {
                        return;
                  }

                  // Process each enabled group in the configuration
                  foreach (ToolbarGroup group in config.groups.Where(static g => g.isEnabled))
                  {
                        // Determine which collection to add elements to based on the group's side setting
                        List<BaseToolbarElement> targetList = group.side == ToolbarSide.Left ? LeftElements : RightElements;

                        // Add a space element at the beginning of each group for visual separation
                        targetList.Add(new ToolbarSpace());

                        // Process each enabled element within the current group
                        foreach (ToolbarElement elementConfig in group.elements.Where(static e => e.isEnabled))
                        {
                              // Use reflection to get the Type from the element's name string
                              var type = Type.GetType(elementConfig.name);

                              if (type != null)
                              {
                                    // Create and add an instance of the toolbar element using Activator
                                    var elementInstance = (BaseToolbarElement)Activator.CreateInstance(type);
                                    targetList.Add(elementInstance);
                              }
                        }
                  }

                  // Add trailing spaces to both sides if they contain any elements
                  if (LeftElements.Any())
                  {
                        LeftElements.Add(new ToolbarSpace());
                  }

                  if (RightElements.Any())
                  {
                        RightElements.Add(new ToolbarSpace());
                  }
            }

            // Creates and returns an Action that draws all toolbar elements in a horizontal layout.
            private static Action DrawToolbar(IReadOnlyList<BaseToolbarElement> elements)
            {
                  return () =>
                  {
                        GUILayout.BeginHorizontal();

                        foreach (BaseToolbarElement element in elements)
                        {
                              element.OnDrawInToolbar();
                        }

                        GUILayout.EndHorizontal();
                  };
            }

            // Sets up event subscriptions for all toolbar elements.
            private static void SubscribeToEditorEvents()
            {
                  // Combine all toolbar elements from both sides into a single collection
                  List<BaseToolbarElement> allElements = LeftElements.Concat(RightElements).ToList();

                  if (!allElements.Any())
                  {
                        return;
                  }

                  // Subscribe to Unity's play mode state change event
                  EditorApplication.playModeStateChanged += state => { allElements.ForEach(e => e.OnPlayModeStateChanged(state)); };

                  // Initialize all toolbar elements by calling their OnInit method
                  allElements.ForEach(static e => e.OnInit());
            }
      }
}