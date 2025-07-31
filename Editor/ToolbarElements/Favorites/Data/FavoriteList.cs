using System.Collections.Generic;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      [System.Serializable]
      public class FavoriteList
      {
            public string name = "New List";
            public List<FavoriteItem> items = new();
      }
}