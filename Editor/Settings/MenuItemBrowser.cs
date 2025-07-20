using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      public sealed class MenuItemBrowser : EditorWindow
      {
            sealed private class Node
            {
                  public string Name;
                  public string Path;
                  public readonly List<Node> Children = new();
                  public bool IsExpanded;
            }

            private Action<string> _onItemSelected;
            private Node _rootNode;
            private Vector2 _scrollPosition;
            private string _searchText = "";

            private GUIStyle _folderStyle;
            private GUIStyle _itemStyle;
            private GUIStyle _searchFieldStyle;
            private readonly Color _hoverColor = new(0.35f, 0.35f, 0.35f, 1f);
            private readonly Color _headerColor = new(0.22f, 0.22f, 0.22f, 1f);

            public static void Show(Action<string> onItemSelected)
            {
                  var window = GetWindow<MenuItemBrowser>(true, "Menu Item Browser", true);
                  window._onItemSelected = onItemSelected;
                  window.minSize = new Vector2(400, 500);
            }

            private void OnEnable()
            {
                  InitStyles();

                  _rootNode = new Node { Name = "Root" };

                  Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                  var menuItems = (from assembly in assemblies
                              from type in assembly.GetTypes()
                              from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                              select method.GetCustomAttributes(typeof(MenuItem), false)
                              into attributes
                              where attributes.Length > 0
                              from MenuItem item in attributes
                              where !string.IsNullOrEmpty(item.menuItem) && !item.menuItem.StartsWith("CONTEXT/", StringComparison.Ordinal) &&
                                    !item.menuItem.StartsWith("internal:", StringComparison.Ordinal)
                              select item.menuItem).ToList();

                  foreach (string path in menuItems.Distinct().OrderBy(static s => s))
                  {
                        string[] parts = path.Split('/');
                        Node currentNode = _rootNode;

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
            }

            private void OnGUI()
            {
                  var headerRect = new Rect(0, 0, position.width, 40);
                  EditorGUI.DrawRect(headerRect, _headerColor);

                  GUILayout.BeginArea(headerRect);
                  GUILayout.Space(10);
                  _searchText = EditorGUILayout.TextField(_searchText, _searchFieldStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20), GUILayout.ExpandWidth(true));
                  GUILayout.EndArea();

                  var contentRect = new Rect(0, 40, position.width, position.height - 40);
                  GUILayout.BeginArea(contentRect);
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                  foreach (Node node in _rootNode.Children)
                  {
                        DrawNode(node, 0);
                  }

                  EditorGUILayout.EndScrollView();
                  GUILayout.EndArea();

                  if (Event.current.type == EventType.MouseMove)
                  {
                        Repaint();
                  }
            }

            private void InitStyles()
            {
                  _folderStyle = new GUIStyle(EditorStyles.foldout)
                  {
                              fontStyle = FontStyle.Bold,
                              fontSize = 13,
                              padding = new RectOffset(15, 0, 3, 3),
                              margin = new RectOffset(0, 0, 2, 2)
                  };

                  _itemStyle = new GUIStyle(EditorStyles.label)
                  {
                              padding = new RectOffset(15, 0, 3, 3),
                              margin = new RectOffset(0, 0, 2, 2)
                  };

                  _searchFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField);
            }

            private void DrawNode(Node node, int indentLevel)
            {
                  if (!string.IsNullOrEmpty(_searchText) && !IsNodeVisibleInSearch(node))
                  {
                        return;
                  }

                  Rect fullRect = EditorGUILayout.GetControlRect(false, 22);

                  fullRect.x += indentLevel * 15;
                  fullRect.width -= indentLevel * 15;

                  if (fullRect.Contains(Event.current.mousePosition))
                  {
                        EditorGUI.DrawRect(fullRect, _hoverColor);
                  }

                  if (node.Children.Count > 0 && node.Path == null)
                  {
                        bool isExpanded = !string.IsNullOrEmpty(_searchText) || node.IsExpanded;
                        node.IsExpanded = EditorGUI.Foldout(fullRect, isExpanded, node.Name, true, _folderStyle);

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
                        if (GUI.Button(fullRect, node.Name, _itemStyle))
                        {
                              ItemSelected(node.Path);
                        }
                  }
            }

            private static string CleanMenuItemName(string name)
            {
                  return Regex.Replace(name, @"\s+[_%#&].*$", "").Trim();
            }

            private bool IsNodeVisibleInSearch(Node node)
            {
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
                  this.Close();
            }
      }
}