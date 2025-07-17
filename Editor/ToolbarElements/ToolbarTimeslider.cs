using System;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarTimeslider : BaseToolbarElement
      {
            [SerializeField] private float minTime = 1;
            [SerializeField] private float maxTime = 120;

            private bool defaultSetOnce;
            private float selectedTimeScale;

            public override string NameInList => "[Slider] Timescale";
            public override int SortingGroup => 1;

            public override void Init()
            {
                  if (!defaultSetOnce)
                  {
                        defaultSetOnce = true;
                        selectedTimeScale = 1;
                  }
            }

            public ToolbarTimeslider(float minTime = 0f, float maxTime = 10.0f) : base(200)
            {
                  this.minTime = minTime;
                  this.maxTime = maxTime;
            }

            protected override void OnDrawInList(Rect position)
            {
                  position.width = 70.0f;
                  EditorGUI.LabelField(position, "Min Time");

                  position.x += position.width + FieldSizeSpace;
                  position.width = 50.0f;
                  minTime = EditorGUI.FloatField(position, "", minTime);

                  position.x += position.width + FieldSizeSpace;
                  position.width = 70.0f;
                  EditorGUI.LabelField(position, "Max Time");

                  position.x += position.width + FieldSizeSpace;
                  position.width = 50.0f;
                  maxTime = EditorGUI.FloatField(position, "", maxTime);
            }

            protected override void OnDrawInToolbar()
            {
                  EditorGUILayout.LabelField("Time", GUILayout.Width(30));
                  selectedTimeScale = EditorGUILayout.Slider("", selectedTimeScale, minTime, maxTime, GUILayout.Width(widthInToolbar - 30.0f));

                  if (EditorApplication.isPlaying && selectedTimeScale != Time.timeScale)
                        Time.timeScale = selectedTimeScale;
            }
      }
}