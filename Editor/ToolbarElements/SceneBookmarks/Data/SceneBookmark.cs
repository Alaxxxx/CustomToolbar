using System;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Data
{
      [Serializable]
      public class SceneBookmark
      {
            public string name;
            public Vector3 pivot;
            public Quaternion rotation;
            public float size;
            public byte[] thumbnailData;
            public string groupId;

            [NonSerialized]
            public Texture2D ThumbnailTexture;

            public SceneBookmark(string name, Vector3 pivot, Quaternion rotation, float size, string groupId = "")
            {
                  this.name = name;
                  this.pivot = pivot;
                  this.rotation = rotation;
                  this.size = size;
                  this.groupId = groupId;
            }

            public void LoadTexture()
            {
                  if (thumbnailData is { Length: > 0 })
                  {
                        ThumbnailTexture = new Texture2D(2, 2);
                        ThumbnailTexture.LoadImage(thumbnailData);
                  }
            }
      }

      [Serializable]
      public class BookmarkGroup
      {
            public string id;
            public string name;
            public bool isExpanded = true;

            public BookmarkGroup(string name)
            {
                  this.id = Guid.NewGuid().ToString();
                  this.name = name;
            }
      }

      [Serializable]
      public class SceneBookmarkData
      {
            public string sceneGuid;
            public System.Collections.Generic.List<SceneBookmark> bookmarks = new();
            public System.Collections.Generic.List<BookmarkGroup> groups = new();

            public SceneBookmarkData(string sceneGuid)
            {
                  this.sceneGuid = sceneGuid;
            }
      }
}