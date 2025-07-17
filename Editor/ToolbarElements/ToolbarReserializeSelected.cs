using System;
using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarReserializeSelected : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            public override string Name => "Reserialize Selected";
            public override string Tooltip => "Forces a re-serialization of the selected assets in the Project window.";

            public override void OnInit()
            {
                  buttonContent = EditorGUIUtility.IconContent("d_Refresh", this.Tooltip);

                  UpdateEnabledState();
            }

            public override void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                  UpdateEnabledState();
            }

            public override void OnSelectionChanged()
            {
                  UpdateEnabledState();
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              Debug.Log("Forcing reserialization of selected assets...");
                              SerializeAssetsUtils.ForceReserializeSelectedAssets();
                        }
                  }
            }

            private void UpdateEnabledState()
            {
                  this.Enabled = !EditorApplication.isPlayingOrWillChangePlaymode && Selection.assetGUIDs.Length > 0;
            }
      }
}