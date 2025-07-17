using System;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      [Serializable]
      internal class ToolbarFPSSlider : BaseToolbarElement
      {
            [SerializeField] private int minFPS = 1;
            [SerializeField] private int maxFPS = 120;

            private int selectedFramerate;

            public override string NameInList => "[Slider] FPS";
            public override int SortingGroup => 1;

            public override void Init()
            {
                  if (selectedFramerate == 0)
                        selectedFramerate = 60;
            }

            public ToolbarFPSSlider(int minFPS = 1, int maxFPS = 120) : base(200)
            {
                  this.minFPS = minFPS;
                  this.maxFPS = maxFPS;
            }

            protected override void OnDrawInList(Rect position)
            {
                  position.width = 70.0f;
                  EditorGUI.LabelField(position, "Min FPS");

                  position.x += position.width + FieldSizeSpace;
                  position.width = 50.0f;
                  minFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", minFPS));

                  position.x += position.width + FieldSizeSpace;
                  position.width = 70.0f;
                  EditorGUI.LabelField(position, "Max FPS");

                  position.x += position.width + FieldSizeSpace;
                  position.width = 50.0f;
                  maxFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", maxFPS));
            }

            protected override void OnDrawInToolbar()
            {
                  EditorGUILayout.LabelField("FPS", GUILayout.Width(30));
                  selectedFramerate = EditorGUILayout.IntSlider("", selectedFramerate, minFPS, maxFPS, GUILayout.Width(widthInToolbar - 30.0f));

                  if (EditorApplication.isPlaying && selectedFramerate != Application.targetFrameRate)
                        Application.targetFrameRate = selectedFramerate;
            }
      }
}