using CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarLayerVisibility : BaseToolbarElement
      {
            private GUIContent _buttonContent;

            protected override string Name => "Layer Visibility";

            protected override string Tooltip => "Controls which layers are visible in the Scene View.";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_SceneLayersToggle").image;
                  _buttonContent = new GUIContent("Layers", icon, this.Tooltip);

                  this.Width = 80;
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(_buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        BuildLayerMenu().ShowAsContext();
                  }
            }

            private static GenericMenu BuildLayerMenu()
            {
                  var menu = new GenericMenu();
                  int currentMask = Tools.visibleLayers;

                  for (int i = 0; i < 32; i++)
                  {
                        string layerName = LayerMask.LayerToName(i);

                        if (!string.IsNullOrEmpty(layerName))
                        {
                              bool isVisible = (currentMask & (1 << i)) != 0;
                              int layerIndex = i;
                              menu.AddItem(new GUIContent(layerName), isVisible, () => ToggleLayerVisibility(layerIndex));
                        }
                  }

                  menu.AddSeparator("");

                  menu.AddItem(new GUIContent("Show All"), false, static () => Tools.visibleLayers = ~0);
                  menu.AddItem(new GUIContent("Hide All"), false, static () => Tools.visibleLayers = 0);
                  menu.AddItem(new GUIContent("Invert Selection"), false, static () => Tools.visibleLayers = ~Tools.visibleLayers);

                  menu.AddSeparator("");

                  for (int i = 0; i < 32; i++)
                  {
                        string layerName = LayerMask.LayerToName(i);

                        if (!string.IsNullOrEmpty(layerName))
                        {
                              int layerIndex = i;

                              menu.AddItem(new GUIContent($"Isolate/{layerName}"), false, () => { Tools.visibleLayers = (1 << layerIndex); });
                        }
                  }

                  return menu;
            }

            private static void ToggleLayerVisibility(int layer)
            {
                  Tools.visibleLayers ^= (1 << layer);
            }
      }
}