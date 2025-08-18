using System.Collections.Generic;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Data;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Window;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks
{
      sealed internal class ToolbarSceneBookmarks : BaseToolbarElement
      {
            private GUIContent _buttonContent;

            protected override string Name => "Scene Bookmarks";
            protected override string Tooltip => "Quickly access saved scene view bookmarks.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_CameraPreview").image;
                  _buttonContent = new GUIContent("", icon, this.Tooltip);
                  this.Width = 45;
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(_buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        ShowBookmarksMenu();
                  }
            }

            private static void ShowBookmarksMenu()
            {
                  var menu = new GenericMenu();
                  var manager = SceneBookmarksManager.Instance;
                  List<BookmarkGroup> groups = manager.GetCurrentSceneGroups();
                  List<SceneBookmark> bookmarks = manager.GetCurrentSceneBookmarks();
                  List<SceneBookmark> rootBookmarks = bookmarks.Where(static b => string.IsNullOrEmpty(b.groupId)).ToList();

                  bool hasItems = false;

                  foreach (BookmarkGroup group in groups)
                  {
                        List<SceneBookmark> groupBookmarks = bookmarks.Where(b => b.groupId == group.id).ToList();

                        if (groupBookmarks.Count > 0)
                        {
                              foreach (SceneBookmark bookmark in groupBookmarks)
                              {
                                    string menuPath = $"{group.name}/{bookmark.name}";
                                    menu.AddItem(new GUIContent(menuPath), false, () => SceneBookmarksWindow.GoToBookmark(bookmark));
                                    hasItems = true;
                              }
                        }
                  }

                  if (rootBookmarks.Count > 0)
                  {
                        if (hasItems)
                        {
                              menu.AddSeparator("");
                        }

                        foreach (SceneBookmark bookmark in rootBookmarks)
                        {
                              menu.AddItem(new GUIContent(bookmark.name), false, () => SceneBookmarksWindow.GoToBookmark(bookmark));
                              hasItems = true;
                        }
                  }

                  if (!hasItems)
                  {
                        menu.AddDisabledItem(new GUIContent("No bookmarks for this scene"));
                  }

                  menu.AddSeparator("");
                  menu.AddItem(new GUIContent("Manage Bookmarks..."), false, SceneBookmarksWindow.ShowWindow);

                  menu.ShowAsContext();
            }
      }
}