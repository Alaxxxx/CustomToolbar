using System.Collections.Generic;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.Core.Data
{
      public sealed class ToolbarConfiguration : ScriptableObject
      {
            public List<ToolbarGroup> groups = new();
            public List<ToolboxShortcut> toolboxShortcuts = new();
      }
}