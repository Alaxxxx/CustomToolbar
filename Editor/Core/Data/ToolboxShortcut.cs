using System;

namespace OpalStudio.CustomToolbar.Editor.Core.Data
{
      [Serializable]
      public class ToolboxShortcut
      {
            public string displayName = "New Shortcut";
            public string subMenuPath = "";
            public string menuItemPath = "Window/...";
            public bool isEnabled = true;
      }
}