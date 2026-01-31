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
            public readonly FavoriteItemType ItemType;
            public readonly string Guid;
            public readonly int InstanceID;
            public readonly string ScenePath;
            public string alias;

            public FavoriteItem()
            {
            }

            public FavoriteItem(Object obj, string alias)
            {
                  this.alias = alias;

                  if (AssetDatabase.Contains(obj))
                  {
                        ItemType = FavoriteItemType.Asset;
                        Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                  }
                  else if (obj is GameObject go)
                  {
                        ItemType = FavoriteItemType.GameObject;

                        // ! InstanceID is obsolete
                        // This is a quick fix for now just to disable warnings in 6.4 that could be annoying
                        // No problem for now as InstanceID is still working but should not work on 6.5 and newer so...
                        // TODO: Convert to EntityID
#if UNITY_6000_3_OR_NEWER
                        #pragma warning disable CS0618
                        InstanceID = go.GetInstanceID();
                        #pragma warning restore CS0618
#else
                        instanceID = go.GetInstanceID();
#endif
                        ScenePath = go.scene.path;
                  }
            }

            public Object GetObject()
            {
                  switch (ItemType)
                  {
                        case FavoriteItemType.Asset:
                              return AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(Guid));
                        case FavoriteItemType.GameObject:
                              // TODO: Convert to EntityID
#if UNITY_6000_3_OR_NEWER
                              #pragma warning disable CS0618
                              return !IsSceneLoaded() ? null : EditorUtility.EntityIdToObject(InstanceID);
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
                  if (ItemType != FavoriteItemType.GameObject || string.IsNullOrEmpty(ScenePath))
                  {
                        return true;
                  }

                  for (int i = 0; i < SceneManager.sceneCount; i++)
                  {
                        if (SceneManager.GetSceneAt(i).path == ScenePath)
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

                  if (ItemType != other.ItemType)
                  {
                        return false;
                  }

                  return ItemType switch
                  {
                        FavoriteItemType.Asset => Guid == other.Guid,
                        FavoriteItemType.GameObject => InstanceID == other.InstanceID && ScenePath == other.ScenePath,
                        _ => false
                  };
            }

            public override int GetHashCode()
            {
                  return ItemType switch
                  {
                        FavoriteItemType.Asset => HashCode.Combine(ItemType, Guid),
                        FavoriteItemType.GameObject => HashCode.Combine(ItemType, InstanceID, ScenePath),
                        _ => base.GetHashCode()
                  };
            }
      }
}