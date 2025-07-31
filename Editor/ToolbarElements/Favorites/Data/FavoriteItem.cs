namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Data
{
      [System.Serializable]
      public class FavoriteItem
      {
            public string guid;
            public string alias;

            public FavoriteItem(string guid, string alias)
            {
                  this.guid = guid;
                  this.alias = alias;
            }
      }
}