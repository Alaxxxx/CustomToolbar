using System;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarSavingPrefs : BaseToolbarElement
      {
            private static GUIContent savePassiveBtn;
            private static GUIContent saveActiveBtn;
            private static bool deleteKeys;

            public override string NameInList => "[Button]Disable saving prefs";
            public override int SortingGroup => 4;

            public override void Init()
            {
                  deleteKeys = false;

                  savePassiveBtn = EditorGUIUtility.IconContent("SavePassive");
                  savePassiveBtn.tooltip = "Enable saving player prefs (currently NOT saving)";

                  saveActiveBtn = EditorGUIUtility.IconContent("SaveActive");
                  saveActiveBtn.tooltip = "Disable saving player prefs (currently saving)";
            }

            protected override void OnDrawInList(Rect position)
            {
            }

            protected override void OnDrawInToolbar()
            {
                  if (deleteKeys)
                  {
                        if (GUILayout.Button(savePassiveBtn, ToolbarStyles.commandButtonStyle))
                        {
                              deleteKeys = false;
                              Debug.Log("Enable saving player prefs");
                        }
                  }
                  else
                  {
                        if (GUILayout.Button(saveActiveBtn, ToolbarStyles.commandButtonStyle))
                        {
                              deleteKeys = true;
                              Debug.Log("Disable saving player prefs");
                        }
                  }

                  if (deleteKeys)
                  {
                        PlayerPrefs.DeleteAll();
                  }
            }
      }
}