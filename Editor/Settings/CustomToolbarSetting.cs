using System.Collections.Generic;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      [FilePath("UserSettings/CustomToolbarSetting.asset", FilePathAttribute.Location.ProjectFolder)]
      sealed internal class CustomToolbarSetting : ScriptableSingleton<CustomToolbarSetting>
      {
            [SerializeReference] List<BaseToolbarElement> toolbarElements = new()
            {
                        new ToolbarEnterPlayMode(),
                        new ToolbarSceneSelection(),
                        new ToolbarSpace(),

                        new ToolbarSavingPrefs(),
                        new ToolbarClearPrefs(),
                        new ToolbarSpace(),

                        new ToolbarReloadScene(),
                        new ToolbarStartFromFirstScene(),
                        new ToolbarSpace(),

                        new ToolbarSides(),

                        new ToolbarTimeslider(),
                        new ToolbarFPSSlider(),
                        new ToolbarSpace(),

                        new ToolbarRecompile(),
                        new ToolbarReserializeSelected(),
                        new ToolbarReserializeAll(),
            };

            internal List<BaseToolbarElement> ToolbarElements => toolbarElements;

            internal static SerializedObject GetSerializedSetting()
            {
                  return new SerializedObject(instance);
            }

            internal void Save()
            {
                  Save(true);
            }
      }
}