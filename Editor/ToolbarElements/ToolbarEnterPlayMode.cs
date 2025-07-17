using System;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarEnterPlayMode : BaseToolbarElement
      {
            int selectedEnterPlayMode;
            string[] enterPlayModeOption;

            public override string NameInList => "[Dropdown] Enter play mode option";
            public override int SortingGroup => 2;

            public override void Init()
            {
                  enterPlayModeOption = new[]
                  {
                              "Disabled",
                              "Reload All",
                              "Reload Scene",
                              "Reload Domain",
                              "FastMode",
                  };
            }

            protected override void OnDrawInList(Rect position)
            {
            }

            protected override void OnDrawInToolbar()
            {
                  if (EditorSettings.enterPlayModeOptionsEnabled)
                  {
                        EnterPlayModeOptions option = EditorSettings.enterPlayModeOptions;
                        selectedEnterPlayMode = (int)option + 1;
                  }

                  selectedEnterPlayMode = EditorGUILayout.Popup(selectedEnterPlayMode, enterPlayModeOption, GUILayout.Width(widthInToolbar));

                  if (GUI.changed && 0 <= selectedEnterPlayMode && selectedEnterPlayMode < enterPlayModeOption.Length)
                  {
                        EditorSettings.enterPlayModeOptionsEnabled = selectedEnterPlayMode != 0;
                        EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)(selectedEnterPlayMode - 1);
                  }
            }
      }
}