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
            private GUIContent buttonContent;
            private readonly List<string> scenePaths = new();
            private Dictionary<string, int> buildSceneData;

            protected override string Name => "Scene Selection";
            protected override string Tooltip => "Select a scene from the 'Assets/Scenes' folder.";

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
                        if (buttonContent == null)
                        {
                              return;
                        }

                        if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                        {
                              BuildSceneMenu().ShowAsContext();
                        }
                  }
            }

            private GenericMenu BuildSceneMenu()
            {
                  var menu = new GenericMenu();

                  if (scenePaths.Count == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No scenes found in project"));

                        return menu;
                  }

                  var ignoredScenes = new List<string> { "Basic", "Standard" };

                  foreach (string path in scenePaths)
                  {
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                        if (ignoredScenes.Contains(sceneName))
                        {
                              continue;
                        }

                        string menuPath = path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? path.Substring("Assets/".Length) : path;
                        menuPath = menuPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ? menuPath.Substring(0, menuPath.Length - ".unity".Length) : menuPath;

                        if (buildSceneData.TryGetValue(path, out int buildIndex))
                        {
                              menuPath = $"{menuPath}   [{buildIndex}]";
                        }

                        menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
                  }

                  return menu;
            }

            private void RefreshScenesList()
            {
                  scenePaths.Clear();

                  buildSceneData = new Dictionary<string, int>();

                  for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                  {
                        if (!string.IsNullOrEmpty(EditorBuildSettings.scenes[i].path))
                        {
                              buildSceneData[EditorBuildSettings.scenes[i].path] = i;
                        }
                  }

                  string[] allSceneGuids = AssetDatabase.FindAssets("t:scene");

                  foreach (string guid in allSceneGuids)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        scenePaths.Add(path);
                  }

                  scenePaths.Sort(static (pathA, pathB) =>
                  {
                        int depthA = pathA.Count(static c => c == '/');
                        int depthB = pathB.Count(static c => c == '/');

                        return depthA != depthB ? depthA.CompareTo(depthB) : string.Compare(pathA, pathB, StringComparison.Ordinal);
                  });

                  Scene activeScene = SceneManager.GetActiveScene();
                  Texture sceneIcon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
                  buttonContent = new GUIContent(activeScene.name, sceneIcon, Tooltip);
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