﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomToolbar.Editor.Settings
{
      /// <summary>
      /// Unity Settings Provider for Custom Toolbar configuration.
      /// Integrates the toolbar settings into Unity's Project Settings window.
      /// </summary>
      public sealed class ToolbarSettingsProvider : SettingsProvider, IDisposable
      {
            // SerializedObject wrapper for the toolbar configuration asset.
            // Enables Unity's property field system to automatically handle GUI drawing,
            // undo/redo operations, and change tracking for the configuration data.
            private SerializedObject _serializedConfig;

            private object _selectedObject;

            private Vector2 _leftPanelScrollPos;
            private readonly HashSet<string> _expandedFolders = new();

            private ToolbarSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
            {
            }

            // Static factory method that Unity calls to create and register this settings provider.
            // The [SettingsProvider] attribute tells Unity to automatically discover and register this method.
            [SettingsProvider]
            public static SettingsProvider CreateToolbarSettingsProvider()
            {
                  // Create the provider with the menu path "Project/Custom Toolbar"
                  var provider = new ToolbarSettingsProvider("Project/Custom Toolbar");

                  provider.OnActivate(null, null);

                  return provider;
            }

            // Called when the settings provider becomes active/visible in the Project Settings window.
            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                  // Create a SerializedObject wrapper around the toolbar configuration singleton
                  _serializedConfig = new SerializedObject(ToolbarSettings.Instance);
            }

            // Called every frame to draw the settings provider's GUI using IMGUI.
            public override void OnGUI(string searchContext)
            {
                  if (_serializedConfig == null || _serializedConfig.targetObject == null)
                  {
                        EditorGUILayout.HelpBox("Settings asset could not be loaded.", MessageType.Warning);

                        return;
                  }

                  _serializedConfig.Update();

                  EditorGUILayout.BeginHorizontal();

                  EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(220), GUILayout.ExpandHeight(true));

                  _leftPanelScrollPos = EditorGUILayout.BeginScrollView(_leftPanelScrollPos);
                  DrawLeftPanel();
                  EditorGUILayout.EndScrollView();
                  EditorGUILayout.EndVertical();

                  EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                  DrawRightPanel();
                  EditorGUILayout.EndVertical();

                  EditorGUILayout.EndHorizontal();
                  _serializedConfig.ApplyModifiedProperties();
            }

            public void Dispose()
            {
                  _serializedConfig?.Dispose();
            }

