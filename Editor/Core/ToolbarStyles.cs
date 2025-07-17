using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      internal static class ToolbarStyles
      {
            public readonly static GUIStyle CommandButtonStyle;
            public readonly static GUIStyle CommandPopupStyle;
            public readonly static GUIStyle ToolbarHorizontalGroupStyle;

            static ToolbarStyles()
            {
                  CommandButtonStyle = new GUIStyle("ToolbarButton")
                  {
                              alignment = TextAnchor.MiddleCenter,
                              margin = new RectOffset(2, 2, 2, 2),
                  };

                  CommandPopupStyle = new GUIStyle("ToolbarPopup")
                  {
                              alignment = TextAnchor.MiddleCenter,
                              margin = new RectOffset(2, 2, 2, 2),
                  };

                  ToolbarHorizontalGroupStyle = new GUIStyle
                  {
                              alignment = TextAnchor.MiddleCenter,
                              stretchHeight = true,
                  };
            }
      }
}