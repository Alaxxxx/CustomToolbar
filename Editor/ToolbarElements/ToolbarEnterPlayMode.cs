using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarEnterPlayMode : BaseToolbarElement
      {
            private static string[] playModeOptionsDisplay;
            private int selectedOptionIndex;

            [NonSerialized] private GUIContent buttonContent;

            public override string Name => "Play Mode Options";
            public override string Tooltip => "Configure 'Enter Play Mode' settings for faster iteration (Domain/Scene Reload).";

            public override void OnInit()
            {
                  this.Width = 120;

                  if (playModeOptionsDisplay == null)
                  {
                        List<string> enumOptions = Enum.GetNames(typeof(EnterPlayModeOptions)).ToList();
                        enumOptions.Insert(0, "Default");
                        playModeOptionsDisplay = enumOptions.ToArray();
                  }

                  selectedOptionIndex = EditorSettings.enterPlayModeOptionsEnabled ? (int)EditorSettings.enterPlayModeOptions + 1 : 0;
                  buttonContent = new GUIContent("", this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  buttonContent.text = playModeOptionsDisplay[selectedOptionIndex];

                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        var menu = new GenericMenu();

                        for (int i = 0; i < playModeOptionsDisplay.Length; i++)
                        {
                              int index = i;

                              menu.AddItem(new GUIContent(playModeOptionsDisplay[index]), selectedOptionIndex == index, () =>
                              {
                                    selectedOptionIndex = index;

                                    bool isEnabled = selectedOptionIndex != 0;
                                    EditorSettings.enterPlayModeOptionsEnabled = isEnabled;

                                    if (isEnabled)
                                    {
                                          EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)(selectedOptionIndex - 1);
                                    }
                              });
                        }

                        menu.ShowAsContext();
                  }
            }
      }
}