using System.Collections.Generic;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Data
{
      [System.Serializable]
      public class FavoriteList
      {
            public string name = "New List";
            public List<FavoriteItem> items = new();
      }
}