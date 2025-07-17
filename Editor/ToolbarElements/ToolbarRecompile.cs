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
            private static GUIContent recompileBtn;

            public override string NameInList => "[Button] Recompile";
            public override int SortingGroup => 5;

            public override void Init()
            {
                  recompileBtn = EditorGUIUtility.IconContent("WaitSpin05");
                  recompileBtn.tooltip = "Recompile";
            }

            protected override void OnDrawInList(Rect position)
            {
            }

            protected override void OnDrawInToolbar()
            {
                  if (GUILayout.Button(recompileBtn, ToolbarStyles.commandButtonStyle))
                  {
                        CompilationPipeline.RequestScriptCompilation();
                        Debug.Log("Recompile");
                  }
            }
      }
}