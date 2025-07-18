using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      internal static class ToolbarStyles
      {
            public readonly static GUIStyle CommandButtonStyle = new("ToolbarButton")
            {
                        alignment = TextAnchor.MiddleCenter,
                        margin = new RectOffset(2, 2, 2, 2),
            };

            public readonly static GUIStyle CommandPopupStyle = new("ToolbarPopup")
            {
                        alignment = TextAnchor.MiddleCenter,
                        margin = new RectOffset(2, 2, 2, 2),
            };

            public readonly static GUIStyle ToolbarHorizontalGroupStyle = new()
            {
                        alignment = TextAnchor.MiddleCenter,
                        stretchHeight = true,
            };
      }
}