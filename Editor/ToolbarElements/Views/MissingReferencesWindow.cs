using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.ToolbarElements.Data;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements.Views
{
      sealed internal class MissingReferencesWindow : EditorWindow
      {
            private Dictionary<GameObject, List<MissingReferenceInfo>> _results;
            private Vector2 _scrollPosition;

            private GUIStyle _headerLabelStyle;
            private GUIStyle _cardStyle;
            private GUIStyle _objectNameFoldoutStyle;
            private GUIStyle _infoTextStyle;
            private GUIStyle _errorTextStyle;
            private GUIStyle _scriptGroupStyle;
            private Texture2D _headerBackground;
            private Texture2D _cardBackground;
            private GUIContent _gameObjectIcon;
            private GUIContent _scriptIcon;
            private GUIContent _fieldIcon;
            private readonly Dictionary<GameObject, bool> _gameObjectFoldouts = new();

            public static void ShowWindow(Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  var window = GetWindow<MissingReferencesWindow>("Missing References");
                  window.minSize = new Vector2(600, 400);
                  window.Setup(results);
                  window.Show();
            }

            private void Setup(Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  _results = results.OrderBy(static kv => kv.Key.name).ToDictionary(static kv => kv.Key, static kv => kv.Value);
                  _gameObjectFoldouts.Clear();

                  foreach (GameObject key in _results.Keys)
                  {
                        _gameObjectFoldouts[key] = true;
                  }

                  InitializeStyles();
            }

            private void InitializeStyles()
            {
                  if (_headerLabelStyle != null)
                  {
                        return;
                  }

                  _headerBackground = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f));
                  _cardBackground = MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f));

                  _headerLabelStyle = new GUIStyle(EditorStyles.largeLabel)
                  {
                              fontSize = 16, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 8, 8),
                              normal = { textColor = Color.white }
                  };

                  _cardStyle = new GUIStyle(GUI.skin.box)
                              { padding = new RectOffset(10, 10, 10, 10), margin = new RectOffset(10, 10, 5, 5), normal = { background = _cardBackground } };
                  _objectNameFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontSize = 14, fontStyle = FontStyle.Bold, richText = true, fixedHeight = 30 };
                  _scriptGroupStyle = new GUIStyle(EditorStyles.label) { richText = true, padding = { left = 18 } };
                  _infoTextStyle = new GUIStyle(EditorStyles.label) { richText = true, padding = { left = 36 } };
                  _errorTextStyle = new GUIStyle(_infoTextStyle) { normal = { textColor = new Color(1f, 0.6f, 0.6f) } };

                  _gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon");
                  _scriptIcon = EditorGUIUtility.IconContent("cs Script Icon");
                  _fieldIcon = EditorGUIUtility.IconContent("DotSelection");
            }

            private void OnGUI()
            {
                  if (_results == null)
                  {
                        return;
                  }

                  DrawHeader();
                  DrawContent();
            }

            private void DrawHeader()
            {
                  Rect headerRect = GUILayoutUtility.GetRect(position.width, 40);
                  GUI.DrawTexture(headerRect, _headerBackground, ScaleMode.StretchToFill);
                  int totalProblems = _results.Values.Sum(static list => list.Count);
                  string headerText = totalProblems > 0 ? $"{totalProblems} Issue(s) Found on {_results.Count} Object(s)" : "No Missing References!";
                  EditorGUI.LabelField(headerRect, headerText, _headerLabelStyle);
            }

            private void DrawContent()
            {
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                  if (_results.Count == 0)
                  {
                        EditorGUILayout.HelpBox("Scene is clean!", MessageType.Info, true);
                  }
                  else
                  {
                        foreach (KeyValuePair<GameObject, List<MissingReferenceInfo>> kvp in _results)
                        {
                              EditorGUILayout.BeginVertical(_cardStyle);
                              DrawResultCard(kvp.Key, kvp.Value);
                              EditorGUILayout.EndVertical();
                        }
                  }

                  EditorGUILayout.EndScrollView();
            }

            private void DrawResultCard(GameObject go, List<MissingReferenceInfo> infos)
            {
                  EditorGUILayout.BeginHorizontal();

                  _gameObjectIcon.text = $" {go.name} ({infos.Count} issue(s))";
                  _gameObjectFoldouts[go] = EditorGUILayout.Foldout(_gameObjectFoldouts[go], _gameObjectIcon, true, _objectNameFoldoutStyle);

                  GUILayout.FlexibleSpace();

                  if (GUILayout.Button("Select", GUILayout.Width(80), GUILayout.Height(28)))
                  {
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);
                  }

                  EditorGUILayout.EndHorizontal();

                  if (_gameObjectFoldouts[go])
                  {
                        GUILayout.Space(10);

                        IEnumerable<IGrouping<string, MissingReferenceInfo>> groupedByComponent =
                                    infos.GroupBy(static info => info.IsScriptMissing ? "Missing Script" : info.ComponentName);

                        foreach (IGrouping<string, MissingReferenceInfo> group in groupedByComponent)
                        {
                              if (group.Key == "Missing Script")
                              {
                                    _scriptIcon.text = " <color=orange><b>Missing Script</b></color>";
                                    EditorGUILayout.LabelField(_scriptIcon, _errorTextStyle);
                              }
                              else
                              {
                                    _scriptIcon.text = $" Script: <b>{group.Key}</b>";
                                    EditorGUILayout.LabelField(_scriptIcon, _scriptGroupStyle);

                                    foreach (MissingReferenceInfo info in group)
                                    {
                                          _fieldIcon.text = $" Field: <i>{info.FieldName}</i>";
                                          EditorGUILayout.LabelField(_fieldIcon, _errorTextStyle);
                                    }
                              }

                              GUILayout.Space(5);
                        }
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

            private void OnDestroy()
            {
                  if (_headerBackground != null)
                  {
                        DestroyImmediate(_headerBackground);
                  }

                  if (_cardBackground != null)
                  {
                        DestroyImmediate(_cardBackground);
                  }
            }
      }
}