#region Left Panel

            private void DrawLeftPanel()
            {
                  SerializedProperty groupsProperty = _serializedConfig.FindProperty("groups");
                  EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
                  var leftGroups = new List<(SerializedProperty prop, int originalIndex)>();
                  var rightGroups = new List<(SerializedProperty prop, int originalIndex)>();

                  for (int i = 0; i < groupsProperty.arraySize; i++)
                  {
                        SerializedProperty group = groupsProperty.GetArrayElementAtIndex(i);

                        if ((Data.ToolbarSide)group.FindPropertyRelative("side").enumValueIndex == Data.ToolbarSide.Left)
                        {
                              leftGroups.Add((group, i));
                        }
                        else
                        {
                              rightGroups.Add((group, i));
                        }
                  }

                  EditorGUILayout.LabelField("Left Side", EditorStyles.miniBoldLabel);
                  DrawGroupList(leftGroups, groupsProperty);
                  GUILayout.Space(10);

                  EditorGUILayout.LabelField("Right Side", EditorStyles.miniBoldLabel);
                  DrawGroupList(rightGroups, groupsProperty);

                  if (GUILayout.Button("Add New Group", GUILayout.Height(25)))
                  {
                        groupsProperty.InsertArrayElementAtIndex(groupsProperty.arraySize);
                        SerializedProperty newGroup = groupsProperty.GetArrayElementAtIndex(groupsProperty.arraySize - 1);
                        newGroup.FindPropertyRelative("groupName").stringValue = "New Group";
                        newGroup.FindPropertyRelative("isEnabled").boolValue = true;
                        _selectedObject = groupsProperty.arraySize - 1;
                  }

                  EditorGUILayout.Space(5);
                  EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.1f, 0.1f, 0.1f, 1));
                  EditorGUILayout.Space(5);

                  EditorGUILayout.LabelField("Toolbox Shortcuts", EditorStyles.boldLabel);
                  DrawToolboxSection();
            }

            private void DrawGroupList(List<(SerializedProperty prop, int originalIndex)> groupList, SerializedProperty parentArray)
            {
                  for (int i = 0; i < groupList.Count; i++)
                  {
                        (SerializedProperty groupProp, int originalIndex) = groupList[i];
                        string groupName = groupProp.FindPropertyRelative("groupName").stringValue;
                        bool isSelected = _selectedObject is int selectedIndex && selectedIndex == originalIndex;
                        var style = new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft };

                        if (isSelected)
                        {
                              style.fontStyle = FontStyle.Bold;
                              style.normal = EditorStyles.toolbarButton.onNormal;
                        }

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button(groupName, style, GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                        {
                              _selectedObject = originalIndex;
                        }

                        if (GUILayout.Button("▲", EditorStyles.toolbarButton, GUILayout.Width(20)) && i > 0)
                        {
                              parentArray.MoveArrayElement(originalIndex, groupList[i - 1].originalIndex);
                        }

                        if (GUILayout.Button("▼", EditorStyles.toolbarButton, GUILayout.Width(20)) && i < groupList.Count - 1)
                        {
                              parentArray.MoveArrayElement(originalIndex, groupList[i + 1].originalIndex);
                        }

                        EditorGUILayout.EndHorizontal();
                  }
            }

            private void DrawToolboxSection()
            {
                  SerializedProperty shortcutsProperty = _serializedConfig.FindProperty("toolboxShortcuts");

                  var shortcutsByMenu = new Dictionary<string, List<(SerializedProperty prop, int index)>>();

                  for (int i = 0; i < shortcutsProperty.arraySize; i++)
                  {
                        SerializedProperty shortcut = shortcutsProperty.GetArrayElementAtIndex(i);
                        string subMenuPath = shortcut.FindPropertyRelative("subMenuPath").stringValue.Trim();

                        if (!shortcutsByMenu.ContainsKey(subMenuPath))
                        {
                              shortcutsByMenu[subMenuPath] = new List<(SerializedProperty prop, int index)>();
                        }

                        shortcutsByMenu[subMenuPath].Add((shortcut, i));
                  }

                  if (shortcutsByMenu.TryGetValue("", out List<(SerializedProperty prop, int index)> rootShortcuts))
                  {
                        DrawShortcutList(rootShortcuts, shortcutsProperty);
                  }

                  foreach (KeyValuePair<string, List<(SerializedProperty prop, int index)>> kvp in shortcutsByMenu.Where(static k => !string.IsNullOrEmpty(k.Key))
                                       .OrderBy(static k => k.Key))
                  {
                        string folderName = kvp.Key;
                        bool isExpanded = _expandedFolders.Contains(folderName);

                        bool newIsExpanded = EditorGUILayout.Foldout(isExpanded, folderName, true, EditorStyles.foldout);

                        if (newIsExpanded != isExpanded)
                        {
                              if (newIsExpanded)
                              {
                                    _expandedFolders.Add(folderName);
                              }
                              else
                              {
                                    _expandedFolders.Remove(folderName);
                              }
                        }

                        if (newIsExpanded)
                        {
                              EditorGUI.indentLevel++;
                              DrawShortcutList(kvp.Value, shortcutsProperty);
                              EditorGUI.indentLevel--;
                        }
                  }

                  if (GUILayout.Button("Add Shortcut", GUILayout.Height(25)))
                  {
                        shortcutsProperty.InsertArrayElementAtIndex(shortcutsProperty.arraySize);
                        SerializedProperty newShortcut = shortcutsProperty.GetArrayElementAtIndex(shortcutsProperty.arraySize - 1);

                        newShortcut.FindPropertyRelative("displayName").stringValue = "New Shortcut";
                        newShortcut.FindPropertyRelative("isEnabled").boolValue = true;

                        _selectedObject = newShortcut;
                  }
            }

            private void DrawShortcutList(List<(SerializedProperty prop, int originalIndex)> shortcutList, SerializedProperty parentArray)
            {
                  for (int i = 0; i < shortcutList.Count; i++)
                  {
                        (SerializedProperty shortcutProp, int originalIndex) = shortcutList[i];
                        string shortcutName = shortcutProp.FindPropertyRelative("displayName").stringValue;
                        bool isSelected = _selectedObject is SerializedProperty selectedProp && selectedProp.propertyPath == shortcutProp.propertyPath;

                        var style = new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft };

                        if (isSelected)
                        {
                              style.normal = EditorStyles.toolbarButton.onNormal;
                        }

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button(shortcutName, style, GUILayout.Height(20), GUILayout.ExpandWidth(true)))
                        {
                              _selectedObject = shortcutProp;
                        }

                        if (GUILayout.Button("▲", EditorStyles.toolbarButton, GUILayout.Width(20)) && i > 0)
                        {
                              parentArray.MoveArrayElement(originalIndex, shortcutList[i - 1].originalIndex);
                        }

                        if (GUILayout.Button("▼", EditorStyles.toolbarButton, GUILayout.Width(20)) && i < shortcutList.Count - 1)
                        {
                              parentArray.MoveArrayElement(originalIndex, shortcutList[i + 1].originalIndex);
                        }

                        EditorGUILayout.EndHorizontal();
                  }
            }

