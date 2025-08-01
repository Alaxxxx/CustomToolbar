﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites.Window
{
      public sealed class FavoritesWindow : EditorWindow
      {
#region Inner Classes

            sealed private class CachedFavorite
            {
                  public FavoriteItem SourceItem { get; set; }
                  public Object AssetObject { get; set; }
                  public Texture AssetIcon { get; set; }
                  public string AssetTypeName { get; set; }
                  public string LowerCaseAlias { get; set; }
            }

#endregion

#region Fields

            // Data & State
            private FavoritesManager _manager;
            private FavoriteList _currentList;
            private int _currentListIndex;
            private readonly List<CachedFavorite> _cachedFavorites = new();
            private readonly List<CachedFavorite> _filteredFavorites = new();

            // UI State
            private Vector2 _scrollPosition;
            private string _searchQuery = "";

            // State for reordering and editing
            private int _reorderDragIndex = -1;
            private int _reorderDropIndex = -1;
            private int _editingAliasIndex = -1;
            private string _aliasEditString = "";

            // UI Styles
            private GUIStyle _searchFieldStyle;
            private GUIStyle _searchCancelButtonStyle;
            private GUIStyle _listBackgroundStyle;
            private GUIStyle _cardTitleStyle;
            private GUIStyle _cardSubtitleStyle;
            private bool _stylesInitialized;

            // Events
            public event Action OnFavoritesChanged;

            // Constants
            private const float CardHeight = 48f;
            private const float IconSize = 40f;
            private const float Padding = 4f;
            private const float IconTextSpacing = 8f;

#endregion

#region Unity Methods

            public static void ShowWindow()
            {
                  var window = GetWindow<FavoritesWindow>("Favorites");
                  window.minSize = new Vector2(300, 400);
            }

            private void OnEnable()
            {
                  _manager = FavoritesManager.Instance;
                  _currentListIndex = _manager.lastUsedListIndex;
                  LoadCurrentList();
                  OnFavoritesChanged += UpdateFavoritesCache;
            }

            private void OnDisable()
            {
                  SaveState();
                  OnFavoritesChanged -= UpdateFavoritesCache;
            }

            private void OnGUI()
            {
                  if (!ValidateManager())
                  {
                        return;
                  }

                  InitStylesIfNeeded();

                  HandleWindowLevelEvents();

                  DrawHeader();

                  if (_currentList != null)
                  {
                        DrawListSettingsHeader();
                  }

                  DrawSearchBar();
                  EditorGUILayout.Space(5);

                  DrawContent();
            }

#endregion

#region Initialization and State Management

            private bool ValidateManager()
            {
                  if (_manager)
                  {
                        return true;
                  }

                  _manager = FavoritesManager.Instance;

                  if (_manager)
                  {
                        return true;
                  }

                  EditorGUILayout.HelpBox("FavoritesManager not found.", MessageType.Error);

                  return false;
            }

            private void SaveState()
            {
                  if (_manager != null)
                  {
                        _manager.lastUsedListIndex = _currentListIndex;
                        EditorUtility.SetDirty(_manager);
                  }
            }

            private void InitStylesIfNeeded()
            {
                  if (_stylesInitialized)
                  {
                        return;
                  }

                  _searchFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? new GUIStyle(EditorStyles.textField);
                  _searchCancelButtonStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? new GUIStyle(EditorStyles.miniButton);

                  _listBackgroundStyle = new GUIStyle(GUI.skin.box);

                  _cardTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft };
                  _cardSubtitleStyle = new GUIStyle(EditorStyles.miniLabel) { clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft };

                  _stylesInitialized = true;
            }

#endregion

#region Data Loading & Caching

            private void LoadCurrentList()
            {
                  _currentList = GetValidCurrentList();
                  UpdateFavoritesCache();
            }

            private FavoriteList GetValidCurrentList()
            {
                  if (_manager.allLists.Count == 0)
                  {
                        return null;
                  }

                  _currentListIndex = Mathf.Clamp(_currentListIndex, 0, _manager.allLists.Count - 1);

                  return _manager.allLists[_currentListIndex];
            }

            private void UpdateFavoritesCache()
            {
                  _cachedFavorites.Clear();

                  if (_currentList?.items != null)
                  {
                        foreach (FavoriteItem item in _currentList.items)
                        {
                              CachedFavorite cachedItem = CreateCachedFavorite(item);

                              if (cachedItem != null)
                              {
                                    _cachedFavorites.Add(cachedItem);
                              }
                        }
                  }

                  FilterFavorites();
                  Repaint();
            }

            private static CachedFavorite CreateCachedFavorite(FavoriteItem item)
            {
                  string path = AssetDatabase.GUIDToAssetPath(item.guid);

                  if (string.IsNullOrEmpty(path))
                  {
                        return null;
                  }

                  var obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                  if (obj == null)
                  {
                        return null;
                  }

                  Texture icon = AssetPreview.GetMiniThumbnail(obj);

                  icon = icon ? icon : EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;

                  return new CachedFavorite
                  {
                              SourceItem = item,
                              AssetObject = obj,
                              AssetIcon = icon,
                              AssetTypeName = obj.GetType().Name,
                              LowerCaseAlias = item.alias.ToLowerInvariant()
                  };
            }

            private void FilterFavorites()
            {
                  _filteredFavorites.Clear();

                  if (string.IsNullOrEmpty(_searchQuery))
                  {
                        _filteredFavorites.AddRange(_cachedFavorites);
                  }
                  else
                  {
                        string lowerQuery = _searchQuery.ToLowerInvariant();

                        _filteredFavorites.AddRange(_cachedFavorites.Where(item =>
                                    item.LowerCaseAlias.Contains(lowerQuery, StringComparison.Ordinal) ||
                                    item.AssetTypeName.ToLowerInvariant().Contains(lowerQuery, StringComparison.Ordinal)));
                  }
            }

#endregion

#region UI Drawing

            private void DrawHeader()
            {
                  using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                  {
                        DrawListSelector();
                        DrawListNavigation();
                        GUILayout.FlexibleSpace();
                        DrawAddListButton();
                  }
            }

            private void DrawListSelector()
            {
                  if (_manager.allLists.Count > 0)
                  {
                        string[] listNames = _manager.allLists.Select(static l => l.name).ToArray();
                        EditorGUI.BeginChangeCheck();
                        _currentListIndex = EditorGUILayout.Popup(_currentListIndex, listNames, EditorStyles.toolbarPopup, GUILayout.Width(150));

                        if (EditorGUI.EndChangeCheck())
                        {
                              LoadCurrentList();
                        }
                  }
                  else
                  {
                        GUILayout.Label("No Lists", EditorStyles.toolbarButton);
                  }
            }

            private void DrawListNavigation()
            {
                  if (_manager.allLists.Count <= 1)
                  {
                        return;
                  }

                  if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                  {
                        _currentListIndex = (_currentListIndex - 1 + _manager.allLists.Count) % _manager.allLists.Count;
                        LoadCurrentList();
                  }

                  if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_next"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                  {
                        _currentListIndex = (_currentListIndex + 1) % _manager.allLists.Count;
                        LoadCurrentList();
                  }
            }

            private void DrawAddListButton()
            {
                  if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.toolbarButton))
                  {
                        CreateNewList();
                  }
            }

            private void DrawListSettingsHeader()
            {
                  using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                  {
                        EditorGUI.BeginChangeCheck();
                        string newName = EditorGUILayout.TextField("List Name", _currentList.name);

                        if (EditorGUI.EndChangeCheck())
                        {
                              Undo.RecordObject(_manager, "Rename List");
                              _currentList.name = newName;
                              EditorUtility.SetDirty(_manager);
                        }

                        if (GUILayout.Button("Delete This List", EditorStyles.miniButton) &&
                            EditorUtility.DisplayDialog("Delete List?", $"Are you sure you want to delete '{_currentList.name}'?", "Yes", "No"))
                        {
                              DeleteCurrentList();
                        }
                  }
            }

            private void DrawSearchBar()
            {
                  using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                  {
                        EditorGUI.BeginChangeCheck();
                        _searchQuery = EditorGUILayout.TextField(_searchQuery, _searchFieldStyle);

                        if (EditorGUI.EndChangeCheck())
                        {
                              FilterFavorites();
                        }

                        if (GUILayout.Button("x", _searchCancelButtonStyle))
                        {
                              _searchQuery = "";
                              GUI.FocusControl(null);
                              FilterFavorites();
                        }
                  }
            }

            private void DrawContent()
            {
                  using var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandHeight(true));
                  _scrollPosition = scrollView.scrollPosition;

                  if (_currentList == null)
                  {
                        EditorGUILayout.HelpBox("No Favorites List found. Create one via the '+' button.", MessageType.Info);

                        return;
                  }

                  if (_filteredFavorites.Count == 0)
                  {
                        string message = string.IsNullOrEmpty(_searchQuery) ? "This list is empty. Drag assets here to add them." : $"No results for '{_searchQuery}'.";
                        EditorGUILayout.HelpBox(message, MessageType.Info);
                  }
                  else
                  {
                        DrawFavoritesList();
                  }
            }

            private void DrawFavoritesList()
            {
                  Rect dropArea = GUILayoutUtility.GetRect(0, _filteredFavorites.Count * (CardHeight + Padding) + Padding * 2, GUILayout.ExpandWidth(true));
                  GUI.Box(dropArea, GUIContent.none, _listBackgroundStyle);

                  for (int i = 0; i < _filteredFavorites.Count; i++)
                  {
                        CachedFavorite cachedItem = _filteredFavorites[i];

                        var cardRect = new Rect(dropArea.x + Padding, dropArea.y + Padding + i * (CardHeight + Padding), dropArea.width - Padding * 2, CardHeight);
                        DrawItemCard(cardRect, cachedItem, i);
                  }

                  DrawReorderDropIndicator(dropArea);
            }

            private void DrawItemCard(Rect cardRect, CachedFavorite cachedItem, int index)
            {
                  bool isHover = cardRect.Contains(Event.current.mousePosition);
                  bool isReorderingSource = _reorderDragIndex == index;

                  DrawCardBackground(cardRect, isHover, isReorderingSource);
                  DrawCardContent(cardRect, cachedItem, index);
                  HandleCardEvents(cardRect, cachedItem, index);

                  if (isHover)
                  {
                        Repaint();
                  }
            }

            private static void DrawCardBackground(Rect cardRect, bool isHover, bool isReorderingSource)
            {
                  Color bgColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.85f, 0.85f, 0.85f);

                  if (isReorderingSource)
                  {
                        bgColor = Color.blue * 0.5f;
                  }
                  else if (isHover)
                  {
                        bgColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : Color.white;
                  }

                  EditorGUI.DrawRect(cardRect, bgColor);

                  var borderRect = new Rect(cardRect.x - 1, cardRect.y - 1, cardRect.width + 2, cardRect.height + 2);
                  EditorGUI.DrawRect(borderRect, Color.black * 0.3f);
            }

            private void DrawCardContent(Rect cardRect, CachedFavorite cachedItem, int index)
            {
                  var iconRect = new Rect(cardRect.x + Padding, cardRect.y + (cardRect.height - IconSize) / 2, IconSize, IconSize);

                  if (cachedItem.AssetIcon)
                  {
                        GUI.DrawTexture(iconRect, cachedItem.AssetIcon);
                  }

                  var textBlockRect = new Rect(iconRect.xMax + IconTextSpacing, cardRect.y, cardRect.width - IconSize - Padding * 2 - IconTextSpacing, cardRect.height);

                  if (_editingAliasIndex == index)
                  {
                        GUI.SetNextControlName("AliasTextField");
                        _aliasEditString = EditorGUI.TextField(textBlockRect, _aliasEditString, _cardTitleStyle);
                  }
                  else
                  {
                        var titleRect = new Rect(textBlockRect.x, textBlockRect.y, textBlockRect.width, textBlockRect.height * 0.6f);
                        var subtitleRect = new Rect(textBlockRect.x, textBlockRect.y + titleRect.height, textBlockRect.width, textBlockRect.height * 0.4f);

                        EditorGUI.LabelField(titleRect, new GUIContent(cachedItem.SourceItem.alias, cachedItem.AssetObject.name), _cardTitleStyle);
                        EditorGUI.LabelField(subtitleRect, cachedItem.AssetTypeName, _cardSubtitleStyle);
                  }
            }

            private void DrawReorderDropIndicator(Rect dropArea)
            {
                  if (_reorderDropIndex == -1)
                  {
                        return;
                  }

                  float y = _reorderDropIndex < _filteredFavorites.Count
                              ? dropArea.y + Padding + _reorderDropIndex * (CardHeight + Padding) - Padding / 2f
                              : dropArea.y + Padding + _filteredFavorites.Count * (CardHeight + Padding) - Padding / 2f;

                  var indicatorRect = new Rect(dropArea.x, y - 1, dropArea.width, 2);
                  EditorGUI.DrawRect(indicatorRect, Color.cyan);
            }

