using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess
{
      sealed internal class ToolbarQuickAccess : BaseToolbarElement
      {
            private GUIContent _buttonContent;

            protected override string Name => "Quick Access";
            protected override string Tooltip => "Access recently selected assets, GameObjects and scenes.";

            public override void OnInit()
            {
                  this.Width = 35;
                  QuickAccessTracker.StartTracking();
                  Texture icon = EditorGUIUtility.IconContent("d_UndoHistory").image;
                  _buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(_buttonContent, FocusType.Keyboard, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                  {
                        BuildQuickAccessMenu().ShowAsContext();
                  }
            }

            private static int GetCategorySortOrder(string category)
            {
                  return category switch
                  {
                              "Scenes" => 0,
                              "Hierarchy" => 1,
                              "Prefabs" => 2,
                              "Scripts" => 3,
                              "ScriptableObjects" => 4,
                              "Folders" => 5,
                              "Animators" => 6,
                              "Others" => 99,
                              _ => 10,
                  };
            }

            private static GenericMenu BuildQuickAccessMenu()
            {
                  var menu = new GenericMenu();
                  List<QuickAccessItem> recentItems = QuickAccessManager.Instance.GetRecentItems();

                  if (recentItems.Count == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No recent items"));
                  }
                  else
                  {
                        IOrderedEnumerable<IGrouping<string, QuickAccessItem>> groupedItems = recentItems
                                                                                              .GroupBy(QuickAccessManager.GetCategoryKey)
                                                                                              .OrderBy(static g => GetCategorySortOrder(g.Key));

                        foreach (IGrouping<string, QuickAccessItem> group in groupedItems)
                        {
                              string category = group.Key;

                              foreach (QuickAccessItem item in group)
                              {
                                    AddMenuItem(menu, item, category);
                              }
                        }
                  }

                  menu.AddSeparator("");
                  menu.AddItem(new GUIContent("Clear History"), false, QuickAccessManager.Instance.ClearHistory);

                  return menu;
            }

            private static void AddMenuItem(GenericMenu menu, QuickAccessItem item, string category)
            {
                  Object obj = item.GetObject();

                  if (obj == null)
                  {
                        return;
                  }

                  var content = new GUIContent($"{category}/{item.displayName}");

                  bool isEnabled = true;

                  if (item.ItemType == QuickAccessItemType.GameObject)
                  {
                        var go = (GameObject)obj;
                        Scene scene = go.scene;

                        if (!scene.isLoaded)
                        {
                              content.text += $" ({scene.name})";
                              isEnabled = false;
                        }
                  }

                  if (isEnabled)
                  {
                        menu.AddItem(content, false, () => SelectItem(item));
                  }
                  else
                  {
                        menu.AddDisabledItem(content);
                  }
            }

            private static void SelectItem(QuickAccessItem item)
            {
                  Object obj = item.GetObject();

                  if (obj == null)
                  {
                        Debug.LogWarning($"[CustomToolbar] Quick Access item '{item.displayName}' could not be found.");

                        return;
                  }

                  if (obj is DefaultAsset && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                  {
                        Type projectWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
                        EditorWindow projectBrowser = EditorWindow.GetWindow(projectWindow);
                        MethodInfo showFolderMethod = projectWindow.GetMethod("ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);

                        if (showFolderMethod != null)
                        {
                              showFolderMethod.Invoke(projectBrowser, new object[] { obj.GetInstanceID(), false });
                        }
                        else
                        {
                              Selection.activeObject = obj;
                              EditorGUIUtility.PingObject(obj);
                        }

                        return;
                  }

                  switch (item.ItemType)
                  {
                        case QuickAccessItemType.Scene:
                              if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                              {
                                    EditorSceneManager.OpenScene(item.scenePath, OpenSceneMode.Single);
                              }

                              break;

                        case QuickAccessItemType.Asset:
                              Selection.activeObject = obj;
                              EditorGUIUtility.PingObject(obj);

                              if (obj is GameObject)
                              {
                                    AssetDatabase.OpenAsset(obj);
                              }

                              break;

                        case QuickAccessItemType.GameObject:
                              var go = (GameObject)obj;

                              if (go.scene.isLoaded)
                              {
                                    Selection.activeGameObject = go;
                                    EditorGUIUtility.PingObject(go);
                                    SceneView.FrameLastActiveSceneView();
                              }

                              break;
                  }
            }
      }
}