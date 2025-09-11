using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.Utils
{
      internal static class SceneAssetsUtils
      {
            private const string LastSceneSetupStateKey = "CustomToolbar.LastSceneSetup";

            [Serializable]
            private class SerializableSceneSetup
            {
                  public string path;
                  public bool isLoaded;
                  public bool isActive;

                  public static SerializableSceneSetup FromSceneSetup(SceneSetup setup)
                  {
                        return new SerializableSceneSetup
                        {
                                    path = setup.path,
                                    isLoaded = setup.isLoaded,
                                    isActive = setup.isActive
                        };
                  }
            }

            [Serializable]
            private class SceneSetupWrapper
            {
                  public SerializableSceneSetup[] setups;
            }

            public static void StartPlayModeFromFirstScene()
            {
                  if (EditorApplication.isPlaying)
                  {
                        return;
                  }

                  if (EditorBuildSettings.scenes.Length == 0)
                  {
                        Debug.LogWarning("Cannot start from first scene: No scenes in Build Settings.");

                        return;
                  }

                  if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                  {
                        SceneSetup[] currentSceneSetup = EditorSceneManager.GetSceneManagerSetup();

                        var wrapper = new SceneSetupWrapper
                        {
                                    setups = currentSceneSetup.Select(SerializableSceneSetup.FromSceneSetup).ToArray()
                        };

                        string jsonSetup = JsonUtility.ToJson(wrapper);

                        SessionState.SetString(LastSceneSetupStateKey, jsonSetup);

                        string firstScenePath = EditorBuildSettings.scenes[0].path;
                        EditorSceneManager.OpenScene(firstScenePath);
                        EditorApplication.isPlaying = true;
                  }
            }

            public static void RestoreSceneAfterPlay()
            {
                  string jsonSetup = SessionState.GetString(LastSceneSetupStateKey, string.Empty);

                  if (!string.IsNullOrEmpty(jsonSetup))
                  {
                        var wrapper = JsonUtility.FromJson<SceneSetupWrapper>(jsonSetup);
                        SerializableSceneSetup[] serializableSetups = wrapper.setups;

                        if (serializableSetups is { Length: > 0 })
                        {
                              foreach (SerializableSceneSetup setup in serializableSetups)
                              {
                                    if (!File.Exists(setup.path))
                                    {
                                          Debug.LogWarning($"[CustomToolbar] Could not restore scene setup. File not found at path: {setup.path}");
                                          SessionState.EraseString(LastSceneSetupStateKey);

                                          return;
                                    }
                              }

                              SceneSetup[] sceneSetupsToRestore = serializableSetups.Select(static s => new SceneSetup
                                                                                    {
                                                                                                path = s.path,
                                                                                                isLoaded = s.isLoaded,
                                                                                                isActive = s.isActive
                                                                                    })
                                                                                    .ToArray();

                              EditorSceneManager.RestoreSceneManagerSetup(sceneSetupsToRestore);
                        }

                        SessionState.EraseString(LastSceneSetupStateKey);
                  }
            }
      }
}