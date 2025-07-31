using System;
using System.Collections.Generic;
using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarEnterPlayMode : BaseToolbarElement
      {
            private List<(string name, EnterPlayModeOptions? value)> availableOptions;
            private int selectedOptionIndex;

            private GUIContent buttonContent;

            protected override string Name => "Play Mode Options";
            protected override string Tooltip => "Configure 'Enter Play Mode' settings for faster iteration (Domain/Scene Reload).";

            public override void OnInit()
            {
                  this.Width = 150;
                  buttonContent = new GUIContent("", this.Tooltip);

                  if (availableOptions == null)
                  {
                        availableOptions = new List<(string name, EnterPlayModeOptions? value)>
                        {
                                    ("Default", null)
                        };

                        foreach (EnterPlayModeOptions option in Enum.GetValues(typeof(EnterPlayModeOptions)))
                        {
                              if (option == EnterPlayModeOptions.None || option.ToString() == "DisableSceneBackupUnlessDirty")
                              {
                                    continue;
                              }

                              availableOptions.Add((option.ToString(), option));
                        }
                  }

                  selectedOptionIndex = EditorSettings.enterPlayModeOptionsEnabled
                              ? availableOptions.FindIndex(static x => x.value == EditorSettings.enterPlayModeOptions)
                              : 0;
            }

            public override void OnDrawInToolbar()
            {
                  if (selectedOptionIndex < 0 || selectedOptionIndex >= availableOptions.Count)
                  {
                        selectedOptionIndex = 0;
                  }

                  buttonContent.text = availableOptions[selectedOptionIndex].name;

                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        var menu = new GenericMenu();

                        for (int i = 0; i < availableOptions.Count; i++)
                        {
                              int index = i;
                              (string name, EnterPlayModeOptions? value) option = availableOptions[index];

                              menu.AddItem(new GUIContent(option.name), selectedOptionIndex == index, () =>
                              {
                                    selectedOptionIndex = index;

                                    if (option.value == null)
                                    {
                                          EditorSettings.enterPlayModeOptionsEnabled = false;
                                    }
                                    else
                                    {
                                          EditorSettings.enterPlayModeOptionsEnabled = true;
                                          EditorSettings.enterPlayModeOptions = option.value.Value;
                                    }
                              });
                        }

                        menu.ShowAsContext();
                  }
            }
      }
}