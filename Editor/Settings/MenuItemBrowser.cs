using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.Settings
{
      public sealed class MenuItemBrowser : EditorWindow
      {
            sealed private class Node
            {
                  public string Name { get; set; }
                  public string Path { get; set; }
                  public List<Node> Children { get; } = new();
                  public bool IsExpanded { get; set; }
            }

            // Constants
            private const float HeaderHeight = 40f;
            private const float RowHeight = 22f;
            private const float IndentWidth = 15f;

            // Regex cache
            private readonly static Regex CleanNameRegex = new(@"\s+[_%#&].*$", RegexOptions.Compiled);

            // Fields
            private Node _rootNode;
            private string _searchText = "";
            private Vector2 _scrollPosition;
            private Rect _hoveredRect;
            private Action<string> _onItemSelected;

            // Styles & Colors
            private GUIStyle _folderStyle;
            private GUIStyle _itemStyle;
            private GUIStyle _searchFieldStyle;
            private Color _hoverColor;
            private Color _headerColor;

            public static void Show(Action<string> onItemSelected)
            {
                  var window = GetWindow<MenuItemBrowser>(true, "Menu Item Browser", true);
                  window._onItemSelected = onItemSelected;
                  window.minSize = new Vector2(400, 500);
            }

#region Unity

            private void OnEnable()
            {
                  _rootNode = BuildMenuItemTree();
            }

            private void OnGUI()
            {
                  if (_folderStyle == null)
                  {
                        InitStyles();
                  }

                  DrawHeader();

                  if (Event.current.type == EventType.Layout)
                  {
                        _hoveredRect = Rect.zero;
                  }

                  GUILayout.Space(HeaderHeight);

                  DrawContent();

                  if (Event.current.type == EventType.MouseMove)
                  {
                        Repaint();
                  }
            }

#endregion

#region Drawing

            private void DrawHeader()
            {
                  var headerRect = new Rect(0, 0, this.position.width, HeaderHeight);
                  EditorGUI.DrawRect(headerRect, _headerColor);

                  var searchRect = new Rect(headerRect.x + 10, headerRect.y + 10, headerRect.width - 20, 20);
                  _searchText = EditorGUI.TextField(searchRect, _searchText, _searchFieldStyle);
            }

            private void DrawContent()
            {
                  using var scrollView = new GUILayout.ScrollViewScope(_scrollPosition);

                  _scrollPosition = scrollView.scrollPosition;

                  foreach (Node node in _rootNode.Children)
                  {
                        DrawNode(node, 0);
                  }
            }

            private void DrawNode(Node node, int indentLevel)
            {
                  if (!IsNodeVisibleInSearch(node))
                  {
                        return;
                  }

                  Rect rowRect = EditorGUILayout.GetControlRect(false, RowHeight);

                  if (rowRect.Contains(Event.current.mousePosition))
                  {
                        _hoveredRect = rowRect;
                  }

                  if (Event.current.type == EventType.Repaint && _hoveredRect == rowRect)
                  {
                        EditorGUI.DrawRect(rowRect, _hoverColor);
                  }

                  var controlRect = new Rect(rowRect.x + indentLevel * IndentWidth, rowRect.y, rowRect.width - indentLevel * IndentWidth, rowRect.height);

                  if (node.Children.Any())
                  {
                        bool isExpanded = !string.IsNullOrEmpty(_searchText) || node.IsExpanded;
                        node.IsExpanded = EditorGUI.Foldout(controlRect, isExpanded, node.Name, true, _folderStyle);

                        if (node.IsExpanded)
                        {
                              foreach (Node child in node.Children)
                              {
                                    DrawNode(child, indentLevel + 1);
                              }
                        }
                  }
                  else
                  {
                        if (GUI.Button(controlRect, node.Name, _itemStyle))
                        {
                              ItemSelected(node.Path);
                        }
                  }
            }

            private void InitStyles()
            {
                  _folderStyle = new GUIStyle(EditorStyles.foldout)
                  {
                              fontStyle = FontStyle.Bold,
                              fontSize = 13,
                              padding = new RectOffset(15, 0, 3, 3),
                  };

                  _itemStyle = new GUIStyle(EditorStyles.label)
                  {
                              padding = new RectOffset(15, 0, 3, 3),
                  };

                  _searchFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField);

                  _hoverColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                  _headerColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            }

#endregion

            private static Node BuildMenuItemTree()
            {
                  var root = new Node { Name = "Root" };
                  IEnumerable<string> menuItemPaths = GetAllValidMenuItems();

                  foreach (string path in menuItemPaths.Distinct().OrderBy(static s => s))
                  {
                        AddPathToTree(root, path);
                  }

                  return root;
            }

            private static void AddPathToTree(Node root, string path)
            {
                  Node currentNode = root;
                  string[] parts = path.Split('/');

                  for (int i = 0; i < parts.Length; i++)
                  {
                        string cleanName = CleanMenuItemName(parts[i]);
                        Node child = currentNode.Children.Find(c => c.Name == cleanName);

                        if (child == null)
                        {
                              child = new Node { Name = cleanName };

                              if (i == parts.Length - 1)
                              {
                                    child.Path = path;
                              }

                              currentNode.Children.Add(child);
                        }

                        currentNode = child;
                  }
            }

            private static IEnumerable<string> GetAllValidMenuItems()
            {
                  return AppDomain.CurrentDomain.GetAssemblies()
                                  .SelectMany(static assembly => assembly.GetTypes())
                                  .SelectMany(static type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                                  .SelectMany(static method => method.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>())
                                  .Select(static item => item.menuItem)
                                  .Where(static path => !string.IsNullOrEmpty(path) && !path.StartsWith("CONTEXT/", StringComparison.Ordinal) &&
                                                        !path.StartsWith("internal:", StringComparison.Ordinal));
            }

            private bool IsNodeVisibleInSearch(Node node)
            {
                  if (string.IsNullOrEmpty(_searchText))
                  {
                        return true;
                  }

                  if (node.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                  {
                        return true;
                  }

                  if (node.Path != null && node.Path.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                  {
                        return true;
                  }

                  return node.Children.Exists(IsNodeVisibleInSearch);
            }

            private void ItemSelected(string path)
            {
                  _onItemSelected?.Invoke(path);
                  Close();
            }

            private static string CleanMenuItemName(string name)
            {
                  return CleanNameRegex.Replace(name, "").Trim();
            }
      }
}