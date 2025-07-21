using System.Collections.Generic;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Data
{
      [System.Serializable]
      public class FavoriteList
      {
            public string name = "New List";
            public Color listColor = new(0.8f, 0.8f, 0.8f, 1f);
            public List<FavoriteItem> items = new();
      }
}