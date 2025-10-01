using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.QuickAccess.Data
{
      public enum QuickAccessItemType
      {
            Asset,
            GameObject,
            Scene
      }

      [Serializable]
      public class QuickAccessItem
      {
            public readonly QuickAccessItemType ItemType;
            public readonly string Guid;
            public readonly int InstanceID;
            public string displayName;
            public string scenePath;

            public QuickAccessItem()
            {
            }

            public QuickAccessItem(Object obj)
            {
                  displayName = obj.name;

                  if (obj is SceneAsset sceneAsset)
                  {
                        ItemType = QuickAccessItemType.Scene;
                        string path = AssetDatabase.GetAssetPath(sceneAsset);
                        Guid = AssetDatabase.AssetPathToGUID(path);
                        scenePath = path;
                  }
                  else if (AssetDatabase.Contains(obj))
                  {
                        ItemType = QuickAccessItemType.Asset;
                        string path = AssetDatabase.GetAssetPath(obj);
                        Guid = AssetDatabase.AssetPathToGUID(path);
                  }
                  else if (obj is GameObject go)
                  {
                        ItemType = QuickAccessItemType.GameObject;
                        InstanceID = go.GetInstanceID();
                        scenePath = go.scene.path;
                  }
            }

            public Object GetObject()
            {
                  return ItemType switch
                  {
                              QuickAccessItemType.Asset or QuickAccessItemType.Scene => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(Guid)),
#if UNITY_6000_3_OR_NEWER
                              QuickAccessItemType.GameObject => EditorUtility.EntityIdToObject(InstanceID),
#else
                              QuickAccessItemType.GameObject => EditorUtility.InstanceIDToObject(InstanceID),
#endif
                              _ => null
                  };
            }

            public override bool Equals(object obj)
            {
                  if (obj is not QuickAccessItem other)
                  {
                        return false;
                  }

                  if (ItemType != other.ItemType)
                  {
                        return false;
                  }

                  return ItemType switch
                  {
                              QuickAccessItemType.Asset or QuickAccessItemType.Scene => Guid == other.Guid,
                              QuickAccessItemType.GameObject => InstanceID == other.InstanceID,
                              _ => false
                  };
            }

            public override int GetHashCode()
            {
                  return ItemType switch
                  {
                              QuickAccessItemType.Asset or QuickAccessItemType.Scene => HashCode.Combine(ItemType, Guid),
                              QuickAccessItemType.GameObject => HashCode.Combine(ItemType, InstanceID),
                              _ => base.GetHashCode()
                  };
            }
      }
}