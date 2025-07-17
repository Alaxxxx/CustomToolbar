using System;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarRecompile : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            public override string Name => "Recompile Scripts";
            public override string Tooltip => "Request a manual script compilation.";

            public override void OnInit()
            {
                  buttonContent = EditorGUIUtility.IconContent("d_BuildSettings.Metro", "Recompile Scripts");
            }

            public override void OnDrawInToolbar()
            {
                  this.Enabled = !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode;

                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              CompilationPipeline.RequestScriptCompilation();
                        }
                  }
            }
      }
}