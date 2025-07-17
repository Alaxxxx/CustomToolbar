using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomToolbar.Editor.Utils
{
      internal static class SceneAssetsUtils
      {
            private const string LastActiveSceneStateKey = "CustomToolbar.LastActiveScene";

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
                        SessionState.SetString(LastActiveSceneStateKey, SceneManager.GetActiveScene().path);
                        string firstScenePath = EditorBuildSettings.scenes[0].path;
                        EditorSceneManager.OpenScene(firstScenePath);
                        EditorApplication.isPlaying = true;
                  }
            }

            public static void RestoreSceneAfterPlay()
            {
                  string sceneToRestore = SessionState.GetString(LastActiveSceneStateKey, string.Empty);

                  if (!string.IsNullOrEmpty(sceneToRestore))
                  {
                        EditorSceneManager.OpenScene(sceneToRestore);
                        SessionState.EraseString(LastActiveSceneStateKey);
                  }
            }
      }
}