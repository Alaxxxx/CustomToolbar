using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences.Data;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences.Window
{
      sealed internal class MissingReferencesWindow : EditorWindow
      {
            private Dictionary<GameObject, List<MissingReferenceInfo>> _results;
            private Vector2 _scrollPosition;
            private readonly HashSet<string> _selectedItems = new();
            private bool _selectAll;
            private string _lastActionResult = "";
            private float _lastActionTime;
            private readonly Dictionary<GameObject, bool> _gameObjectFoldouts = new();

            public static void ShowWindow(Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  var window = GetWindow<MissingReferencesWindow>("Missing References");
                  window.minSize = new Vector2(700, 450);
                  window.Setup(results);
                  window.Show();
            }

            private void Setup(Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  var sortedResults = new Dictionary<GameObject, List<MissingReferenceInfo>>();

                  List<GameObject> sortedKeys = results.Keys.OrderBy(static go => go.name).ToList();

                  foreach (GameObject key in sortedKeys)
                  {
                        sortedResults[key] = results[key];
                  }

                  _results = sortedResults;
                  _gameObjectFoldouts.Clear();
                  _selectedItems.Clear();
                  _selectAll = false;

                  foreach (GameObject key in _results.Keys)
                  {
                        _gameObjectFoldouts[key] = true;
                  }
            }

            private void OnGUI()
            {
                  if (_results == null)
                  {
                        EditorGUILayout.LabelField("Loading...", EditorStyles.centeredGreyMiniLabel);

                        return;
                  }

                  DrawHeader();
                  DrawActionBar();
                  DrawContent();
                  DrawActionResult();
            }

            private void DrawHeader()
            {
                  EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                  int totalProblems = _results.Values.Sum(static list => list.Count);
                  int missingScripts = _results.Values.Sum(static list => list.Count(static info => info.IsScriptMissing));
                  int nullRefs = totalProblems - missingScripts;

                  string headerText = totalProblems > 0 ? $"{totalProblems} Issue(s) Found • {missingScripts} Missing Scripts • {nullRefs} Null References" : "🎉 No Missing References!";

                  EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel);
                  EditorGUILayout.EndVertical();
                  EditorGUILayout.Space(5);
            }

            private void DrawActionBar()
            {
                  if (_results.Count == 0)
                  {
                        return;
                  }

                  EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                  EditorGUI.BeginChangeCheck();
                  _selectAll = EditorGUILayout.ToggleLeft("Select All", _selectAll, GUILayout.Width(80));

                  if (EditorGUI.EndChangeCheck())
                  {
                        if (_selectAll)
                        {
                              SelectAllItems();
                        }
                        else
                        {
                              _selectedItems.Clear();
                        }
                  }

                  GUILayout.Space(10);

                  int selectedCount = _selectedItems.Count;

                  using (new EditorGUI.DisabledScope(selectedCount == 0))
                  {
                        if (GUILayout.Button($"Remove Components ({selectedCount})", EditorStyles.miniButton, GUILayout.Width(160)))
                        {
                              RemoveSelectedComponents();
                        }

                        if (GUILayout.Button($"Remove Scripts ({selectedCount})", EditorStyles.miniButton, GUILayout.Width(140)))
                        {
                              RemoveSelectedMissingScripts();
                        }
                  }

                  GUILayout.FlexibleSpace();

                  int totalNullRefs = CountNullReferences();
                  int totalMissingScripts = CountMissingScripts();

                  if (totalNullRefs > 0 && GUILayout.Button($"Remove All Null ({totalNullRefs})", EditorStyles.miniButton, GUILayout.Width(140)))
                  {
                        RemoveAllComponentsWithNullRefs();
                  }

                  if (totalMissingScripts > 0 && GUILayout.Button($"Remove All Missing ({totalMissingScripts})", EditorStyles.miniButton, GUILayout.Width(150)))
                  {
                        RemoveAllMissingScripts();
                  }

                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.Space(5);
            }

            private void DrawContent()
            {
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                  if (_results.Count == 0)
                  {
                        EditorGUILayout.HelpBox("🎉 Scene is clean! No missing references found.", MessageType.Info, true);
                  }
                  else
                  {
                        foreach (KeyValuePair<GameObject, List<MissingReferenceInfo>> kvp in _results)
                        {
                              EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                              DrawResultCard(kvp.Key, kvp.Value);
                              EditorGUILayout.EndVertical();
                              EditorGUILayout.Space(5);
                        }
                  }

                  EditorGUILayout.EndScrollView();
            }

            private void DrawActionResult()
            {
                  if (!string.IsNullOrEmpty(_lastActionResult) && Time.realtimeSinceStartup - _lastActionTime < 3f)
                  {
                        EditorGUILayout.LabelField(_lastActionResult, EditorStyles.centeredGreyMiniLabel);
                  }
            }

            private void DrawResultCard(GameObject go, List<MissingReferenceInfo> infos)
            {
                  EditorGUILayout.BeginHorizontal();

                  string objectDisplayName = $"{go.name} ({infos.Count} issue(s))";
                  _gameObjectFoldouts[go] = EditorGUILayout.Foldout(_gameObjectFoldouts[go], objectDisplayName, true);

                  GUILayout.FlexibleSpace();

                  if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(60)))
                  {
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);
                  }

                  EditorGUILayout.EndHorizontal();

                  if (_gameObjectFoldouts[go])
                  {
                        EditorGUI.indentLevel++;

                        IEnumerable<IGrouping<string, MissingReferenceInfo>> groupedByComponent = infos.GroupBy(static info => info.IsScriptMissing ? "Missing Script" : info.ComponentName);

                        foreach (IGrouping<string, MissingReferenceInfo> group in groupedByComponent)
                        {
                              if (group.Key == "Missing Script")
                              {
                                    DrawMissingScriptGroup(go);
                              }
                              else
                              {
                                    DrawNullReferenceGroup(go, group);
                              }
                        }

                        EditorGUI.indentLevel--;
                  }
            }

            private void DrawMissingScriptGroup(GameObject go)
            {
                  EditorGUILayout.BeginHorizontal();

                  string itemKey = $"{go.GetInstanceID()}_missing_script";
                  bool isSelected = _selectedItems.Contains(itemKey);

                  isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(16));
                  UpdateSelection(itemKey, isSelected);

                  EditorGUILayout.LabelField("❌ Missing Script", EditorStyles.label);

                  GUILayout.FlexibleSpace();

                  if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60)))
                  {
                        RemoveMissingScriptFromObject(go);
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void DrawNullReferenceGroup(GameObject go, IGrouping<string, MissingReferenceInfo> group)
            {
                  EditorGUILayout.LabelField($"📜 Script: {group.Key}", EditorStyles.boldLabel);

                  foreach (MissingReferenceInfo info in group)
                  {
                        EditorGUILayout.BeginHorizontal();

                        string itemKey = $"{go.GetInstanceID()}_{info.ComponentName}_{info.FieldName}";
                        bool isSelected = _selectedItems.Contains(itemKey);

                        EditorGUI.indentLevel++;
                        isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(16));
                        UpdateSelection(itemKey, isSelected);

                        EditorGUILayout.LabelField($"⚠️ Field: {info.FieldName}", EditorStyles.label);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60)))
                        {
                              RemoveComponentFromObject(go, info);
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndHorizontal();
                  }
            }

            private void UpdateSelection(string itemKey, bool isSelected)
            {
                  if (isSelected)
                  {
                        _selectedItems.Add(itemKey);
                  }
                  else
                  {
                        _selectedItems.Remove(itemKey);
                        _selectAll = false;
                  }
            }

            private void SelectAllItems()
            {
                  _selectedItems.Clear();

                  foreach (KeyValuePair<GameObject, List<MissingReferenceInfo>> kvp in _results)
                  {
                        GameObject go = kvp.Key;
                        List<MissingReferenceInfo> infos = kvp.Value;

                        bool hasMissingScript = false;

                        foreach (MissingReferenceInfo info in infos)
                        {
                              if (info.IsScriptMissing)
                              {
                                    hasMissingScript = true;

                                    break;
                              }
                        }

                        if (hasMissingScript)
                        {
                              _selectedItems.Add($"{go.GetInstanceID()}_missing_script");
                        }

                        foreach (MissingReferenceInfo info in infos)
                        {
                              if (!info.IsScriptMissing)
                              {
                                    _selectedItems.Add($"{go.GetInstanceID()}_{info.ComponentName}_{info.FieldName}");
                              }
                        }
                  }
            }

            private int CountNullReferences()
            {
                  return _results.Values.Sum(static list => list.Count(static info => !info.IsScriptMissing));
            }

            private int CountMissingScripts()
            {
                  return _results.Values.Sum(static list => list.Count(static info => info.IsScriptMissing));
            }

            private void RemoveSelectedComponents()
            {
                  int removedCount = 0;
                  var itemsToRemove = new List<string>();

                  foreach (string itemKey in _selectedItems)
                  {
                        if (!itemKey.Contains("_missing_script", StringComparison.OrdinalIgnoreCase))
                        {
                              string[] parts = itemKey.Split('_');

                              if (parts.Length >= 3)
                              {
                                    int instanceId = int.Parse(parts[0]);
#if UNITY_6000_3_OR_NEWER
                                    var go = EditorUtility.EntityIdToObject(instanceId) as GameObject;
#else
                                    var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
#endif

                                    if (go)
                                    {
                                          string componentName = parts[1];

                                          string fieldName = "";

                                          for (int i = 2; i < parts.Length; i++)
                                          {
                                                if (i > 2)
                                                {
                                                      fieldName += "_";
                                                }

                                                fieldName += parts[i];
                                          }

                                          var info = new MissingReferenceInfo
                                          {
                                                      ComponentName = componentName,
                                                      FieldName = fieldName,
                                                      IsScriptMissing = false
                                          };

                                          if (RemoveComponentFromObject(go, info))
                                          {
                                                removedCount++;
                                                itemsToRemove.Add(itemKey);
                                          }
                                    }
                              }
                        }
                  }

                  foreach (string item in itemsToRemove)
                  {
                        _selectedItems.Remove(item);
                  }

                  ShowActionResult($"✅ Removed {removedCount} component(s)");
                  RefreshResults();
            }

            private void RemoveSelectedMissingScripts()
            {
                  int fixedCount = 0;
                  var itemsToRemove = new List<string>();

                  foreach (string itemKey in _selectedItems)
                  {
                        if (itemKey.Contains("_missing_script", StringComparison.OrdinalIgnoreCase))
                        {
                              string[] parts = itemKey.Split('_');
                              int instanceId = int.Parse(parts[0]);
#if UNITY_6000_3_OR_NEWER
                              var go = EditorUtility.EntityIdToObject(instanceId) as GameObject;
#else
                              var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
#endif

                              if (go && RemoveMissingScriptFromObject(go))
                              {
                                    fixedCount++;
                                    itemsToRemove.Add(itemKey);
                              }
                        }
                  }

                  foreach (string item in itemsToRemove)
                  {
                        _selectedItems.Remove(item);
                  }

                  ShowActionResult($"✅ Removed {fixedCount} missing script(s)");
                  RefreshResults();
            }

            private void RemoveAllComponentsWithNullRefs()
            {
                  int removedCount = 0;

                  foreach (KeyValuePair<GameObject, List<MissingReferenceInfo>> kvp in _results)
                  {
                        GameObject go = kvp.Key;
                        List<MissingReferenceInfo> infos = kvp.Value;

                        var processedComponents = new HashSet<string>();

                        foreach (MissingReferenceInfo info in infos)
                        {
                              if (!info.IsScriptMissing && !processedComponents.Contains(info.ComponentName) && RemoveComponentFromObject(go, info))
                              {
                                    removedCount++;
                                    processedComponents.Add(info.ComponentName);
                              }
                        }
                  }

                  ShowActionResult($"✅ Removed {removedCount} component(s) with null references");
                  RefreshResults();
            }

            private void RemoveAllMissingScripts()
            {
                  int removedCount = 0;

                  foreach (KeyValuePair<GameObject, List<MissingReferenceInfo>> kvp in _results)
                  {
                        GameObject go = kvp.Key;
                        List<MissingReferenceInfo> infos = kvp.Value;

                        bool hasMissingScript = false;

                        foreach (MissingReferenceInfo info in infos)
                        {
                              if (info.IsScriptMissing)
                              {
                                    hasMissingScript = true;

                                    break;
                              }
                        }

                        if (hasMissingScript && RemoveMissingScriptFromObject(go))
                        {
                              removedCount++;
                        }
                  }

                  ShowActionResult($"✅ Removed missing scripts from {removedCount} object(s)");
                  RefreshResults();
            }

            private static bool RemoveComponentFromObject(GameObject go, MissingReferenceInfo info)
            {
                  try
                  {
                        MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();

                        foreach (MonoBehaviour component in components)
                        {
                              if (component && component.GetType().Name == info.ComponentName)
                              {
                                    if (EditorUtility.DisplayDialog("Remove Component", $"Remove component '{info.ComponentName}' from '{go.name}'?\n\nThis action cannot be undone.", "Remove",
                                                    "Cancel"))
                                    {
                                          DestroyImmediate(component);
                                          EditorUtility.SetDirty(go);

                                          return true;
                                    }

                                    return false;
                              }
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogWarning($"Failed to remove component {info.ComponentName} from {go.name}: {e.Message}");

                        return false;
                  }

                  return false;
            }

            private static bool RemoveMissingScriptFromObject(GameObject go)
            {
                  try
                  {
                        Component[] components = go.GetComponents<Component>();
                        int removedCount = 0;

                        for (int i = components.Length - 1; i >= 0; i--)
                        {
                              if (!components[i])
                              {
                                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                                    removedCount++;

                                    break;
                              }
                        }

                        return removedCount > 0;
                  }
                  catch (Exception e)
                  {
                        Debug.LogWarning($"Failed to remove missing script from {go.name}: {e.Message}");

                        return false;
                  }
            }

            private void ShowActionResult(string message)
            {
                  _lastActionResult = message;
                  _lastActionTime = Time.realtimeSinceStartup;
                  Repaint();
            }

            private void RefreshResults()
            {
                  EditorApplication.delayCall += () =>
                  {
                        MethodInfo scanMethod = typeof(ToolbarFindMissingReferences).GetMethod("StartScanCoroutine", BindingFlags.NonPublic | BindingFlags.Static);

                        if (scanMethod != null)
                        {
                              scanMethod.Invoke(null, null);
                        }
                        else
                        {
                              Repaint();
                        }
                  };
            }
      }
}