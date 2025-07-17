using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      [InitializeOnLoad]
      public static class ToolbarExtender
      {
            private static int mToolCount;
            private static GUIStyle mCommandStyle;

            public readonly static List<Action> LeftToolbarGUI = new();
            public readonly static List<Action> RightToolbarGUI = new();

            static ToolbarExtender()
            {
                  EditorApplication.update += Init;
                  EditorApplication.playModeStateChanged += OnChangePlayMode;
            }

            private static void OnChangePlayMode(PlayModeStateChange state)
            {
                  if (state == PlayModeStateChange.EnteredPlayMode)
                  {
                        InitElements();
                  }
            }

            private static void Init()
            {
                  EditorApplication.update -= Init;
                  InitElements();
            }

            private static void InitElements()
            {
                  Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

                  const string fieldName = "k_ToolCount";

                  FieldInfo toolIcons = toolbarType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                  mToolCount = toolIcons != null ? ((int)toolIcons.GetValue(null)) : 8;

                  ToolbarCallback.OnToolbarGUI -= OnGUI;
                  ToolbarCallback.OnToolbarGUI += OnGUI;
                  ToolbarCallback.OnToolbarGUILeft -= GUILeft;
                  ToolbarCallback.OnToolbarGUILeft += GUILeft;
                  ToolbarCallback.OnToolbarGUIRight -= GUIRight;
                  ToolbarCallback.OnToolbarGUIRight += GUIRight;
            }

            public const float Space = 8;
            public const float LargeSpace = 20;
            public const float ButtonWidth = 32;
            public const float DropdownWidth = 80;
            public const float PlayPauseStopWidth = 140;

            public static void OnGUI()
            {
                  mCommandStyle ??= new GUIStyle("CommandLeft");

                  float screenWidth = EditorGUIUtility.currentViewWidth;

                  float playButtonsPosition = Mathf.RoundToInt((screenWidth - PlayPauseStopWidth) / 2);

                  var leftRect = new Rect(0, 0, screenWidth, Screen.height);
                  leftRect.xMin += Space; // Spacing left
                  leftRect.xMin += ButtonWidth * mToolCount; // Tool buttons
                  leftRect.xMin += Space; // Spacing between tools and pivot
                  leftRect.xMin += 64 * 2; // Pivot buttons
                  leftRect.xMax = playButtonsPosition;

                  var rightRect = new Rect(0, 0, screenWidth, Screen.height);
                  rightRect.xMin = playButtonsPosition;
                  rightRect.xMin += mCommandStyle.fixedWidth * 3; // Play buttons
                  rightRect.xMax = screenWidth;
                  rightRect.xMax -= Space; // Spacing right
                  rightRect.xMax -= DropdownWidth; // Layout
                  rightRect.xMax -= Space; // Spacing between layout and layers
                  rightRect.xMax -= DropdownWidth; // Layers
                  rightRect.xMax -= Space; // Spacing between layers and account
                  rightRect.xMax -= DropdownWidth; // Account
                  rightRect.xMax -= Space; // Spacing between account and cloud
                  rightRect.xMax -= ButtonWidth; // Cloud
                  rightRect.xMax -= Space; // Spacing between cloud and collab
                  rightRect.xMax -= 78; // Colab

                  // Add spacing around existing controls
                  leftRect.xMin += Space;
                  leftRect.xMax -= Space;
                  rightRect.xMin += Space;
                  rightRect.xMax -= Space;

                  // Add top and bottom margins
                  leftRect.y = 4;
                  leftRect.height = 22;
                  rightRect.y = 4;
                  rightRect.height = 22;

                  if (leftRect.width > 0)
                  {
                        GUILayout.BeginArea(leftRect);
                        GUILayout.BeginHorizontal();

                        foreach (Action handler in LeftToolbarGUI)
                        {
                              handler();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.EndArea();
                  }

                  if (rightRect.width > 0)
                  {
                        GUILayout.BeginArea(rightRect);
                        GUILayout.BeginHorizontal();

                        foreach (Action handler in RightToolbarGUI)
                        {
                              handler();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.EndArea();
                  }
            }

            private static void GUILeft()
            {
                  GUILayout.BeginHorizontal();

                  foreach (Action handler in LeftToolbarGUI)
                  {
                        handler();
                  }

                  GUILayout.EndHorizontal();
            }

            private static void GUIRight()
            {
                  GUILayout.BeginHorizontal();

                  foreach (Action handler in RightToolbarGUI)
                  {
                        handler();
                  }

                  GUILayout.EndHorizontal();
            }
      }
}