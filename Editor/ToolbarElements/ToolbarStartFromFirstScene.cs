using System;
using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarStartFromFirstScene : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            public override string Name => "Start From First Scene";
            public override string Tooltip => "Saves changes, starts Play Mode from the first scene in Build Settings, and returns to the original scene on exit.";

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