using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Data
{
      public sealed class SceneBookmarksManager : ScriptableObject
      {
            private const string AssetPath = "Assets/Settings/CustomToolbar/SceneBookmarks.asset";

            [SerializeField]
            private List<SceneBookmarkData> allScenesData = new();

            [SerializeField]
            private List<SceneBookmark> legacyBookmarks = new();

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
                                    Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
                                    AssetDatabase.CreateAsset(instance, AssetPath);
                                    AssetDatabase.SaveAssets();
                              }
                              else
                              {
                                    instance.MigrateLegacyBookmarks();
                              }
                        }

                        return instance;
                  }
            }

            private SceneBookmarkData GetCurrentSceneData()
            {
                  string currentSceneGuid = GetCurrentSceneGuid();

                  if (string.IsNullOrEmpty(currentSceneGuid))
                  {
                        return null;
                  }

                  SceneBookmarkData sceneData = allScenesData.Find(data => data.sceneGuid == currentSceneGuid);

                  if (sceneData == null)
                  {
                        sceneData = new SceneBookmarkData(currentSceneGuid);
                        allScenesData.Add(sceneData);
                  }

                  return sceneData;
            }

            public List<SceneBookmark> GetCurrentSceneBookmarks()
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  return sceneData?.bookmarks ?? new List<SceneBookmark>();
            }

            public List<BookmarkGroup> GetCurrentSceneGroups()
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  return sceneData?.groups ?? new List<BookmarkGroup>();
            }

            public void AddBookmark(SceneBookmark bookmark)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData != null)
                  {
                        sceneData.bookmarks.Add(bookmark);
                        Save();
                  }
            }

            public void RemoveBookmark(SceneBookmark bookmark)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData != null)
                  {
                        sceneData.bookmarks.Remove(bookmark);
                        Save();
                  }
            }

            public void CreateGroup(string groupName)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData != null)
                  {
                        var group = new BookmarkGroup(groupName);
                        sceneData.groups.Add(group);
                        Save();
                  }
            }

            public void RemoveGroup(BookmarkGroup group)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData != null)
                  {
                        foreach (SceneBookmark bookmark in sceneData.bookmarks.Where(b => b.groupId == group.id))
                        {
                              bookmark.groupId = "";
                        }

                        sceneData.groups.Remove(group);
                        Save();
                  }
            }

            public void MoveBookmarkToGroup(SceneBookmark bookmark, string groupId)
            {
                  bookmark.groupId = groupId;
                  Save();
            }

            public void MoveBookmark(SceneBookmark bookmark, int direction)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData == null)
                  {
                        return;
                  }

                  List<SceneBookmark> bookmarks = sceneData.bookmarks;
                  int index = bookmarks.IndexOf(bookmark);

                  if (index == -1)
                  {
                        return;
                  }

                  int newIndex = index + direction;

                  if (newIndex >= 0 && newIndex < bookmarks.Count)
                  {
                        bookmarks.RemoveAt(index);
                        bookmarks.Insert(newIndex, bookmark);
                        Save();
                  }
            }

            public void MoveGroup(BookmarkGroup group, int direction)
            {
                  SceneBookmarkData sceneData = GetCurrentSceneData();

                  if (sceneData == null)
                  {
                        return;
                  }

                  List<BookmarkGroup> groups = sceneData.groups;
                  int index = groups.IndexOf(group);

                  if (index == -1)
                  {
                        return;
                  }

                  int newIndex = index + direction;

                  if (newIndex >= 0 && newIndex < groups.Count)
                  {
                        groups.RemoveAt(index);
                        groups.Insert(newIndex, group);
                        Save();
                  }
            }

            private static string GetCurrentSceneGuid()
            {
                  Scene activeScene = SceneManager.GetActiveScene();

                  if (!activeScene.IsValid())
                  {
                        return null;
                  }

                  string scenePath = activeScene.path;

                  if (string.IsNullOrEmpty(scenePath))
                  {
                        return null;
                  }

                  return AssetDatabase.AssetPathToGUID(scenePath);
            }

            private void MigrateLegacyBookmarks()
            {
                  if (legacyBookmarks.Count > 0)
                  {
                        string currentSceneGuid = GetCurrentSceneGuid();

                        if (!string.IsNullOrEmpty(currentSceneGuid))
                        {
                              SceneBookmarkData sceneData = GetCurrentSceneData();

                              if (sceneData != null)
                              {
                                    foreach (SceneBookmark legacyBookmark in legacyBookmarks)
                                    {
                                          legacyBookmark.groupId = "";
                                          sceneData.bookmarks.Add(legacyBookmark);
                                    }

                                    legacyBookmarks.Clear();
                                    Save();

                                    Debug.Log($"Migrated {legacyBookmarks.Count} legacy bookmarks to current scene.");
                              }
                        }
                  }
            }

            public void Save()
            {
                  EditorUtility.SetDirty(this);
                  AssetDatabase.SaveAssets();
            }
      }
}