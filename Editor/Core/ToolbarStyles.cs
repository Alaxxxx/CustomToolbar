using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      internal static class ToolbarStyles
      {
            public readonly static GUIStyle commandButtonStyle;

            static ToolbarStyles()
            {
                  commandButtonStyle = new GUIStyle("Command")
                  {
                              fontSize = 16,
                              alignment = TextAnchor.MiddleCenter,
                              imagePosition = ImagePosition.ImageAbove,
                              fontStyle = FontStyle.Bold
                  };
            }
      }
}