using System.Collections.Generic;
using UnityEngine;

namespace CustomToolbar.Editor.Settings.Data
{
      public sealed class ToolbarConfiguration : ScriptableObject
      {
            public string packageRootPath = "Assets/CustomToolbar";
            public List<ToolbarGroup> groups = new();
            public List<ToolboxShortcut> toolboxShortcuts = new();
      }
}