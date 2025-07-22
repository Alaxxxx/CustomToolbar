using System.Collections.Generic;

namespace CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      [System.Serializable]
      public class FavoriteList
      {
            public string name = "New List";
            public List<FavoriteItem> items = new();
      }
}