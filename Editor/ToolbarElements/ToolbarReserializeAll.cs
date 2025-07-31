using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarReserializeAll : BaseToolbarElement
      {
            private GUIContent buttonContent;

            protected override string Name => "Reserialize All Assets";
            protected override string Tooltip => "Forces a re-serialization of all assets in the project. Useful after a Unity upgrade or to fix serialization errors.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_Refresh").image;

                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  this.Enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              Debug.Log("Starting to force reserialize all assets...");
                              SerializeAssetsUtils.ForceReserializeAllAssets();
                        }
                  }
            }
      }
}