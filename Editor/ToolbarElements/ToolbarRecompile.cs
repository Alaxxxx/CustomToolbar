using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarRecompile : BaseToolbarElement
      {
            private GUIContent buttonContent;

            protected override string Name => "Recompile Scripts";
            protected override string Tooltip => "Request a manual script compilation.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_debug").image;

                  buttonContent = new GUIContent(icon, this.Tooltip);
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