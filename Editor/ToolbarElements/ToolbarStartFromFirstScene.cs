using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarStartFromFirstScene : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            protected override string Name => "Start From First Scene";
            protected override string Tooltip => "Saves changes, starts Play Mode from the first scene in Build Settings, and returns to the original scene on exit.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_PlayButton@2x").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);

                  this.Enabled = !EditorApplication.isPlayingOrWillChangePlaymode &&
                                 SceneUtility.GetBuildIndexByScenePath(SceneUtility.GetScenePathByBuildIndex(0)) != -1;
            }

            public override void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                  this.Enabled = (state == PlayModeStateChange.EnteredEditMode);
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              SceneAssetsUtils.StartPlayModeFromFirstScene();
                        }
                  }
            }
      }
}