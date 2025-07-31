using System;
using System.IO;
using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarScreenshot : BaseToolbarElement
      {
            private GUIContent buttonContent;
            private const string ScreenshotFolderPath = "Screenshots";

            protected override string Name => "Screenshot";
            protected override string Tooltip => "Screenshot options";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_FrameCapture").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                  {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Capture Game View"), false, CaptureGameView);
                        menu.AddItem(new GUIContent("Capture Scene View"), false, CaptureSceneView);
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Open Screenshots Folder"), false, OpenScreenshotsFolder);
                        menu.ShowAsContext();
                  }
            }

            private static void OpenScreenshotsFolder()
            {
                  EnsureFolderExists();

                  Application.OpenURL(ScreenshotFolderPath);
            }

            private static void CaptureGameView()
            {
                  EnsureFolderExists();
                  string fullPath = GetUniqueScreenshotPath();
                  ScreenCapture.CaptureScreenshot(fullPath);

                  EditorApplication.delayCall += () => { LogScreenshot(fullPath); };
            }

            private static void CaptureSceneView()
            {
                  var sceneView = SceneView.lastActiveSceneView;

                  if (sceneView == null)
                  {
                        Debug.LogWarning("[CustomToolbar] Cannot capture Scene View: no scene view is currently active or focused.");

                        return;
                  }

                  Camera sceneCamera = sceneView.camera;

                  var renderTexture = new RenderTexture(sceneCamera.pixelWidth, sceneCamera.pixelHeight, 24);
                  sceneCamera.targetTexture = renderTexture;

                  sceneCamera.Render();

                  sceneCamera.targetTexture = null;

                  RenderTexture.active = renderTexture;
                  var texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                  texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                  texture2D.Apply();
                  RenderTexture.active = null;

                  byte[] bytes = texture2D.EncodeToPNG();
                  EnsureFolderExists();
                  string fullPath = GetUniqueScreenshotPath();
                  File.WriteAllText(fullPath, string.Empty);
                  File.WriteAllBytes(fullPath, bytes);

                  UnityEngine.Object.DestroyImmediate(texture2D);
                  UnityEngine.Object.DestroyImmediate(renderTexture);

                  LogScreenshot(fullPath);
            }

            private static void EnsureFolderExists()
            {
                  if (!Directory.Exists(ScreenshotFolderPath))
                  {
                        Directory.CreateDirectory(ScreenshotFolderPath);
                  }
            }

            private static string GetUniqueScreenshotPath()
            {
                  return Path.Combine(ScreenshotFolderPath, $"Screenshot_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss}.png");
            }

            private static void LogScreenshot(string path)
            {
                  AssetDatabase.Refresh();
                  Debug.Log($"Screenshot saved to: <a href=\"{path}\">{path}</a>");
            }
      }
}