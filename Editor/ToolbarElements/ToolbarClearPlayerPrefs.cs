using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarClearPlayerPrefs : BaseToolbarElement
      {
            private GUIContent _buttonContent;

            protected override string Name => "Clear PlayerPrefs";
            protected override string Tooltip => "Deletes all keys and values from PlayerPrefs. This cannot be undone.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_TreeEditor.Trash").image;
                  _buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  if (GUILayout.Button(_buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)) && EditorUtility.DisplayDialog("Clear PlayerPrefs",
                                  "Are you sure you want to delete all PlayerPrefs? This action cannot be undone.", "Yes, delete them", "Cancel"))
                  {
                        PlayerPrefs.DeleteAll();
                        Debug.Log("PlayerPrefs cleared successfully.");
                  }
            }
      }
}