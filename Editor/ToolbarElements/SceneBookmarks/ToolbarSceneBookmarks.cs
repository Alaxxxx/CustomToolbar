using System.Collections.Generic;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Data;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Window;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks
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