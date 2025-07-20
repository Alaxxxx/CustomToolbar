using System;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Data
{
      [Serializable]
      public class SceneBookmark
      {
            public string name;
            public Vector3 pivot;
            public Quaternion rotation;
            public float size;

            public byte[] thumbnailData;

            [NonSerialized]
            public Texture2D ThumbnailTexture;

            public SceneBookmark(string name, Vector3 pivot, Quaternion rotation, float size)
            {
                  this.name = name;
                  this.pivot = pivot;
                  this.rotation = rotation;
                  this.size = size;
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
}