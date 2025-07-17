using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.Settings;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;

namespace CustomToolbar.Editor.Core
{
      [InitializeOnLoad]
      public static class CustomToolbarInitializer
      {
            static CustomToolbarInitializer()
            {
                  CustomToolbarSetting setting = ScriptableSingleton<CustomToolbarSetting>.instance;

                  setting.ToolbarElements.ForEach(static element => element.Init());

                  List<BaseToolbarElement> leftTools = setting.ToolbarElements.TakeWhile(static element => element is not ToolbarSides).ToList();
                  List<BaseToolbarElement> rightTools = setting.ToolbarElements.Except(leftTools).ToList();
                  IEnumerable<Action> leftToolsDrawActions = leftTools.Select(TakeDrawAction);
                  IEnumerable<Action> rightToolsDrawActions = rightTools.Select(TakeDrawAction);

                  ToolbarExtender.LeftToolbarGUI.AddRange(leftToolsDrawActions);
                  ToolbarExtender.RightToolbarGUI.AddRange(rightToolsDrawActions);
            }

            private static Action TakeDrawAction(BaseToolbarElement element)
            {
                  Action action = element.DrawInToolbar;

                  return action;
            }
      }
}