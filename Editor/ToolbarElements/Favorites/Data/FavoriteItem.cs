using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      [Serializable]
      public enum FavoriteItemType
      {
            Asset,
            GameObject
      }

      [Serializable]
      public class FavoriteItem
      {
            public FavoriteItemType itemType;
            public string guid;
            public int instanceID;
            public string scenePath;
            public string alias;

            public FavoriteItem()
            {
            }

            public FavoriteItem(Object obj, string alias)
            {
                  this.alias = alias;

                  if (AssetDatabase.Contains(obj))
                  {
                        itemType = FavoriteItemType.Asset;
                        guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                  }
                  else if (obj is GameObject go)
                  {
                        itemType = FavoriteItemType.GameObject;

                        // ! InstanceID is obsolete
                        // This is a quick fix for now just to disable warnings in 6.4 that could be annoying
                        // No problem for now as InstanceID is still working but should not work on 6.5 and newer so...
                        // TODO: Convert to EntityID
#if UNITY_6000_3_OR_NEWER
                        #pragma warning disable CS0618
                        instanceID = go.GetInstanceID();
                        #pragma warning restore CS0618
#else
                        instanceID = go.GetInstanceID();
#endif
                        scenePath = go.scene.path;
                  }
            }

            public Object GetObject()
            {
                  switch (itemType)
                  {
                        case FavoriteItemType.Asset:
                              return AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                        case FavoriteItemType.GameObject:
                              // TODO: Convert to EntityID
#if UNITY_6000_3_OR_NEWER
                              #pragma warning disable CS0618
                              return !IsSceneLoaded() ? null : EditorUtility.EntityIdToObject(instanceID);
                              #pragma warning restore CS0618
#else
                              return !IsSceneLoaded() ? null : EditorUtility.InstanceIDToObject(instanceID);
#endif

                        default:
                              return null;
                  }
            }

            public bool IsSceneLoaded()
            {
                  if (itemType != FavoriteItemType.GameObject || string.IsNullOrEmpty(scenePath))
                  {
                        return true;
                  }

                  for (int i = 0; i < SceneManager.sceneCount; i++)
                  {
                        if (SceneManager.GetSceneAt(i).path == scenePath)
                        {
                              return true;
                        }
                  }

                  return false;
            }

            public override bool Equals(object obj)
            {
                  if (obj is not FavoriteItem other)
                  {
                        return false;
                  }

                  if (itemType != other.itemType)
                  {
                        return false;
                  }

                  return itemType switch
                  {
                        FavoriteItemType.Asset => guid == other.guid,
                        FavoriteItemType.GameObject => instanceID == other.instanceID && scenePath == other.scenePath,
                        _ => false
                  };
            }

            public override int GetHashCode()
            {
                  return itemType switch
                  {
                        FavoriteItemType.Asset => HashCode.Combine(itemType, guid),
                        FavoriteItemType.GameObject => HashCode.Combine(itemType, instanceID, scenePath),
                        _ => base.GetHashCode()
                  };
            }
      }
}