using OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess
{
      internal static class QuickAccessTracker
      {
            public static void StartTracking()
            {
                  Selection.selectionChanged -= OnSelectionChanged;
                  Selection.selectionChanged += OnSelectionChanged;
                  EditorSceneManager.sceneOpened -= OnSceneOpened;
                  EditorSceneManager.sceneOpened += OnSceneOpened;
            }

            public static void StopTracking()
            {
                  Selection.selectionChanged -= OnSelectionChanged;
                  EditorSceneManager.sceneOpened -= OnSceneOpened;
            }

            private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
            {
                  if (scene.path == null)
                  {
                        return;
                  }

                  var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);

                  if (sceneAsset != null)
                  {
                        QuickAccessManager.Instance.AddItem(sceneAsset);
                  }
            }

            private static void OnSelectionChanged()
            {
                  var manager = QuickAccessManager.Instance;

                  foreach (Object obj in Selection.objects)
                  {
                        if (obj == null)
                        {
                              continue;
                        }

                        manager.AddItem(obj);
                  }
            }
      }
}