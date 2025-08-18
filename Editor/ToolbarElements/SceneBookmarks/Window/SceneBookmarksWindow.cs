using System.Collections.Generic;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks.Window
{
      public sealed class SceneBookmarksWindow : EditorWindow
      {
            private SceneBookmarksManager _manager;
            private Vector2 _scrollPosition;
            private string _newBookmarkName = "New Bookmark";
            private const string NewGroupName = "New Group";

            private GUIStyle _headerBackgroundStyle;
            private GUIStyle _cardStyle;
            private GUIStyle _groupHeaderStyle;
            private GUIStyle _bookmarkNameStyle;
            private GUIStyle _toggleButtonStyle;
            private GUIContent _deleteIcon;
            private GUIContent _gotoIcon;
            private GUIContent _updateIcon;
            private GUIContent _upIcon;
            private GUIContent _downIcon;
            private GUIContent _folderIcon;
            private GUIContent _addIcon;
            private static bool useSmoothTransition = true;
            private static bool isTransitioning;

            private bool _stylesInitialized;

            private static float transitionStartTime;
            private const float TransitionDuration = 0.5f;
            private static SceneBookmark targetBookmark;
            private static Vector3 startPivot;
            private static Quaternion startRotation;
            private static float startSize;

            public static void ShowWindow()
            {
                  var window = GetWindow<SceneBookmarksWindow>("Scene Bookmarks");
                  window.minSize = new Vector2(450, 350);
                  window.Show();
            }

            private void OnEnable()
            {
                  _manager = SceneBookmarksManager.Instance;
                  LoadTexturesForCurrentScene();
            }

            private void LoadTexturesForCurrentScene()
            {
                  List<SceneBookmark> bookmarks = _manager.GetCurrentSceneBookmarks();

                  foreach (SceneBookmark bookmark in bookmarks)
                  {
                        bookmark.LoadTexture();
                  }
            }

            private void InitializeStyles()
            {
                  Texture2D headerBgTex = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f));
                  Texture2D cardBgTex = MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f));
                  Texture2D groupBgTex = MakeTex(1, 1, new Color(0.18f, 0.18f, 0.18f));

                  _headerBackgroundStyle = new GUIStyle { normal = { background = headerBgTex }, padding = new RectOffset(10, 10, 7, 7) };

                  _cardStyle = new GUIStyle(GUI.skin.box)
                              { padding = new RectOffset(10, 10, 10, 10), margin = new RectOffset(10, 10, 5, 5), normal = { background = cardBgTex } };

                  _groupHeaderStyle = new GUIStyle(GUI.skin.box)
                              { padding = new RectOffset(8, 8, 5, 5), margin = new RectOffset(5, 5, 2, 2), normal = { background = groupBgTex } };
                  _bookmarkNameStyle = new GUIStyle(EditorStyles.textField) { fontSize = 13, fixedHeight = 20, margin = { top = 2 } };
                  _toggleButtonStyle = new GUIStyle(EditorStyles.toolbarButton);

                  _deleteIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
                  _gotoIcon = EditorGUIUtility.IconContent("d_scenevis_visible_hover");
                  _updateIcon = EditorGUIUtility.IconContent("d_Refresh");
                  _upIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineDigDownArrow");
                  _downIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineRollUpArrow");
                  _folderIcon = EditorGUIUtility.IconContent("d_Folder Icon");
                  _addIcon = EditorGUIUtility.IconContent("d_Toolbar Plus");

                  _stylesInitialized = true;
            }

            private void OnGUI()
            {
                  if (!_stylesInitialized)
                  {
                        InitializeStyles();
                  }

                  DrawHeader();
                  DrawCurrentSceneInfo();
                  DrawBookmarkList();
                  DrawFooter();
            }

            private static void DrawCurrentSceneInfo()
            {
                  Scene activeScene = SceneManager.GetActiveScene();

                  if (activeScene.IsValid())
                  {
                        EditorGUILayout.LabelField($"Scene: {activeScene.name}", EditorStyles.boldLabel);
                  }
                  else
                  {
                        EditorGUILayout.HelpBox("No active scene. Bookmarks require an active scene.", MessageType.Warning);
                  }
            }

            private void DrawHeader()
            {
                  EditorGUILayout.BeginHorizontal(_headerBackgroundStyle, GUILayout.Height(40));

                  _newBookmarkName = EditorGUILayout.TextField(_newBookmarkName, GUILayout.ExpandWidth(true), GUILayout.Height(25));

                  if (GUILayout.Button("Add Bookmark", GUILayout.Width(100), GUILayout.Height(25)))
                  {
                        AddNewBookmark();
                  }

                  if (GUILayout.Button(new GUIContent(_addIcon.image, "Create New Group"), GUILayout.Width(30), GUILayout.Height(25)))
                  {
                        ShowCreateGroupDialog();
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void DrawBookmarkList()
            {
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                  List<BookmarkGroup> groups = _manager.GetCurrentSceneGroups();
                  List<SceneBookmark> bookmarks = _manager.GetCurrentSceneBookmarks();
                  List<SceneBookmark> rootBookmarks = bookmarks.Where(static b => string.IsNullOrEmpty(b.groupId)).ToList();

                  for (int i = 0; i < groups.Count; i++)
                  {
                        DrawGroup(groups[i], i, groups.Count);
                  }

                  if (rootBookmarks.Count > 0)
                  {
                        EditorGUILayout.Space(5);

                        for (int i = 0; i < rootBookmarks.Count; i++)
                        {
                              EditorGUILayout.BeginHorizontal(_cardStyle);
                              DrawBookmarkCard(rootBookmarks[i], i, rootBookmarks.Count);
                              EditorGUILayout.EndHorizontal();
                        }
                  }

                  if (groups.Count == 0 && rootBookmarks.Count == 0)
                  {
                        EditorGUILayout.HelpBox("No bookmarks yet. Position your camera and click 'Add Bookmark'.", MessageType.Info);
                  }

                  EditorGUILayout.EndScrollView();
            }

            private void DrawGroup(BookmarkGroup group, int groupIndex, int totalGroups)
            {
                  EditorGUILayout.BeginVertical(_groupHeaderStyle);

                  EditorGUILayout.BeginHorizontal();

                  group.isExpanded = EditorGUILayout.Foldout(group.isExpanded, "", true);

                  GUILayout.Label(_folderIcon, GUILayout.Width(16), GUILayout.Height(16));

                  EditorGUI.BeginChangeCheck();
                  string newGroupName = EditorGUILayout.TextField(group.name, EditorStyles.textField);

                  if (EditorGUI.EndChangeCheck())
                  {
                        group.name = newGroupName;
                        _manager.Save();
                  }

                  GUILayout.FlexibleSpace();

                  if (IconButton(_upIcon, groupIndex > 0))
                  {
                        _manager.MoveGroup(group, -1);
                  }

                  if (IconButton(_downIcon, groupIndex < totalGroups - 1))
                  {
                        _manager.MoveGroup(group, 1);
                  }

                  if (IconButton(_deleteIcon) &&
                      EditorUtility.DisplayDialog("Delete Group", $"Delete group '{group.name}'?\nBookmarks will be moved to root.", "Yes", "No"))
                  {
                        _manager.RemoveGroup(group);
                        GUIUtility.ExitGUI();
                  }

                  EditorGUILayout.EndHorizontal();

                  if (group.isExpanded)
                  {
                        List<SceneBookmark> groupBookmarks = _manager.GetCurrentSceneBookmarks().Where(b => b.groupId == group.id).ToList();

                        if (groupBookmarks.Count > 0)
                        {
                              EditorGUILayout.Space(3);

                              for (int i = 0; i < groupBookmarks.Count; i++)
                              {
                                    EditorGUILayout.BeginHorizontal(_cardStyle);
                                    DrawBookmarkCard(groupBookmarks[i], i, groupBookmarks.Count);
                                    EditorGUILayout.EndHorizontal();
                              }
                        }
                        else
                        {
                              EditorGUILayout.LabelField("  No bookmarks in this group", EditorStyles.centeredGreyMiniLabel);
                        }
                  }

                  EditorGUILayout.EndVertical();
                  EditorGUILayout.Space(3);
            }

            private void DrawBookmarkCard(SceneBookmark bookmark, int index, int totalCount)
            {
                  EditorGUILayout.BeginHorizontal();

                  if (bookmark.ThumbnailTexture)
                  {
                        Rect rect = GUILayoutUtility.GetRect(128, 72, GUILayout.Width(128), GUILayout.Height(72));
                        GUI.DrawTexture(rect, bookmark.ThumbnailTexture);

                        if (rect.Contains(Event.current.mousePosition))
                        {
                              GUI.tooltip = $"Position: ({bookmark.pivot.x:F1}, {bookmark.pivot.y:F1}, {bookmark.pivot.z:F1}) - Size: {bookmark.size:F1}";
                        }
                  }
                  else
                  {
                        if (GUILayout.Button("Generate\nThumbnail", GUILayout.Width(128), GUILayout.Height(72)))
                        {
                              CaptureThumbnail(bookmark);
                        }
                  }

                  EditorGUILayout.BeginVertical();

                  EditorGUI.BeginChangeCheck();
                  string newName = EditorGUILayout.TextField(bookmark.name, _bookmarkNameStyle);

                  if (EditorGUI.EndChangeCheck())
                  {
                        bookmark.name = newName;
                        _manager.Save();
                  }

                  EditorGUILayout.BeginHorizontal();

                  if (IconButton(_gotoIcon))
                  {
                        GoToBookmark(bookmark);
                  }

                  if (IconButton(_updateIcon))
                  {
                        UpdateBookmark(bookmark);
                  }

                  if (GUILayout.Button("Move", GUILayout.Width(50), GUILayout.Height(30)))
                  {
                        ShowMoveToGroupMenu(bookmark);
                  }

                  if (IconButton(_deleteIcon) && EditorUtility.DisplayDialog("Delete Bookmark", $"Delete '{bookmark.name}'?", "Yes", "No"))
                  {
                        _manager.RemoveBookmark(bookmark);
                        GUIUtility.ExitGUI();
                  }

                  GUILayout.FlexibleSpace();

                  if (IconButton(_upIcon, index > 0))
                  {
                        _manager.MoveBookmark(bookmark, -1);
                  }

                  if (IconButton(_downIcon, index < totalCount - 1))
                  {
                        _manager.MoveBookmark(bookmark, 1);
                  }

                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.EndVertical();
                  EditorGUILayout.EndHorizontal();
            }

            private void ShowCreateGroupDialog()
            {
                  string groupName = EditorInputDialog.Show("Create New Group", "Group name:", NewGroupName);

                  if (!string.IsNullOrEmpty(groupName) && groupName != NewGroupName)
                  {
                        _manager.CreateGroup(groupName);
                  }
            }

            private void ShowMoveToGroupMenu(SceneBookmark bookmark)
            {
                  var menu = new GenericMenu();
                  List<BookmarkGroup> groups = _manager.GetCurrentSceneGroups();

                  menu.AddItem(new GUIContent("Root"), string.IsNullOrEmpty(bookmark.groupId), () => _manager.MoveBookmarkToGroup(bookmark, ""));

                  if (groups.Count > 0)
                  {
                        menu.AddSeparator("");

                        foreach (BookmarkGroup group in groups)
                        {
                              bool isCurrentGroup = bookmark.groupId == group.id;
                              menu.AddItem(new GUIContent(group.name), isCurrentGroup, () => _manager.MoveBookmarkToGroup(bookmark, group.id));
                        }
                  }

                  menu.ShowAsContext();
            }

            private static bool IconButton(GUIContent content, bool enabled = true)
            {
                  using (new EditorGUI.DisabledScope(!enabled))
                  {
                        return GUILayout.Button(content, GUI.skin.button, GUILayout.Width(35), GUILayout.Height(30));
                  }
            }

            private void DrawFooter()
            {
                  EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                  Color originalColor = GUI.backgroundColor;
                  GUI.backgroundColor = useSmoothTransition ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.6f, 0.6f);
                  useSmoothTransition = GUILayout.Toggle(useSmoothTransition, "Smooth Transition", _toggleButtonStyle);
                  GUI.backgroundColor = originalColor;

                  GUILayout.FlexibleSpace();
                  EditorGUILayout.EndHorizontal();
            }

            private void AddNewBookmark()
            {
                  var currentSceneView = SceneView.lastActiveSceneView;

                  if (currentSceneView)
                  {
                        var newBookmark = new SceneBookmark(_newBookmarkName, currentSceneView.pivot, currentSceneView.rotation, currentSceneView.size);
                        CaptureThumbnail(newBookmark);
                        _manager.AddBookmark(newBookmark);
                        _newBookmarkName = "New Bookmark";
                        GUI.FocusControl(null);
                  }
            }

            private void UpdateBookmark(SceneBookmark bookmark)
            {
                  var currentSceneView = SceneView.lastActiveSceneView;

                  if (currentSceneView)
                  {
                        bookmark.pivot = currentSceneView.pivot;
                        bookmark.rotation = currentSceneView.rotation;
                        bookmark.size = currentSceneView.size;
                        CaptureThumbnail(bookmark);
                        _manager.Save();
                  }
            }

            public static void GoToBookmark(SceneBookmark bookmark)
            {
                  var sceneView = SceneView.lastActiveSceneView;

                  if (!sceneView || isTransitioning)
                  {
                        return;
                  }

                  if (useSmoothTransition)
                  {
                        EditorApplication.update += SmoothTransitionUpdate;
                        transitionStartTime = (float)EditorApplication.timeSinceStartup;
                        startPivot = sceneView.pivot;
                        startRotation = sceneView.rotation;
                        startSize = sceneView.size;
                        targetBookmark = bookmark;
                        isTransitioning = true;
                  }
                  else
                  {
                        sceneView.pivot = bookmark.pivot;
                        sceneView.rotation = bookmark.rotation;
                        sceneView.size = bookmark.size;
                        sceneView.Repaint();
                  }
            }

            private void CaptureThumbnail(SceneBookmark bookmark, int width = 256, int height = 144)
            {
                  var sceneView = SceneView.lastActiveSceneView;

                  if (!sceneView)
                  {
                        return;
                  }

                  var tempCamGo = new GameObject("ThumbnailCamera") { hideFlags = HideFlags.HideAndDontSave };
                  var tempCam = tempCamGo.AddComponent<Camera>();

                  tempCam.CopyFrom(sceneView.camera);
                  Transform transform = tempCam.transform;
                  transform.position = bookmark.pivot - (bookmark.rotation * Vector3.forward * (bookmark.size * 2));
                  transform.rotation = bookmark.rotation;

                  var rt = new RenderTexture(width, height, 24);
                  tempCam.targetTexture = rt;
                  tempCam.Render();

                  RenderTexture.active = rt;
                  var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                  texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                  texture.Apply();

                  bookmark.thumbnailData = texture.EncodeToPNG();
                  bookmark.LoadTexture();

                  RenderTexture.active = null;
                  DestroyImmediate(tempCamGo);
                  DestroyImmediate(rt);
                  DestroyImmediate(texture);

                  _manager.Save();
            }

            private static void SmoothTransitionUpdate()
            {
                  var sceneView = SceneView.lastActiveSceneView;

                  if (sceneView == null)
                  {
                        EditorApplication.update -= SmoothTransitionUpdate;
                        isTransitioning = false;

                        return;
                  }

                  float t = (float)(EditorApplication.timeSinceStartup - transitionStartTime) / TransitionDuration;
                  t = Mathf.SmoothStep(0.0f, 1.0f, t);

                  sceneView.pivot = Vector3.Lerp(startPivot, targetBookmark.pivot, t);
                  sceneView.rotation = Quaternion.Slerp(startRotation, targetBookmark.rotation, t);
                  sceneView.size = Mathf.Lerp(startSize, targetBookmark.size, t);
                  sceneView.Repaint();

                  if (t >= 1.0f)
                  {
                        EditorApplication.update -= SmoothTransitionUpdate;
                        isTransitioning = false;
                  }
            }

            private static Texture2D MakeTex(int width, int height, Color col)
            {
                  var pix = new Color[width * height];

                  for (int i = 0; i < pix.Length; ++i)
                  {
                        pix[i] = col;
                  }

                  var result = new Texture2D(width, height);
                  result.SetPixels(pix);
                  result.Apply();

                  return result;
            }
      }

      public static class EditorInputDialog
      {
            public static string Show(string title, string message, string defaultValue)
            {
                  bool dialogResult = EditorUtility.DisplayDialog(title, $"{message}\n\nCurrent: {defaultValue}", "OK", "Cancel");

                  if (dialogResult)
                  {
                        return defaultValue + " Copy";
                  }

                  return null;
            }
      }
}