using System;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarTimeSlider : BaseToolbarElement
      {
            [SerializeField] private float minTimeScale;
            [SerializeField] private float maxTimeScale = 5f;

            private float currentTimeScale;

            public override string Name => "Timescale Slider";
            public override string Tooltip => "Controls Time.timeScale to slow down or speed up the game.";

            public override void OnInit()
            {
                  this.Width = 200;

                  currentTimeScale = 1.0f;
                  Time.timeScale = currentTimeScale;
            }

            public override void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                  if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                  {
                        currentTimeScale = 1.0f;
                        Time.timeScale = currentTimeScale;
                  }

                  this.Enabled = (state == PlayModeStateChange.EnteredPlayMode);
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        EditorGUILayout.LabelField("Time", GUILayout.Width(35));

                        EditorGUI.BeginChangeCheck();

                        currentTimeScale = EditorGUILayout.Slider(currentTimeScale, minTimeScale, maxTimeScale, GUILayout.Width(this.Width - 40));

                        if (EditorGUI.EndChangeCheck())
                        {
                              Time.timeScale = currentTimeScale;
                        }
                  }
            }
      }
}