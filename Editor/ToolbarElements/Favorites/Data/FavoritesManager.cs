using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      public sealed class FavoritesManager : ScriptableObject
      {
            public List<FavoriteList> allLists = new();
            public int lastUsedListIndex;

            private static FavoritesManager instance;
            public static FavoritesManager Instance
            {
                  get
                  {
                        if (instance)
                        {
                              return instance;
                        }

                        string[] guids = AssetDatabase.FindAssets("t:FavoritesManager");

                        if (guids.Length > 0)
                        {
                              string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                              instance = AssetDatabase.LoadAssetAtPath<FavoritesManager>(path);
                        }
                        else
                        {
                              instance = CreateInstance<FavoritesManager>();
                              const string directoryPath = "Assets/Settings/CustomToolbar";

                              if (!System.IO.Directory.Exists(directoryPath))
                              {
                                    System.IO.Directory.CreateDirectory(directoryPath);
                              }

                              AssetDatabase.CreateAsset(instance, $"{directoryPath}/FavoritesManager.asset");
                              AssetDatabase.SaveAssets();
                        }

                        return instance;
                  }
            }
      }
}