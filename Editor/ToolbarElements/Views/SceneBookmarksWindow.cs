using CustomToolbar.Editor.ToolbarElements.Data;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Views
{
      public sealed class SceneBookmarksWindow : EditorWindow
      {
            private SceneBookmarksManager _manager;
            private Vector2 _scrollPosition;
            private string _newBookmarkName = "New Bookmark";

            private GUIStyle _headerBackgroundStyle;
            private GUIStyle _cardStyle;
            private GUIStyle _bookmarkNameStyle;
            private GUIStyle _toggleButtonStyle;
            private GUIContent _deleteIcon;
            private GUIContent _gotoIcon;
            private GUIContent _updateIcon;
            private GUIContent _upIcon;
            private GUIContent _downIcon;
            private static bool useSmoothTransition = true;
            private static bool isTransitioning;

            private bool _stylesInitialized;

            private static float transitionStartTime;
            private const float TransitionDuration = 0.5f;
            private static SceneBookmark targetBookmark;
            private static Vector3 startPivot;
            private static Quaternion startRotation;
            private static float startSize;

            [MenuItem("Tools/Scene Bookmarks")]
            public static void ShowWindow()
            {
                  var window = GetWindow<SceneBookmarksWindow>("Scene Bookmarks");
                  window.minSize = new Vector2(450, 350);
                  window.Show();
            }

            private void OnEnable()
            {
                  _manager = SceneBookmarksManager.Instance;

                  foreach (SceneBookmark bookmark in _manager.bookmarks)
                  {
                        bookmark.LoadTexture();
                  }
            }

            private void InitializeStyles()
            {
                  Texture2D headerBgTex = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f));
                  Texture2D cardBgTex = MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f));

                  _headerBackgroundStyle = new GUIStyle { normal = { background = headerBgTex }, padding = new RectOffset(10, 10, 7, 7) };

                  _cardStyle = new GUIStyle(GUI.skin.box)
                              { padding = new RectOffset(10, 10, 10, 10), margin = new RectOffset(10, 10, 5, 5), normal = { background = cardBgTex } };
                  _bookmarkNameStyle = new GUIStyle(EditorStyles.textField) { fontSize = 13, fixedHeight = 20, margin = { top = 2 } };
                  _toggleButtonStyle = new GUIStyle(EditorStyles.toolbarButton);

                  _deleteIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
                  _gotoIcon = EditorGUIUtility.IconContent("d_scenevis_visible_hover");
                  _updateIcon = EditorGUIUtility.IconContent("d_Refresh");
                  _upIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineDigDownArrow");
                  _downIcon = EditorGUIUtility.IconContent("d_ProfilerTimelineRollUpArrow");

                  _stylesInitialized = true;
            }

            private void OnGUI()
            {
                  if (!_stylesInitialized)
                  {
                        InitializeStyles();
                  }

                  DrawHeader();
                  DrawBookmarkList();
                  DrawFooter();
            }

            private void DrawHeader()
            {
                  EditorGUILayout.BeginHorizontal(_headerBackgroundStyle, GUILayout.Height(40));
                  _newBookmarkName = EditorGUILayout.TextField(_newBookmarkName, GUILayout.ExpandWidth(true), GUILayout.Height(25));

                  if (GUILayout.Button("Add", GUILayout.Width(60), GUILayout.Height(25)))
                  {
                        AddNewBookmark();
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void DrawBookmarkList()
            {
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                  if (_manager.bookmarks.Count > 0)
                  {
                        for (int i = _manager.bookmarks.Count - 1; i >= 0; i--)
                        {
                              EditorGUILayout.BeginHorizontal(_cardStyle);
                              DrawBookmarkCard(i);
                              EditorGUILayout.EndHorizontal();
                        }
                  }
                  else
                  {
                        EditorGUILayout.HelpBox("No bookmarks yet. Position your camera and click 'Add'.", MessageType.Info);
                  }

                  EditorGUILayout.EndScrollView();
            }

            private void DrawBookmarkCard(int index)
            {
                  SceneBookmark bookmark = _manager.bookmarks[index];
                  EditorGUILayout.BeginHorizontal();

                  if (bookmark.ThumbnailTexture)
                  {
                        GUILayout.Box(bookmark.ThumbnailTexture, GUILayout.Width(128), GUILayout.Height(72));
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

                  if (IconButton(_deleteIcon) && EditorUtility.DisplayDialog("Delete Bookmark", $"Delete '{bookmark.name}'?", "Yes", "No"))
                  {
                        _manager.bookmarks.RemoveAt(index);
                        _manager.Save();

                        GUIUtility.ExitGUI();
                  }

                  GUILayout.FlexibleSpace();

                  if (IconButton(_upIcon, index > 0))
                  {
                        MoveBookmark(index, -1);
                  }

                  if (IconButton(_downIcon, index < _manager.bookmarks.Count - 1))
                  {
                        MoveBookmark(index, 1);
                  }

                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.EndVertical();
                  EditorGUILayout.EndHorizontal();
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
                        _manager.bookmarks.Add(newBookmark);
                        _manager.Save();
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

            private void MoveBookmark(int index, int direction)
            {
                  SceneBookmark bookmark = _manager.bookmarks[index];
                  _manager.bookmarks.RemoveAt(index);
                  _manager.bookmarks.Insert(index + direction, bookmark);
                  _manager.Save();
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

                  tempCam.transform.position = bookmark.pivot - (bookmark.rotation * Vector3.forward * (bookmark.size * 2));
                  tempCam.transform.rotation = bookmark.rotation;

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
}