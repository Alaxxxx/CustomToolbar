using System;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarFpsSlider : BaseToolbarElement
      {
            [SerializeField] private int minFPS;
            [SerializeField] private int maxFPS;

            private int currentFPS;

            public override string Name => "FPS Slider";
            public override string Tooltip => "Controls Application.targetFrameRate to simulate performance.";

            public override void OnInit()
            {
                  this.Width = 200;

                  if (currentFPS == 0)
                  {
                        currentFPS = 60;
                  }

                  Application.targetFrameRate = currentFPS;
            }

            public override void OnDrawInToolbar()
            {
                  EditorGUILayout.LabelField("FPS", GUILayout.Width(30));

                  EditorGUI.BeginChangeCheck();

                  currentFPS = EditorGUILayout.IntSlider(currentFPS, minFPS, maxFPS, GUILayout.Width(this.Width - 35));

                  if (EditorGUI.EndChangeCheck())
                  {
                        Application.targetFrameRate = currentFPS;
                  }
            }
      }
}