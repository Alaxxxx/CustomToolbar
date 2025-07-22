using System;

namespace CustomToolbar.Editor.Core.Data
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