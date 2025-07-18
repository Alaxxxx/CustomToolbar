using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarSceneSelection : BaseToolbarElement
      {
            private GUIContent mButtonContent;
            private readonly List<string> mScenePaths = new();
            private Dictionary<string, int> mBuildSceneData;

            public override string Name => "Scene Selection";
            public override string Tooltip => "Select a scene from the 'Assets/Scenes' folder.";

            public override void OnInit()
            {
                  Width = 120;

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
                        if (mButtonContent == null)
                        {
                              return;
                        }

                        if (EditorGUILayout.DropdownButton(mButtonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(Width)))
                        {
                              BuildSceneMenu().ShowAsContext();
                        }
                  }
            }

            private GenericMenu BuildSceneMenu()
            {
                  var menu = new GenericMenu();

                  if (mScenePaths.Count == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No scenes found in Assets/Scenes"));

                        return menu;
                  }

                  foreach (string path in mScenePaths)
                  {
                        string menuPath = path["Assets/Scenes/".Length..].Replace(".unity", "", StringComparison.Ordinal);

                        if (mBuildSceneData.TryGetValue(path, out int buildIndex))
                        {
                              menuPath = $"{menuPath}   [{buildIndex}]";
                        }

                        menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
                  }

                  return menu;
            }

            private void RefreshScenesList()
            {
                  mScenePaths.Clear();

                  mBuildSceneData = new Dictionary<string, int>();

                  for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                  {
                        if (!string.IsNullOrEmpty(EditorBuildSettings.scenes[i].path))
                        {
                              mBuildSceneData[EditorBuildSettings.scenes[i].path] = i;
                        }
                  }

                  string[] allSceneGuids = AssetDatabase.FindAssets("t:scene");

                  foreach (string guid in allSceneGuids)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        if (path.StartsWith("Assets/Scenes/", StringComparison.Ordinal))
                        {
                              mScenePaths.Add(path);
                        }
                  }

                  mScenePaths.Sort(static (pathA, pathB) =>
                  {
                        int depthA = pathA.Count(static c => c == '/');
                        int depthB = pathB.Count(static c => c == '/');

                        if (depthA != depthB)
                        {
                              return depthA.CompareTo(depthB);
                        }

                        return string.Compare(pathA, pathB, StringComparison.Ordinal);
                  });

                  Scene activeScene = SceneManager.GetActiveScene();
                  mButtonContent = new GUIContent(activeScene.name, Tooltip);
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