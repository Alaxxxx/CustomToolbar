using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.ToolbarElements.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomToolbar.Editor.ToolbarElements.Views
{
      public sealed class FavoritesWindow : EditorWindow
      {
            sealed private class CachedFavorite
            {
                  public FavoriteItem SourceItem;
                  public Object AssetObject;
                  public Texture AssetIcon;
                  public string AssetTypeName;
            }

            private FavoritesManager manager;
            private FavoriteList currentList;
            private int currentListIndex;
            private readonly List<CachedFavorite> cachedFavorites = new();
            private string searchQuery = "";
            private Vector2 scrollPosition;

            private GUIStyle searchFieldStyle;
            private GUIStyle searchCancelButtonStyle;
            private GUIStyle listBackgroundStyle;
            private GUIStyle cardTitleStyle;
            private GUIStyle cardSubtitleStyle;

            private readonly Dictionary<Color, Texture2D> textureCache = new();
            private bool stylesInitialized;

            public static void ShowWindow()
            {
                  GetWindow<FavoritesWindow>("Favorites");
            }

            private void OnEnable()
            {
                  manager = FavoritesManager.Instance;
                  currentListIndex = manager.lastUsedListIndex;
                  LoadCurrentList();
            }

            private void OnDisable()
            {
                  if (manager != null)
                  {
                        manager.lastUsedListIndex = currentListIndex;
                        EditorUtility.SetDirty(manager);
                  }
            }

            private void InitStyles()
            {
                  if (stylesInitialized)
                  {
                        return;
                  }

                  searchFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField");
                  searchCancelButtonStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton");
                  listBackgroundStyle = new GUIStyle(GUI.skin.box);

                  cardTitleStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.LowerLeft, fontSize = 13 };
                  cardSubtitleStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperLeft };

                  stylesInitialized = true;
            }

            private void UpdateFavoritesCache()
            {
                  cachedFavorites.Clear();

                  if (currentList == null)
                  {
                        Repaint();

                        return;
                  }

                  foreach (FavoriteItem item in currentList.items)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(item.guid);

                        if (string.IsNullOrEmpty(path))
                        {
                              continue;
                        }

                        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                        if (obj == null)
                        {
                              continue;
                        }

                        Texture icon = AssetPreview.GetAssetPreview(obj) ? AssetPreview.GetAssetPreview(obj) : EditorGUIUtility.ObjectContent(obj, typeof(Object)).image;

                        cachedFavorites.Add(new CachedFavorite
                        {
                                    SourceItem = item,
                                    AssetObject = obj,
                                    AssetIcon = icon,
                                    AssetTypeName = obj.GetType().Name
                        });
                  }

                  Repaint();
            }

            private void OnGUI()
            {
                  if (Event.current.type == EventType.MouseMove)
                  {
                        Repaint();
                  }

                  manager = manager ? manager : FavoritesManager.Instance;

                  InitStyles();
                  DrawHeader();

                  if (currentList != null)
                  {
                        DrawListSettingsHeader();
                  }

                  DrawSearchBar();
                  EditorGUILayout.Space(5);

                  scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                  if (currentList == null)
                  {
                        EditorGUILayout.HelpBox("No Favorites List found. Create one via the '+' button.", MessageType.Info);
                  }
                  else
                  {
                        DrawFavorites();
                        HandleDragAndDrop();
                  }

                  EditorGUILayout.EndScrollView();
            }

            private void LoadCurrentList()
            {
                  if (manager.allLists.Count > 0)
                  {
                        currentListIndex = Mathf.Clamp(currentListIndex, 0, manager.allLists.Count - 1);
                        currentList = manager.allLists[currentListIndex];
                  }
                  else
                  {
                        currentList = null;
                  }

                  UpdateFavoritesCache();
            }

            private void DrawHeader()
            {
                  EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                  if (manager.allLists.Count > 0)
                  {
                        string[] listNames = manager.allLists.Select(static l => l.name).ToArray();

                        EditorGUI.BeginChangeCheck();
                        currentListIndex = EditorGUILayout.Popup(currentListIndex, listNames, EditorStyles.toolbarPopup);

                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                              currentListIndex = (currentListIndex - 1 + manager.allLists.Count) % manager.allLists.Count;
                        }

                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_next"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                              currentListIndex = (currentListIndex + 1) % manager.allLists.Count;
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                              LoadCurrentList();
                        }
                  }
                  else
                  {
                        GUILayout.Label("No lists found", EditorStyles.toolbarButton);
                  }

                  GUILayout.FlexibleSpace();

                  if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.toolbarButton))
                  {
                        Undo.RecordObject(manager, "Add New List");
                        manager.allLists.Add(new FavoriteList());
                        currentListIndex = manager.allLists.Count - 1;
                        LoadCurrentList();
                        EditorUtility.SetDirty(manager);
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void DrawListSettingsHeader()
            {
                  EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                  EditorGUILayout.Space(2);
                  GUILayout.Label("List Settings", EditorStyles.boldLabel);
                  EditorGUILayout.Space(2);
                  EditorGUI.BeginChangeCheck();
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("Name", GUILayout.Width(50));
                  string newName = EditorGUILayout.TextField(currentList.name);
                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("Color", GUILayout.Width(50));
                  Color newColor = EditorGUILayout.ColorField(currentList.listColor);
                  EditorGUILayout.EndHorizontal();

                  if (EditorGUI.EndChangeCheck())
                  {
                        Undo.RecordObject(manager, "Change List Settings");
                        currentList.name = newName;
                        currentList.listColor = newColor;
                        EditorUtility.SetDirty(manager);
                  }

                  EditorGUILayout.Space(5);

                  if (GUILayout.Button(new GUIContent("Delete This List", "This action cannot be undone."), EditorStyles.miniButton) &&
                      EditorUtility.DisplayDialog("Delete List?", $"Are you sure you want to delete the list '{currentList.name}'?", "Yes", "No"))
                  {
                        Undo.RecordObject(manager, "Delete List");
                        manager.allLists.RemoveAt(currentListIndex);
                        currentListIndex = Mathf.Max(0, currentListIndex - 1);
                        LoadCurrentList();
                        EditorUtility.SetDirty(manager);
                  }

                  EditorGUILayout.Space(2);
                  EditorGUILayout.EndVertical();
            }

            private void DrawSearchBar()
            {
                  EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                  searchQuery = EditorGUILayout.TextField(searchQuery, searchFieldStyle);

                  if (GUILayout.Button("", searchCancelButtonStyle))
                  {
                        searchQuery = "";
                        GUI.FocusControl(null);
                  }

                  EditorGUILayout.EndHorizontal();
            }

            private void DrawFavorites()
            {
                  Color bgColor = currentList.listColor;
                  bgColor.a = 0.05f;

                  if (!textureCache.TryGetValue(bgColor, out Texture2D bgTexture))
                  {
                        bgTexture = new Texture2D(1, 1);
                        bgTexture.SetPixel(0, 0, bgColor);
                        bgTexture.Apply();
                        textureCache[bgColor] = bgTexture;
                  }

                  if (listBackgroundStyle.normal.background != bgTexture)
                  {
                        listBackgroundStyle.normal.background = bgTexture;
                  }

                  EditorGUILayout.BeginHorizontal();
                  GUILayout.Space(5);
                  EditorGUILayout.BeginVertical(listBackgroundStyle);
                  EditorGUILayout.Space(5);

                  foreach (CachedFavorite cachedItem in cachedFavorites)
                  {
                        if (!string.IsNullOrEmpty(searchQuery) &&
                            !cachedItem.SourceItem.alias.ToLower().Contains(searchQuery.ToLower(), StringComparison.OrdinalIgnoreCase))
                        {
                              continue;
                        }

                        DrawItemCard(cachedItem);
                        EditorGUILayout.Space(4);
                  }

                  EditorGUILayout.Space(5);
                  EditorGUILayout.EndVertical();
                  GUILayout.Space(5);
                  EditorGUILayout.EndHorizontal();
            }

            private void DrawItemCard(CachedFavorite cachedItem)
            {
                  Rect cardRect = GUILayoutUtility.GetRect(0, 48);
                  bool isHover = cardRect.Contains(Event.current.mousePosition);

                  Color bgColor = EditorGUIUtility.isProSkin
                              ? (isHover ? new(0.3f, 0.3f, 0.3f) : new Color(0.25f, 0.25f, 0.25f))
                              : (isHover ? new(0.95f, 0.95f, 0.95f) : new Color(0.9f, 0.9f, 0.9f));
                  EditorGUI.DrawRect(cardRect, bgColor);

                  const float padding = 4f;
                  const float iconSize = 40f;

                  var iconRect = new Rect(cardRect.x + padding, cardRect.y + padding, iconSize, iconSize);
                  var textBlockRect = new Rect(iconRect.xMax + 8, cardRect.y, cardRect.width - iconSize - padding * 2 - 8, cardRect.height);

                  if (cachedItem.AssetIcon)
                  {
                        GUI.DrawTexture(iconRect, cachedItem.AssetIcon);
                  }

                  var titleRect = new Rect(textBlockRect.x, textBlockRect.y, textBlockRect.width, textBlockRect.height / 2);
                  var subtitleRect = new Rect(textBlockRect.x, textBlockRect.y + titleRect.height, textBlockRect.width, textBlockRect.height / 2);

                  EditorGUI.LabelField(titleRect, new GUIContent(cachedItem.SourceItem.alias), cardTitleStyle);
                  EditorGUI.LabelField(subtitleRect, cachedItem.AssetTypeName, cardSubtitleStyle);

                  Event e = Event.current;

                  if (cardRect.Contains(e.mousePosition))
                  {
                        if (e.type == EventType.MouseDrag && e.button == 0)
                        {
                              DragAndDrop.PrepareStartDrag();
                              DragAndDrop.objectReferences = new[] { cachedItem.AssetObject };
                              DragAndDrop.StartDrag(cachedItem.SourceItem.alias);
                              e.Use();
                        }
                        else if (e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2)
                        {
                              AssetDatabase.OpenAsset(cachedItem.AssetObject);
                              e.Use();
                        }
                        else if (e.type == EventType.ContextClick)
                        {
                              var menu = new GenericMenu();

                              menu.AddItem(new GUIContent("Remove"), false, () =>
                              {
                                    Undo.RecordObject(manager, "Remove Favorite");
                                    currentList.items.Remove(cachedItem.SourceItem);
                                    EditorUtility.SetDirty(manager);
                                    UpdateFavoritesCache();
                              });
                              menu.ShowAsContext();
                              e.Use();
                        }
                  }
            }

            private void HandleDragAndDrop()
            {
                  Event e = Event.current;
                  var windowRect = new Rect(0, 0, position.width, position.height);

                  if (!windowRect.Contains(e.mousePosition))
                  {
                        return;
                  }

                  if (e.type is EventType.DragUpdated or EventType.DragPerform &&
                      DragAndDrop.objectReferences.Any(static o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))))
                  {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (e.type == EventType.DragPerform)
                        {
                              DragAndDrop.AcceptDrag();
                              Undo.RecordObject(manager, "Add Favorites");

                              foreach (Object draggedObject in DragAndDrop.objectReferences)
                              {
                                    string path = AssetDatabase.GetAssetPath(draggedObject);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                          string guid = AssetDatabase.AssetPathToGUID(path);
                                          currentList.items.Add(new FavoriteItem(guid, draggedObject.name));
                                    }
                              }

                              EditorUtility.SetDirty(manager);
                              UpdateFavoritesCache();
                        }

                        e.Use();
                  }
            }
      }
}