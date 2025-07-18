using System;
using System.IO;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarScreenshot : BaseToolbarElement
      {
            private GUIContent buttonContent;

            public override string Name => "Screenshot";
            public override string Tooltip => "Takes a screenshot of the Game view.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_FrameCapture").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                  {
                        TakeScreenshot();
                  }
            }

            private static void TakeScreenshot()
            {
                  const string folderPath = "Screenshots";

                  if (!Directory.Exists(folderPath))
                  {
                        Directory.CreateDirectory(folderPath);
                  }

                  string fileName = $"Screenshot_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm-ss}.png";
                  string fullPath = Path.Combine(folderPath, fileName);

                  ScreenCapture.CaptureScreenshot(fullPath);

                  Debug.Log($"Screenshot saved to <a href=\"{fullPath}\">{fullPath}</a>", AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath));
            }
      }
}