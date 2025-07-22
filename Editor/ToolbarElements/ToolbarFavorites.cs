using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.ToolbarElements.Views;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarFavorites : BaseToolbarElement
      {
            private GUIContent buttonContent;

            protected override string Name => "Favorites";
            protected override string Tooltip => "Quick access to favorite assets and folders.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_Favorite Icon").image;

                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              FavoritesWindow.ShowWindow();
                        }
                  }
            }
      }
}