#endregion

#region Right Panel

            private void DrawRightPanel()
            {
                  if (_selectedObject == null)
                  {
                        EditorGUILayout.HelpBox("Select an item from the left panel to edit its properties.", MessageType.Info);

                        return;
                  }

                  if (_selectedObject is SerializedProperty { type: "ToolboxShortcut" } selectedProp)
                  {
                        DrawShortcutEditor(selectedProp);
                  }
                  else if (_selectedObject is int selectedIndex)
                  {
                        SerializedProperty groupsProperty = _serializedConfig.FindProperty("groups");

                        if (selectedIndex >= 0 && selectedIndex < groupsProperty.arraySize)
                        {
                              DrawGroupContent(groupsProperty.GetArrayElementAtIndex(selectedIndex), selectedIndex);
                        }
                  }
            }

            private void DrawShortcutEditor(SerializedProperty shortcutProp)
            {
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("Display Name", GUILayout.Width(110));
                  EditorGUILayout.PropertyField(shortcutProp.FindPropertyRelative("displayName"), GUIContent.none);

                  SerializedProperty enabledProp = shortcutProp.FindPropertyRelative("isEnabled");
                  Color originalBgColor = GUI.backgroundColor;
                  GUI.backgroundColor = enabledProp.boolValue ? new Color(0.4f, 1f, 0.6f, 1f) : new Color(1f, 0.6f, 0.6f, 1f);

                  enabledProp.boolValue = GUILayout.Toggle(enabledProp.boolValue, "Enabled", EditorStyles.toolbarButton, GUILayout.Width(70));

                  GUI.backgroundColor = originalBgColor;
                  EditorGUILayout.EndHorizontal();

                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("Menu (Optional)", GUILayout.Width(110));
                  EditorGUILayout.PropertyField(shortcutProp.FindPropertyRelative("subMenuPath"), GUIContent.none, GUILayout.ExpandWidth(true));
                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.Space(5);

                  EditorGUILayout.LabelField("Action Path", EditorStyles.boldLabel);
                  SerializedProperty pathProp = shortcutProp.FindPropertyRelative("menuItemPath");
                  pathProp.stringValue = EditorGUILayout.TextArea(pathProp.stringValue, GUI.skin.textArea, GUILayout.MinHeight(60));

                  var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                  {
                              fontSize = 12,
                              padding = new RectOffset(10, 10, 10, 10),
                  };

                  EditorGUILayout.LabelField(
                              "Use prefixes for special actions:\n" + "• settings:Project/Player\n" + "• asset:Assets/Prefabs/My.prefab\n" + "• folder:Assets/Scenes\n" +
                              "• url:https://unity.com\n" + "• select:/Player/Camera\n" + "• method:MyNamespace.MyClass.MyMethod|param1,true", helpBoxStyle);

                  GUILayout.FlexibleSpace();
                  Color originalColor = GUI.backgroundColor;
                  GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

                  if (GUILayout.Button("Delete Shortcut") &&
                      EditorUtility.DisplayDialog("Delete Shortcut", "Are you sure you want to delete this shortcut?", "Yes", "No"))
                  {
                        SerializedProperty shortcutsProperty = _serializedConfig.FindProperty("toolboxShortcuts");

                        for (int i = 0; i < shortcutsProperty.arraySize; i++)
                        {
                              if (shortcutsProperty.GetArrayElementAtIndex(i).propertyPath == shortcutProp.propertyPath)
                              {
                                    shortcutsProperty.DeleteArrayElementAtIndex(i);
                                    _selectedObject = null;

                                    break;
                              }
                        }
                  }

                  GUI.backgroundColor = originalColor;
            }

            private void DrawGroupContent(SerializedProperty groupProperty, int groupIndex)
            {
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("Group Name", GUILayout.Width(85));
                  EditorGUILayout.PropertyField(groupProperty.FindPropertyRelative("groupName"), GUIContent.none);

                  SerializedProperty enabledProp = groupProperty.FindPropertyRelative("isEnabled");
                  Color originalBgColor = GUI.backgroundColor;
                  GUI.backgroundColor = enabledProp.boolValue ? new Color(0.4f, 1f, 0.6f, 1f) : new Color(1f, 0.6f, 0.6f, 1f);

                  enabledProp.boolValue = GUILayout.Toggle(enabledProp.boolValue, "Enabled", EditorStyles.toolbarButton, GUILayout.Width(70));

                  GUI.backgroundColor = originalBgColor;
                  EditorGUILayout.EndHorizontal();

                  SerializedProperty sideProp = groupProperty.FindPropertyRelative("side");
                  var currentSide = (Data.ToolbarSide)sideProp.enumValueIndex;
                  string switchButtonText = $"Move to {(currentSide == Data.ToolbarSide.Left ? "Right" : "Left")} Side";

                  if (GUILayout.Button(switchButtonText))
                  {
                        sideProp.enumValueIndex = 1 - sideProp.enumValueIndex;
                  }

                  EditorGUILayout.Space();
                  EditorGUILayout.LabelField("Elements", EditorStyles.boldLabel);
                  SerializedProperty elementsProperty = groupProperty.FindPropertyRelative("elements");

                  for (int i = 0; i < elementsProperty.arraySize; i++)
                  {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        DrawElementCard(elementsProperty.GetArrayElementAtIndex(i), elementsProperty, i);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                  }

                  if (GUILayout.Button("Add Element", GUILayout.Height(30)))
                  {
                        ShowAddElementMenu(elementsProperty);
                  }

                  GUILayout.FlexibleSpace();
                  Color originalColor = GUI.backgroundColor;
                  GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

                  if (GUILayout.Button("Delete Group", GUILayout.Height(25)) && EditorUtility.DisplayDialog("Delete Group",
                                  $"Are you sure you want to delete the group '{groupProperty.FindPropertyRelative("groupName").stringValue}'?", "Yes, Delete", "Cancel"))
                  {
                        SerializedProperty groupsProperty = _serializedConfig.FindProperty("groups");
                        groupsProperty.DeleteArrayElementAtIndex(groupIndex);
                        _selectedObject = null;
                  }

                  GUI.backgroundColor = originalColor;
            }

            private static void DrawElementCard(SerializedProperty elementProp, SerializedProperty parentArray, int index)
            {
                  EditorGUILayout.BeginHorizontal();
                  SerializedProperty typeNameProp = elementProp.FindPropertyRelative("name");
                  string displayName = "Unknown Element";
                  string tooltip = "No description available.";
                  var type = Type.GetType(typeNameProp.stringValue);

                  if (type != null)
                  {
                        var tempInstance = (BaseToolbarElement)Activator.CreateInstance(type);

                        PropertyInfo namePropInfo = typeof(BaseToolbarElement).GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance);

                        PropertyInfo tooltipPropInfo = typeof(BaseToolbarElement).GetProperty("Tooltip", BindingFlags.NonPublic | BindingFlags.Instance);
                        displayName = namePropInfo?.GetValue(tempInstance)?.ToString() ?? type.Name;
                        tooltip = tooltipPropInfo?.GetValue(tempInstance)?.ToString();
                  }

                  SerializedProperty enabledProp = elementProp.FindPropertyRelative("isEnabled");
                  EditorGUILayout.BeginVertical();
                  enabledProp.boolValue = EditorGUILayout.ToggleLeft(displayName, enabledProp.boolValue, EditorStyles.boldLabel);

                  EditorGUILayout.LabelField(tooltip, EditorStyles.wordWrappedMiniLabel);

                  EditorGUILayout.EndVertical();
                  GUILayout.FlexibleSpace();

                  if (GUILayout.Button("▲", GUILayout.Width(25)) && index > 0)
                  {
                        parentArray.MoveArrayElement(index, index - 1);
                  }

                  if (GUILayout.Button("▼", GUILayout.Width(25)) && index < parentArray.arraySize - 1)
                  {
                        parentArray.MoveArrayElement(index, index + 1);
                  }

                  if (GUILayout.Button("X", GUILayout.Width(25)) &&
                      EditorUtility.DisplayDialog("Delete Element", $"Are you sure you want to delete '{displayName}'?", "Yes", "No"))
                  {
                        parentArray.DeleteArrayElementAtIndex(index);
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void ShowAddElementMenu(SerializedProperty elementsProperty)
            {
                  var menu = new GenericMenu();
                  TypeCache.TypeCollection elementTypes = TypeCache.GetTypesDerivedFrom<BaseToolbarElement>();

                  foreach (Type type in elementTypes.Where(static t => !t.IsAbstract))
                  {
                        if (type == typeof(ToolbarSpace))
                        {
                              continue;
                        }

                        menu.AddItem(new GUIContent(type.Name.Replace("Toolbar", "", StringComparison.Ordinal)), false, () =>
                        {
                              int newIndex = elementsProperty.arraySize;
                              elementsProperty.InsertArrayElementAtIndex(newIndex);
                              SerializedProperty newElementProp = elementsProperty.GetArrayElementAtIndex(newIndex);
                              newElementProp.FindPropertyRelative("name").stringValue = type.AssemblyQualifiedName;
                              newElementProp.FindPropertyRelative("isEnabled").boolValue = true;
                              _serializedConfig.ApplyModifiedProperties();
                        });
                  }

                  menu.ShowAsContext();
            }

#endregion
      }
}