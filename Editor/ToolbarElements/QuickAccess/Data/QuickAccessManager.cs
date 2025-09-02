using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess.Data
{
      public sealed class QuickAccessManager : ScriptableObject
      {
            private const string AssetPath = "Assets/Settings/CustomToolbar/QuickAccessManager.asset";
            private const int MaxItemsPerCategory = 10;

            [SerializeField] private List<QuickAccessItem> items = new();

            private static QuickAccessManager instance;
            public static QuickAccessManager Instance
            {
                  get
                  {
                        if (instance == null)
                        {
                              instance = AssetDatabase.LoadAssetAtPath<QuickAccessManager>(AssetPath);

                              if (instance == null)
                              {
                                    instance = CreateInstance<QuickAccessManager>();
                                    Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
                                    AssetDatabase.CreateAsset(instance, AssetPath);
                                    AssetDatabase.SaveAssets();
                              }
                        }

                        return instance;
                  }
            }

            public void AddItem(Object obj)
            {
                  if (obj == null)
                  {
                        return;
                  }

                  var newItem = new QuickAccessItem(obj);

                  if (newItem.ItemType is QuickAccessItemType.Asset or QuickAccessItemType.Scene && string.IsNullOrEmpty(newItem.Guid))
                  {
                        return;
                  }

                  if (newItem.ItemType == QuickAccessItemType.GameObject && newItem.InstanceID == 0)
                  {
                        return;
                  }

                  items.RemoveAll(item => item.Equals(newItem));
                  items.Insert(0, newItem);

                  List<IGrouping<string, QuickAccessItem>> itemsByCategory = items.GroupBy(GetCategoryKey).ToList();
                  var trimmedList = new List<QuickAccessItem>();

                  foreach (IGrouping<string, QuickAccessItem> group in itemsByCategory)
                  {
                        trimmedList.AddRange(group.Take(MaxItemsPerCategory));
                  }

                  items = trimmedList.OrderBy(i => items.IndexOf(i)).ToList();

                  EditorUtility.SetDirty(this);
            }

            public List<QuickAccessItem> GetRecentItems()
            {
                  items.RemoveAll(static item => item.GetObject() == null);

                  return items;
            }

            public void ClearHistory()
            {
                  items.Clear();
                  EditorUtility.SetDirty(this);
            }

            public static string GetCategoryKey(QuickAccessItem item)
            {
                  if (item.ItemType == QuickAccessItemType.Scene)
                  {
                        return "Scenes";
                  }

                  if (item.ItemType == QuickAccessItemType.GameObject)
                  {
                        return "Hierarchy";
                  }

                  Object obj = item.GetObject();

                  if (obj == null)
                  {
                        return "Others";
                  }

                  if (obj is GameObject)
                  {
                        return "Prefabs";
                  }

                  if (obj is DefaultAsset && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                  {
                        return "Folders";
                  }

                  if (obj is MonoScript)
                  {
                        return "Scripts";
                  }

                  if (obj is ScriptableObject)
                  {
                        return "ScriptableObjects";
                  }

                  if (obj.GetType().Name.EndsWith("Controller", System.StringComparison.Ordinal))
                  {
                        return "Animators";
                  }

                  return "Others";
            }
      }
}