using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarReloadScene : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            protected override string Name => "Reload Scene";
            protected override string Tooltip => "Reloads the currently active scene (only in Play Mode).";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_Refresh").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);

                  this.Enabled = false;
            }

            public override void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                  this.Enabled = EditorApplication.isPlaying;
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                  }
            }
      }
}