using System;
using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarReserializeAll : BaseToolbarElement
      {
            private static GUIContent buttonContent;

            public override string Name => "Reserialize All Assets";
            public override string Tooltip => "Forces a re-serialization of all assets in the project. Useful after a Unity upgrade or to fix serialization errors.";

            public override void OnInit()
            {
                  buttonContent = EditorGUIUtility.IconContent("d_Refresh", this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  this.Enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              Debug.Log("Starting to force reserialize all assets. This may take a while...");
                              SerializeAssetsUtils.ForceReserializeAllAssets();
                        }
                  }
            }
      }
}