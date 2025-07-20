using System.Collections.Generic;
using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.ToolbarElements.Data;
using CustomToolbar.Editor.ToolbarElements.Views;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarSceneBookmarks : BaseToolbarElement
      {
            private GUIContent buttonContent;

            protected override string Name => "Scene Bookmarks";
            protected override string Tooltip => "Quickly access saved scene view bookmarks.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_CameraPreview").image;
                  buttonContent = new GUIContent("", icon, this.Tooltip);
                  this.Width = 45;
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        ShowBookmarksMenu();
                  }
            }

            private static void ShowBookmarksMenu()
            {
                  var menu = new GenericMenu();
                  List<SceneBookmark> bookmarks = SceneBookmarksManager.Instance.bookmarks;

                  if (bookmarks.Count > 0)
                  {
                        foreach (SceneBookmark bookmark in bookmarks)
                        {
                              menu.AddItem(new GUIContent(bookmark.name), false, () => SceneBookmarksWindow.GoToBookmark(bookmark));
                        }
                  }
                  else
                  {
                        menu.AddDisabledItem(new GUIContent("No bookmarks yet"));
                  }

                  menu.AddSeparator("");
                  menu.AddItem(new GUIContent("Manage Bookmarks..."), false, SceneBookmarksWindow.ShowWindow);

                  menu.ShowAsContext();
            }
      }
}