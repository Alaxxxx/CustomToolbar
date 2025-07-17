using System;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarClearPrefs : BaseToolbarElement
      {
            private static GUIContent clearPlayerPrefsBtn;

            public override string NameInList => "[Button] Clear prefs";
            public override int SortingGroup => 4;

            public override void Init()
            {
                  clearPlayerPrefsBtn = EditorGUIUtility.IconContent("SaveFromPlay");
                  clearPlayerPrefsBtn.tooltip = "Clear player prefs";
            }

            protected override void OnDrawInList(Rect position)
            {
            }

            protected override void OnDrawInToolbar()
            {
                  if (GUILayout.Button(clearPlayerPrefsBtn, ToolbarStyles.commandButtonStyle))
                  {
                        PlayerPrefs.DeleteAll();
                        Debug.Log("Clear Player Prefs");
                  }
            }
      }
}