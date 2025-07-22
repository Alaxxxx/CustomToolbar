using System.Collections.Generic;
using UnityEngine;

namespace CustomToolbar.Editor.Core.Data
{
      public sealed class ToolbarConfiguration : ScriptableObject
      {
            public List<ToolbarGroup> groups = new();
            public List<ToolboxShortcut> toolboxShortcuts = new();
      }
}