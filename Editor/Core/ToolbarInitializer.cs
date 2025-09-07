using System;
using System.Collections.Generic;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.Core.Data;
using OpalStudio.CustomToolbar.Editor.Settings;
using OpalStudio.CustomToolbar.Editor.ToolbarElements;
using OpalStudio.CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.Core
{
      /// <summary>
      /// Initializes and manages custom toolbar elements based on configuration settings.
      /// This class creates toolbar elements from configuration data and handles their lifecycle.
      /// </summary>
      [InitializeOnLoad]
      public static class ToolbarInitializer
      {
            private readonly static List<BaseToolbarElement> LeftElements = new();
            private readonly static List<BaseToolbarElement> RightElements = new();

            static ToolbarInitializer()
            {
                  ToolbarConfiguration config = ToolbarSettings.Instance;

                  CreateElementsFromConfig(config);

                  // Register the toolbar drawing callbacks
                  ToolbarCallback.OnToolbarGUILeftOfCenter = DrawToolbar(LeftElements, true);
                  ToolbarCallback.OnToolbarGUIRightOfCenter = DrawToolbar(RightElements, false);

                  SubscribeToEditorEvents();

                  EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
            }

            private static void CreateElementsFromConfig(ToolbarConfiguration config)
            {
                  if (config == null)
                  {
                        return;
                  }

                  // Process each enabled group in the configuration
                  foreach (ToolbarGroup group in config.groups.Where(static g => g.isEnabled))
                  {
                        List<BaseToolbarElement> targetList = group.side == ToolbarSide.Left ? LeftElements : RightElements;

                        targetList.Add(new ToolbarSpace());

                        // Process each enabled element within the current group
                        foreach (ToolbarElement elementConfig in group.elements.Where(static e => e.isEnabled))
                        {
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
            private static Action DrawToolbar(IReadOnlyList<BaseToolbarElement> elements, bool alignRight)
            {
                  return () =>
                  {
                        GUILayout.BeginHorizontal();

                        if (alignRight)
                        {
                              GUILayout.FlexibleSpace();
                        }

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

            private static void HandlePlayModeStateChange(PlayModeStateChange state)
            {
                  if (state == PlayModeStateChange.EnteredEditMode)
                  {
                        SceneAssetsUtils.RestoreSceneAfterPlay();
                  }
            }
      }
}