#endregion

#region Event Handling & Actions

            private void HandleWindowLevelEvents()
            {
                  Event e = Event.current;

                  if (e.type is EventType.DragUpdated or EventType.DragPerform && DragAndDrop.GetGenericData("FavoriteReorder") == null && HasValidDraggedAssets())
                  {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (e.type == EventType.DragPerform)
                        {
                              ProcessDraggedAssets();
                        }

                        e.Use();
                  }
            }

            private void HandleCardEvents(Rect cardRect, CachedFavorite cachedItem, int index)
            {
                  Event e = Event.current;

                  if (!cardRect.Contains(e.mousePosition))
                  {
                        return;
                  }

                  switch (e.type)
                  {
                        case EventType.MouseDown when e.button == 0:
                              if (e.clickCount == 1 && string.IsNullOrEmpty(_searchQuery))
                              {
                                    DragAndDrop.PrepareStartDrag();
                                    DragAndDrop.SetGenericData("FavoriteReorder", index);
                                    DragAndDrop.objectReferences = new[] { cachedItem.AssetObject };
                                    _reorderDragIndex = index;
                                    e.Use();
                              }
                              else if (e.clickCount == 2)
                              {
                                    AssetDatabase.OpenAsset(cachedItem.AssetObject);
                                    e.Use();
                              }

                              break;

                        case EventType.MouseUp when e.button == 0:
                              if (_reorderDragIndex != -1)
                              {
                                    DragAndDrop.SetGenericData("FavoriteReorder", null);
                                    _reorderDragIndex = -1;
                                    _reorderDropIndex = -1;
                                    Repaint();
                              }

                              break;

                        case EventType.MouseDrag:
                              if (_reorderDragIndex != -1)
                              {
                                    DragAndDrop.StartDrag(cachedItem.SourceItem.alias);
                                    e.Use();
                              }

                              break;

                        case EventType.DragUpdated:
                              if (DragAndDrop.GetGenericData("FavoriteReorder") != null)
                              {
                                    _reorderDropIndex = e.mousePosition.y > cardRect.y + cardRect.height / 2 ? index + 1 : index;
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                                    Repaint();
                                    e.Use();
                              }

                              break;

                        case EventType.DragPerform:
                              if (DragAndDrop.GetGenericData("FavoriteReorder") != null)
                              {
                                    DragAndDrop.AcceptDrag();
                                    CompleteReorder();
                                    e.Use();
                              }

                              break;

                        case EventType.KeyUp when e.keyCode == KeyCode.Return && _editingAliasIndex == index:
                              CommitAliasChange();
                              e.Use();

                              break;

                        case EventType.ContextClick:
                              ShowContextMenu(cachedItem, index);
                              e.Use();

                              break;
                  }
            }

            private void CompleteReorder()
            {
                  if (_reorderDragIndex != -1 && _reorderDropIndex != -1 && _reorderDragIndex != _reorderDropIndex)
                  {
                        FavoriteItem draggedItem = _filteredFavorites[_reorderDragIndex].SourceItem;

                        int sourceListDragIndex = _currentList.items.IndexOf(draggedItem);

                        FavoriteItem dropItem = (_reorderDropIndex < _filteredFavorites.Count && _reorderDropIndex != _reorderDragIndex)
                                    ? _filteredFavorites[_reorderDropIndex].SourceItem
                                    : null;
                        int sourceListDropIndex = (dropItem != null) ? _currentList.items.IndexOf(dropItem) : _currentList.items.Count;

                        if (sourceListDragIndex < sourceListDropIndex)
                        {
                              sourceListDropIndex--;
                        }

                        Undo.RecordObject(_manager, "Reorder Favorite");
                        _currentList.items.RemoveAt(sourceListDragIndex);
                        _currentList.items.Insert(sourceListDropIndex, draggedItem);
                        EditorUtility.SetDirty(_manager);

                        OnFavoritesChanged?.Invoke();
                  }

                  DragAndDrop.SetGenericData("FavoriteReorder", null);
                  _reorderDragIndex = -1;
                  _reorderDropIndex = -1;
            }

            private void ShowContextMenu(CachedFavorite cachedItem, int index)
            {
                  var menu = new GenericMenu();

                  menu.AddItem(new GUIContent("Edit Alias"), false, () => StartEditingAlias(index));
                  menu.AddItem(new GUIContent("Remove"), false, () => RemoveFavorite(cachedItem));
                  menu.ShowAsContext();
            }

            private void StartEditingAlias(int index)
            {
                  _editingAliasIndex = index;
                  _aliasEditString = _filteredFavorites[index].SourceItem.alias;
                  EditorApplication.delayCall += static () => EditorGUI.FocusTextInControl("AliasTextField");
            }

            private void CommitAliasChange()
            {
                  if (_editingAliasIndex == -1)
                  {
                        return;
                  }

                  FavoriteItem item = _filteredFavorites[_editingAliasIndex].SourceItem;

                  if (item.alias != _aliasEditString)
                  {
                        Undo.RecordObject(_manager, "Change Favorite Alias");
                        item.alias = _aliasEditString;
                        EditorUtility.SetDirty(_manager);
                        OnFavoritesChanged?.Invoke();
                  }

                  _editingAliasIndex = -1;
                  GUI.FocusControl(null);
                  Repaint();
            }

            private void CreateNewList()
            {
                  Undo.RecordObject(_manager, "Add New List");
                  _manager.allLists.Add(new FavoriteList { name = $"New List {_manager.allLists.Count + 1}" });
                  _currentListIndex = _manager.allLists.Count - 1;
                  LoadCurrentList();
                  EditorUtility.SetDirty(_manager);
            }

            private void DeleteCurrentList()
            {
                  Undo.RecordObject(_manager, "Delete List");
                  _manager.allLists.RemoveAt(_currentListIndex);
                  _currentListIndex = Mathf.Max(0, _currentListIndex - 1);
                  LoadCurrentList();
                  EditorUtility.SetDirty(_manager);
            }

            private void RemoveFavorite(CachedFavorite cachedItem)
            {
                  if (_editingAliasIndex != -1 && _filteredFavorites[_editingAliasIndex] == cachedItem)
                  {
                        _editingAliasIndex = -1;
                  }

                  Undo.RecordObject(_manager, "Remove Favorite");
                  _currentList.items.Remove(cachedItem.SourceItem);
                  EditorUtility.SetDirty(_manager);

                  OnFavoritesChanged?.Invoke();
            }

            private static bool HasValidDraggedAssets() =>
                        DragAndDrop.objectReferences.Any(static o => o != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)));

            private void ProcessDraggedAssets()
            {
                  DragAndDrop.AcceptDrag();
                  Undo.RecordObject(_manager, "Add Favorites");

                  foreach (Object draggedObject in DragAndDrop.objectReferences)
                  {
                        AddObjectToFavorites(draggedObject);
                  }

                  EditorUtility.SetDirty(_manager);
                  OnFavoritesChanged?.Invoke();
            }

            private void AddObjectToFavorites(Object obj)
            {
                  string path = AssetDatabase.GetAssetPath(obj);

                  if (string.IsNullOrEmpty(path))
                  {
                        return;
                  }

                  string guid = AssetDatabase.AssetPathToGUID(path);

                  if (_currentList.items.Any(item => item.guid == guid))
                  {
                        Debug.LogWarning($"L'asset '{obj.name}' est déjà dans cette liste de favoris.");

                        return;
                  }

                  _currentList.items.Add(new FavoriteItem(guid, obj.name));
            }

#endregion
      }
}