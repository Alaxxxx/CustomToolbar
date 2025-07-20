using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Data
{
      public sealed class SceneBookmarksManager : ScriptableObject
      {
            private const string AssetPath = "Assets/Settings/CustomToolbarSceneBookmarks.asset";

            public List<SceneBookmark> bookmarks = new();

            private static SceneBookmarksManager instance;
            public static SceneBookmarksManager Instance
            {
                  get
                  {
                        if (instance == null)
                        {
                              instance = AssetDatabase.LoadAssetAtPath<SceneBookmarksManager>(AssetPath);

                              if (instance == null)
                              {
                                    instance = CreateInstance<SceneBookmarksManager>();
                                    Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
                                    AssetDatabase.CreateAsset(instance, AssetPath);
                                    AssetDatabase.SaveAssets();
                              }
                        }

                        return instance;
                  }
            }

            public void Save()
            {
                  EditorUtility.SetDirty(this);
                  AssetDatabase.SaveAssets();
            }
      }
}