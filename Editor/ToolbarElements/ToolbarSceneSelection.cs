using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarSceneSelection : BaseToolbarElement
      {
            public override string NameInList => "[Dropdown] Scene selection";
            public override int SortingGroup => 2;

            [SerializeField] private bool showSceneFolder = true;

            private SceneData[] scenesPopupDisplay;
            private string[] scenesPath;
            private string[] scenesBuildPath;
            private int selectedSceneIndex;

            private List<SceneData> toDisplay = new();
            private string[] sceneGuids;
            private Scene activeScene;
            private int usedIds;
            private string name;
            private GUIContent content;
            private bool isPlaceSeparator;

            public override void Init()
            {
                  RefreshScenesList();
                  EditorSceneManager.sceneOpened -= HandleSceneOpened;
                  EditorSceneManager.sceneOpened += HandleSceneOpened;
            }

            protected override void OnDrawInList(Rect position)
            {
                  position.width = 200.0f;
                  showSceneFolder = EditorGUI.Toggle(position, "Group by folders", showSceneFolder);
            }

            protected override void OnDrawInToolbar()
            {
                  EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                  DrawSceneDropdown();
                  EditorGUI.EndDisabledGroup();
            }

            private void DrawSceneDropdown()
            {
                  selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, scenesPopupDisplay.Select(e => e.popupDisplay).ToArray(),
                              GUILayout.Width(widthInToolbar));

                  if (GUI.changed && 0 <= selectedSceneIndex && selectedSceneIndex < scenesPopupDisplay.Length)
                  {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                              foreach (var scenePath in scenesPath)
                              {
                                    if ((scenePath) == scenesPopupDisplay[selectedSceneIndex].path)
                                    {
                                          EditorSceneManager.OpenScene(scenePath);

                                          break;
                                    }
                              }
                        }
                  }
            }

            private void RefreshScenesList()
            {
                  InitScenesData();

                  // Scenes in build settings
                  for (int i = 0; i < scenesBuildPath.Length; ++i)
                  {
                        AddScene(scenesBuildPath[i]);
                  }

                  // Scenes on Assets/Scenes/
                  isPlaceSeparator = false;

                  for (int i = 0; i < scenesPath.Length; ++i)
                  {
                        if (scenesPath[i].Contains("Assets/Scenes"))
                        {
                              PlaceSeperatorIfNeeded();
                              AddScene(scenesPath[i]);
                        }
                  }

                  // Scenes on Plugins/Plugins/
                  // Consider them as demo scenes from plugins
                  isPlaceSeparator = false;

                  for (int i = 0; i < scenesPath.Length; ++i)
                  {
                        if (scenesPath[i].Contains("Assets/Plugins/"))
                        {
                              PlaceSeperatorIfNeeded();
                              AddScene(scenesPath[i], "Plugins demo");
                        }
                  }

                  // All other scenes.
                  isPlaceSeparator = false;

                  for (int i = 0; i < scenesPath.Length; ++i)
                  {
                        PlaceSeperatorIfNeeded();
                        AddScene(scenesPath[i]);
                  }

                  scenesPopupDisplay = toDisplay.ToArray();
            }

            private void AddScene(string path, string prefix = null, string overrideName = null)
            {
                  if (!path.Contains(".unity"))
                        path += ".unity";

                  if (toDisplay.Find(data => path == data.path) != null)
                        return;

                  if (!string.IsNullOrEmpty(overrideName))
                  {
                        name = overrideName;
                  }
                  else
                  {
                        if (showSceneFolder)
                        {
                              string folderName = Path.GetFileName(Path.GetDirectoryName(path));
                              name = $"{folderName}/{GetSceneName(path)}";
                        }
                        else
                        {
                              name = GetSceneName(path);
                        }
                  }

                  if (!string.IsNullOrEmpty(prefix))
                        name = $"{prefix}/{name}";

                  if (scenesBuildPath.Contains(path))
                        content = new GUIContent(name, EditorGUIUtility.Load("BuildSettings.Editor.Small") as Texture, "Open scene");
                  else
                        content = new GUIContent(name, "Open scene");

                  toDisplay.Add(new SceneData()
                  {
                              path = path,
                              popupDisplay = content,
                  });

                  if (selectedSceneIndex == -1 && GetSceneName(path) == activeScene.name)
                        selectedSceneIndex = usedIds;
                  ++usedIds;
            }

            private void PlaceSeperatorIfNeeded()
            {
                  if (!isPlaceSeparator)
                  {
                        isPlaceSeparator = true;
                        PlaceSeperator();
                  }
            }

            private void PlaceSeperator()
            {
                  toDisplay.Add(new SceneData()
                  {
                              path = "\0",
                              popupDisplay = new GUIContent("\0"),
                  });
                  ++usedIds;
            }

            private void HandleSceneOpened(Scene scene, OpenSceneMode mode)
            {
                  RefreshScenesList();
            }

            private string GetSceneName(string path)
            {
                  path = path.Replace(".unity", "");

                  return Path.GetFileName(path);
            }

            private void InitScenesData()
            {
                  toDisplay.Clear();
                  selectedSceneIndex = -1;
                  scenesBuildPath = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

                  sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });
                  scenesPath = new string[sceneGuids.Length];

                  for (int i = 0; i < scenesPath.Length; ++i)
                        scenesPath[i] = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

                  activeScene = SceneManager.GetActiveScene();
                  usedIds = 0;
            }

            private class SceneData
            {
                  public string path;
                  public GUIContent popupDisplay;
            }
      }
}