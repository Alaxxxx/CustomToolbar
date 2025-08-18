using System;
using System.Collections.Generic;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarSceneSelection : BaseToolbarElement
      {
            private GUIContent _buttonContent;
            private readonly List<string> _scenePaths = new();
            private Dictionary<string, int> _buildSceneData;

            protected override string Name => "Scene Selection";
            protected override string Tooltip => "Select a scene from the 'Assets/' folder.";

            public override void OnInit()
            {
                  this.Width = 120;

                  EditorSceneManager.sceneOpened -= OnSceneChanged;
                  EditorSceneManager.sceneOpened += OnSceneChanged;

                  EditorBuildSettings.sceneListChanged -= RefreshScenesList;
                  EditorBuildSettings.sceneListChanged += RefreshScenesList;

                  EditorApplication.projectChanged -= RefreshScenesList;
                  EditorApplication.projectChanged += RefreshScenesList;

                  RefreshScenesList();

                  EditorApplication.update += ForceInitialRefresh;
            }

            private void ForceInitialRefresh()
            {
                  EditorApplication.update -= ForceInitialRefresh;
                  RefreshScenesList();
            }

            private void OnSceneChanged(Scene scene, OpenSceneMode mode) => RefreshScenesList();

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                  {
                        if (_buttonContent == null)
                        {
                              return;
                        }

                        if (EditorGUILayout.DropdownButton(_buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                        {
                              BuildSceneMenu().ShowAsContext();
                        }
                  }
            }

            private GenericMenu BuildSceneMenu()
            {
                  var menu = new GenericMenu();

                  if (_scenePaths.Count == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No scenes found in project"));

                        return menu;
                  }

                  var ignoredScenes = new List<string> { "Basic", "Standard" };
                  var buildScenes = new List<(string path, int buildIndex)>();
                  var otherScenes = new List<string>();

                  foreach (string path in _scenePaths)
                  {
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                        if (ignoredScenes.Contains(sceneName))
                        {
                              continue;
                        }

                        if (_buildSceneData.TryGetValue(path, out int buildIndex))
                        {
                              buildScenes.Add((path, buildIndex));
                        }
                        else
                        {
                              otherScenes.Add(path);
                        }
                  }

                  buildScenes.Sort(static (a, b) => a.buildIndex.CompareTo(b.buildIndex));

                  foreach ((string path, int buildIndex) in buildScenes)
                  {
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                        string menuPath = $"{sceneName}   [{buildIndex}]";
                        menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
                  }

                  if (buildScenes.Count > 0 && otherScenes.Count > 0)
                  {
                        menu.AddSeparator("");
                  }

                  foreach (string path in otherScenes)
                  {
                        string menuPath = path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? path["Assets/".Length..] : path;
                        menuPath = menuPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ? menuPath[..^".unity".Length] : menuPath;

                        menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
                  }

                  return menu;
            }

            private void RefreshScenesList()
            {
                  _scenePaths.Clear();

                  _buildSceneData = new Dictionary<string, int>();

                  for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                  {
                        if (!string.IsNullOrEmpty(EditorBuildSettings.scenes[i].path))
                        {
                              _buildSceneData[EditorBuildSettings.scenes[i].path] = i;
                        }
                  }

                  string[] allSceneGuids = AssetDatabase.FindAssets("t:scene", new[] { "Assets" });

                  foreach (string guid in allSceneGuids)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                        {
                              _scenePaths.Add(path);
                        }
                  }

                  _scenePaths.Sort(static (pathA, pathB) =>
                  {
                        int depthA = pathA.Count(static c => c == '/');
                        int depthB = pathB.Count(static c => c == '/');

                        return depthA != depthB ? depthA.CompareTo(depthB) : string.Compare(pathA, pathB, StringComparison.Ordinal);
                  });

                  Scene activeScene = SceneManager.GetActiveScene();
                  Texture sceneIcon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
                  _buttonContent = new GUIContent(activeScene.name, sceneIcon, Tooltip);
            }

            private static void OpenScene(string path)
            {
                  if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                  {
                        return;
                  }

                  EditorSceneManager.OpenScene(path);
            }
      }
}