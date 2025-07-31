using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpalStudio.CustomToolbar.Editor.Core.Data;
using OpalStudio.CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.Settings
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

            // Holds the currently selected object in the settings GUI.
            private object _selectedObject;
            private object _lastSelectedObject;

            // Scroll position for the left panel, used to maintain the scroll state during GUI rendering.
            private Vector2 _leftPanelScrollPos;

            // Set of expanded folder names in the toolbox shortcuts section.
            private readonly HashSet<string> _expandedFolders = new();

            // Search text for filtering groups and shortcuts in the left panel.
            private string _searchText = "";

            // Flag to track if there are unsaved changes in the settings.
            private bool _hasUnsavedChanges;

            // Fields for the method selection editor in the right panel.
            private MonoScript _selectedScript;
            private int _selectedMethodIndex;
            private string[] _methodNames = Array.Empty<string>();
            private object[] _methodParameters = Array.Empty<object>();

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

                  // Update the serialized object to reflect any changes made in the GUI
                  _serializedConfig.Update();

                  // Begin change check to track modifications in the GUI
                  EditorGUI.BeginChangeCheck();

                  // Draw the main layout with two vertical panels: left for groups and shortcuts, right for selected item properties
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

                  // Check if any changes were made in the GUI
                  if (EditorGUI.EndChangeCheck())
                  {
                        _hasUnsavedChanges = true;
                  }

                  // Apply any changes made to the serialized object back to the configuration asset
                  _serializedConfig.ApplyModifiedProperties();
            }

            public void Dispose()
            {
                  _serializedConfig?.Dispose();
            }

