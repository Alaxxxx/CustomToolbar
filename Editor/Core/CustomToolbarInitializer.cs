using System.Collections.Generic;
using System.Linq;
using CustomToolbar.Editor.ToolbarElements;
using CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Core
{
      [InitializeOnLoad]
      public static class CustomToolbarInitializer
      {
            static CustomToolbarInitializer()
            {
                  var sceneManagementGroup = new BaseToolbarElement[]
                  {
                              new ToolbarSceneSelection(),
                              new ToolbarStartFromFirstScene(),
                              new ToolbarReloadScene(),
                  };

                  var controlGroup = new BaseToolbarElement[]
                  {
                              new ToolbarEnterPlayMode(),
                              new ToolbarTimeSlider(),
                              new ToolbarFpsSlider(),
                  };

                  var assetDebugGroup = new BaseToolbarElement[]
                  {
                              new ToolbarRecompile(),
                              new ToolbarReserializeAll(),
                              new ToolbarReserializeSelected(),
                  };

                  ToolbarCallback.OnToolbarGUILeftOfCenter = () =>
                  {
                        GUILayout.BeginHorizontal(ToolbarStyles.ToolbarHorizontalGroupStyle);

                        GUILayout.FlexibleSpace();
                        DrawGroup(sceneManagementGroup);

                        GUILayout.Space(50);
                        GUILayout.EndHorizontal();
                  };

                  ToolbarCallback.OnToolbarGUIRightOfCenter = () =>
                  {
                        GUILayout.BeginHorizontal(ToolbarStyles.ToolbarHorizontalGroupStyle);

                        GUILayout.Space(50);

                        DrawGroup(controlGroup);
                        GUILayout.Space(50);
                        DrawGroup(assetDebugGroup);

                        GUILayout.EndHorizontal();
                  };

                  List<BaseToolbarElement> allElements = sceneManagementGroup.Concat(controlGroup).Concat(assetDebugGroup).ToList();

                  allElements.ForEach(static e => e.OnInit());

                  EditorApplication.playModeStateChanged += (state) =>
                  {
                        allElements.ForEach(e => e.OnPlayModeStateChanged(state));

                        if (state == PlayModeStateChange.EnteredEditMode)
                        {
                              SceneAssetsUtils.RestoreSceneAfterPlay();
                        }
                  };
                  Selection.selectionChanged += () => allElements.ForEach(static e => e.OnSelectionChanged());
            }

            private static void DrawGroup(IEnumerable<BaseToolbarElement> elements)
            {
                  foreach (BaseToolbarElement element in elements)
                  {
                        element.OnDrawInToolbar();
                  }
            }
      }
}