#region Left Panel

            private void DrawLeftPanel()
            {
                  _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);

                  SerializedProperty groupsProperty = _serializedConfig.FindProperty("groups");
                  EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
                  var leftGroups = new List<(SerializedProperty prop, int originalIndex)>();
                  var rightGroups = new List<(SerializedProperty prop, int originalIndex)>();

                  for (int i = 0; i < groupsProperty.arraySize; i++)
                  {
                        SerializedProperty group = groupsProperty.GetArrayElementAtIndex(i);

                        if (group.FindPropertyRelative("groupName").stringValue.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                              if ((ToolbarSide)group.FindPropertyRelative("side").enumValueIndex == ToolbarSide.Left)
                              {
                                    leftGroups.Add((group, i));
                              }
                              else
                              {
                                    rightGroups.Add((group, i));
                              }
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

                  GUILayout.FlexibleSpace();

                  using (new EditorGUI.DisabledScope(!_hasUnsavedChanges))
                  {
                        if (GUILayout.Button("Save and Recompile", GUILayout.Height(30)))
                        {
                              AssetDatabase.SaveAssets();
                              CompilationPipeline.RequestScriptCompilation();
                              _hasUnsavedChanges = false;
                        }
                  }
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
                  var visibleShortcuts = new HashSet<string>();

                  if (!string.IsNullOrEmpty(_searchText))
                  {
                        for (int i = 0; i < shortcutsProperty.arraySize; i++)
                        {
                              SerializedProperty shortcut = shortcutsProperty.GetArrayElementAtIndex(i);

                              if (shortcut.FindPropertyRelative("displayName").stringValue.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                              {
                                    visibleShortcuts.Add(shortcut.propertyPath);
                              }
                        }
                  }

                  for (int i = 0; i < shortcutsProperty.arraySize; i++)
                  {
                        SerializedProperty shortcut = shortcutsProperty.GetArrayElementAtIndex(i);

                        if (!string.IsNullOrEmpty(_searchText) && !visibleShortcuts.Contains(shortcut.propertyPath))
                        {
                              continue;
                        }

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

                        bool newIsExpanded = !string.IsNullOrEmpty(_searchText) || EditorGUILayout.Foldout(isExpanded, folderName, true, EditorStyles.foldout);

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
                        DrawAdvancedShortcutEditor(selectedProp);
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

             private void DrawAdvancedShortcutEditor(SerializedProperty shortcutProp)
            {
                  if (_selectedObject != _lastSelectedObject)
                  {
                        if (_selectedObject is SerializedProperty { type: "ToolboxShortcut" } prop)
                        {
                              string path = prop.FindPropertyRelative("menuItemPath").stringValue;

                              if (path.StartsWith("method:", StringComparison.Ordinal))
                              {
                                    ParseAndSetupMethodEditor(path);
                              }
                        }

                        _lastSelectedObject = _selectedObject;
                  }


                  EditorGUILayout.BeginHorizontal();

                  EditorGUILayout.PropertyField(shortcutProp.FindPropertyRelative("displayName"),
                              new GUIContent("Display Name", "The name that will appear in the Toolbox menu."), true);
                  SerializedProperty enabledProp = shortcutProp.FindPropertyRelative("isEnabled");
                  Color originalBgColor = GUI.backgroundColor;
                  GUI.backgroundColor = enabledProp.boolValue ? new Color(0.4f, 1f, 0.6f, 1f) : new Color(1f, 0.6f, 0.6f, 1f);
                  enabledProp.boolValue = GUILayout.Toggle(enabledProp.boolValue, "Enabled", EditorStyles.toolbarButton, GUILayout.Width(70));
                  GUI.backgroundColor = originalBgColor;
                  EditorGUILayout.EndHorizontal();

                  EditorGUILayout.PropertyField(shortcutProp.FindPropertyRelative("subMenuPath"),
                              new GUIContent("Menu (Optional)", "Organize shortcuts into sub-menus using a path (e.g., 'Creation/Prefabs')."));
                  EditorGUILayout.Space();

                  SerializedProperty pathProp = shortcutProp.FindPropertyRelative("menuItemPath");
                  string currentPath = pathProp.stringValue;

                  string[] actionTypes = { "Window", "Project Asset", "Project Folder", "URL", "GameObject", "Method" };
                  string[] prefixes = { "", "asset:", "folder:", "url:", "select:", "method:" };

                  int currentTypeIndex = 0;

                  for (int i = 1; i < prefixes.Length; i++)
                  {
                        if (currentPath.StartsWith(prefixes[i], StringComparison.OrdinalIgnoreCase))
                        {
                              currentTypeIndex = i;

                              break;
                        }
                  }

                  int newTypeIndex = EditorGUILayout.Popup("Action Type", currentTypeIndex, actionTypes);

                  if (newTypeIndex != currentTypeIndex)
                  {
                        pathProp.stringValue = prefixes[newTypeIndex];

                        if (newTypeIndex != 5)
                        {
                              _selectedScript = null;
                              _methodParameters = Array.Empty<object>();
                              _methodNames = Array.Empty<string>();
                        }
                  }

                  string value = currentPath.Contains(":", StringComparison.OrdinalIgnoreCase) ? currentPath.Substring(currentPath.IndexOf(':') + 1) : currentPath;

                  switch (newTypeIndex)
                  {
                        case 1:
                              var asset = AssetDatabase.LoadAssetAtPath<Object>(value);
                              Object newAsset = EditorGUILayout.ObjectField("Asset", asset, typeof(Object), false);

                              if (newAsset != asset)
                              {
                                    pathProp.stringValue = "asset:" + AssetDatabase.GetAssetPath(newAsset);
                              }

                              break;
                        case 2:
                              var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(value);
                              Object newFolder = EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);

                              if (newFolder != folder)
                              {
                                    pathProp.stringValue = "folder:" + AssetDatabase.GetAssetPath(newFolder);
                              }

                              break;
                        case 3:

                              string newUrl = EditorGUILayout.TextField("URL", value);

                              if (newUrl != value)
                              {
                                    pathProp.stringValue = "url:" + newUrl;
                              }

                              break;
                        case 4:
                              GameObject sceneObject = null;

                              if (!string.IsNullOrEmpty(value))
                              {
                                    sceneObject = GameObject.Find(value);
                              }

                              var newSceneObject = (GameObject)EditorGUILayout.ObjectField("Scene GameObject", sceneObject, typeof(GameObject), true);

                              if (newSceneObject != sceneObject)
                              {
                                    pathProp.stringValue = "select:" + GetGameObjectPath(newSceneObject);
                              }

                              break;
                        case 5:
                              DrawMethodEditor(pathProp);

                              break;
                        default:
                              EditorGUILayout.LabelField(new GUIContent("Menu Path", "The path to the window in the main menu."));
                              EditorGUILayout.BeginHorizontal();

                              EditorGUILayout.PropertyField(pathProp, GUIContent.none);

                              if (GUILayout.Button("Browse...", GUILayout.Width(80)))
                              {
                                    MenuItemBrowser.Show(selectedPath =>
                                    {
                                          pathProp.stringValue = selectedPath;

                                          _serializedConfig.ApplyModifiedProperties();
                                    });
                              }

                              EditorGUILayout.EndHorizontal();

                              break;
                  }

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

            private void ParseAndSetupMethodEditor(string path)
            {
                  _selectedScript = null;
                  _methodParameters = Array.Empty<object>();
                  _methodNames = Array.Empty<string>();
                  _selectedMethodIndex = -1;

                  try
                  {
                        string methodData = path.Substring("method:".Length);
                        string[] parts = methodData.Split(new[] { '|' }, 2);
                        string methodPath = parts[0];
                        string paramsString = parts.Length > 1 ? parts[1] : null;

                        int lastDot = methodPath.LastIndexOf('.');

                        if (lastDot <= 0)
                        {
                              return;
                        }

                        string className = methodPath.Substring(0, lastDot);
                        string methodName = methodPath.Substring(lastDot + 1);

                        string[] guids = AssetDatabase.FindAssets($"t:MonoScript {className.Split('.').Last()}");

                        foreach (string guid in guids)
                        {
                              string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                              var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                              if (script != null && script.GetClass() != null && script.GetClass().FullName == className)
                              {
                                    _selectedScript = script;

                                    break;
                              }
                        }

                        if (_selectedScript == null)
                        {
                              return;
                        }

                        Type scriptClass = _selectedScript.GetClass();

                        if (scriptClass == null)
                        {
                              return;
                        }

                        MethodInfo[] methods = scriptClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
                        _methodNames = methods.Select(static m => m.Name).ToArray();
                        _selectedMethodIndex = Array.FindIndex(_methodNames, name => name == methodName);

                        if (_selectedMethodIndex == -1)
                        {
                              return;
                        }

                        MethodInfo selectedMethod = methods[_selectedMethodIndex];
                        ParameterInfo[] paramInfos = selectedMethod.GetParameters();
                        string[] stringArgs = !string.IsNullOrEmpty(paramsString) ? paramsString.Split(',') : Array.Empty<string>();

                        if (paramInfos.Length == stringArgs.Length)
                        {
                              _methodParameters = new object[paramInfos.Length];

                              for (int i = 0; i < paramInfos.Length; i++)
                              {
                                    Type paramType = paramInfos[i].ParameterType;
                                    string stringValue = stringArgs[i].Trim();

                                    _methodParameters[i] = paramType.IsEnum
                                                ? Enum.Parse(paramType, stringValue)
                                                : Convert.ChangeType(stringValue, paramType, System.Globalization.CultureInfo.InvariantCulture);
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Debug.LogWarning($"[CustomToolbar] Could not parse method shortcut: {path}\n{ex.Message}");

                        _selectedScript = null;
                        _methodParameters = Array.Empty<object>();
                        _methodNames = Array.Empty<string>();
                  }
            }

            private static string GetGameObjectPath(GameObject obj)
            {
                  if (obj == null)
                  {
                        return "";
                  }

                  string path = "/" + obj.name;

                  while (obj.transform.parent != null)
                  {
                        obj = obj.transform.parent.gameObject;
                        path = "/" + obj.name + path;
                  }

                  return path;
            }

            private void DrawMethodEditor(SerializedProperty pathProp)
            {
                  EditorGUILayout.BeginVertical(GUI.skin.box);

                  _selectedScript = (MonoScript)EditorGUILayout.ObjectField("Script File", _selectedScript, typeof(MonoScript), false);

                  MethodInfo[] methods = Array.Empty<MethodInfo>();

                  if (_selectedScript != null)
                  {
                        Type scriptClass = _selectedScript.GetClass();

                        if (scriptClass != null)
                        {
                              methods = scriptClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
                              _methodNames = methods.Select(static m => m.Name).ToArray();
                        }
                  }

                  if (methods.Length == 0)
                  {
                        EditorGUILayout.HelpBox("Drop a script file above. Only public static methods will be shown.", MessageType.Info);
                        EditorGUILayout.EndVertical();

                        return;
                  }

                  _selectedMethodIndex = EditorGUILayout.Popup("Method", _selectedMethodIndex, _methodNames);

                  if (_selectedMethodIndex >= methods.Length)
                  {
                        _selectedMethodIndex = 0;
                  }

                  MethodInfo selectedMethod = methods[_selectedMethodIndex];
                  ParameterInfo[] parameters = selectedMethod.GetParameters();

                  if (parameters.Length > 0)
                  {
                        EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

                        if (_methodParameters.Length != parameters.Length)
                        {
                              _methodParameters = new object[parameters.Length];
                        }

                        for (int i = 0; i < parameters.Length; i++)
                        {
                              ParameterInfo p = parameters[i];

                              EditorGUILayout.BeginHorizontal();
                              EditorGUILayout.LabelField(p.Name, GUILayout.Width(100));

                              try
                              {
                                    if (p.ParameterType == typeof(string))
                                    {
                                          _methodParameters[i] = EditorGUILayout.TextField((string)_methodParameters[i]);
                                    }
                                    else if (p.ParameterType == typeof(int))
                                    {
                                          _methodParameters[i] = EditorGUILayout.IntField((int)Convert.ChangeType(_methodParameters[i], typeof(int)));
                                    }
                                    else if (p.ParameterType == typeof(float))
                                    {
                                          _methodParameters[i] = EditorGUILayout.FloatField((float)Convert.ChangeType(_methodParameters[i], typeof(float)));
                                    }
                                    else if (p.ParameterType == typeof(bool))
                                    {
                                          _methodParameters[i] = EditorGUILayout.Toggle((bool)Convert.ChangeType(_methodParameters[i], typeof(bool)));
                                    }
                                    else if (p.ParameterType.IsEnum)
                                    {
                                          _methodParameters[i] = EditorGUILayout.EnumPopup((Enum)Convert.ChangeType(_methodParameters[i], p.ParameterType));
                                    }
                                    else
                                    {
                                          EditorGUILayout.LabelField($"Type '{p.ParameterType.Name}' not supported.");
                                    }
                              }
                              catch (Exception)
                              {
                                    _methodParameters[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                              }

                              EditorGUILayout.EndHorizontal();
                        }
                  }

                  string finalPath;

                  if (parameters.Length > 0)
                  {
                        string parametersString = string.Join(",", _methodParameters.Select(static p => p?.ToString() ?? ""));
                        finalPath = $"method:{selectedMethod.DeclaringType!.FullName}.{selectedMethod.Name}|{parametersString}";
                  }
                  else
                  {
                        finalPath = $"method:{selectedMethod.DeclaringType!.FullName}.{selectedMethod.Name}";
                  }

                  if (pathProp.stringValue != finalPath)
                  {
                        pathProp.stringValue = finalPath;
                  }

                  EditorGUILayout.EndVertical();

                  EditorGUILayout.HelpBox("Generated path: " + finalPath, MessageType.None);
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
                  var currentSide = (ToolbarSide)sideProp.enumValueIndex;
                  string switchButtonText = $"Move to {(currentSide == ToolbarSide.Left ? "Right" : "Left")} Side";

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
                  var usedElementNames = new HashSet<string>();
                  SerializedProperty groupsProperty = _serializedConfig.FindProperty("groups");

                  for (int i = 0; i < groupsProperty.arraySize; i++)
                  {
                        SerializedProperty elements = groupsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("elements");

                        for (int j = 0; j < elements.arraySize; j++)
                        {
                              usedElementNames.Add(elements.GetArrayElementAtIndex(j).FindPropertyRelative("name").stringValue);
                        }
                  }

                  var menu = new GenericMenu();
                  TypeCache.TypeCollection elementTypes = TypeCache.GetTypesDerivedFrom<BaseToolbarElement>();

                  foreach (Type type in elementTypes.Where(static t => !t.IsAbstract))
                  {
                        if (type == typeof(ToolbarSpace))
                        {
                              continue;
                        }

                        bool isUsed = usedElementNames.Contains(type.AssemblyQualifiedName);

                        var content = new GUIContent(type.Name.Replace("Toolbar", "", StringComparison.Ordinal));

                        if (isUsed)
                        {
                              content.text += " (Used)";
                              menu.AddDisabledItem(content);
                        }
                        else
                        {
                              menu.AddItem(content, false, () =>
                              {
                                    int newIndex = elementsProperty.arraySize;
                                    elementsProperty.InsertArrayElementAtIndex(newIndex);
                                    SerializedProperty newElementProp = elementsProperty.GetArrayElementAtIndex(newIndex);
                                    newElementProp.FindPropertyRelative("name").stringValue = type.AssemblyQualifiedName;
                                    newElementProp.FindPropertyRelative("isEnabled").boolValue = true;
                                    _serializedConfig.ApplyModifiedProperties();
                              });
                        }
                  }

                  menu.ShowAsContext();
            }

#endregion
      }
}