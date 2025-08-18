using OpalStudio.CustomToolbar.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
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

                  var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                  SerializedProperty layersProp = tagManager.FindProperty("layers");

                  for (int i = 0; i < 32; i++)
                  {
                        string layerName = "";

                        if (i <= 7)
                        {
                              layerName = LayerMask.LayerToName(i);
                        }
                        else
                        {
                              if (i < layersProp.arraySize)
                              {
                                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                                    layerName = layerProp.stringValue;
                              }
                        }

                        if (!string.IsNullOrEmpty(layerName))
                        {
                              bool isVisible = (currentMask & (1 << i)) != 0;
                              int layerIndex = i;

                              string menuItemName = $"{layerName} (Layer {i})";
                              menu.AddItem(new GUIContent(menuItemName), isVisible, () => ToggleLayerVisibility(layerIndex));
                        }
                  }

                  if (menu.GetItemCount() == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No layers found"));

                        return menu;
                  }

                  menu.AddSeparator("");

                  menu.AddItem(new GUIContent("Show All"), false, static () => Tools.visibleLayers = ~0);
                  menu.AddItem(new GUIContent("Hide All"), false, static () => Tools.visibleLayers = 0);
                  menu.AddItem(new GUIContent("Invert Selection"), false, static () => Tools.visibleLayers = ~Tools.visibleLayers);

                  menu.AddSeparator("");

                  bool hasIsolateItems = false;

                  for (int i = 0; i < 32; i++)
                  {
                        string layerName = "";

                        if (i <= 7)
                        {
                              layerName = LayerMask.LayerToName(i);
                        }
                        else
                        {
                              if (i < layersProp.arraySize)
                              {
                                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                                    layerName = layerProp.stringValue;
                              }
                        }

                        if (!string.IsNullOrEmpty(layerName))
                        {
                              int layerIndex = i;
                              string menuItemName = $"Isolate/{layerName} (Layer {i})";
                              menu.AddItem(new GUIContent(menuItemName), false, () => { Tools.visibleLayers = (1 << layerIndex); });
                              hasIsolateItems = true;
                        }
                  }

                  if (!hasIsolateItems)
                  {
                        menu.AddDisabledItem(new GUIContent("Isolate/No layers available"));
                  }

                  return menu;
            }

            private static void ToggleLayerVisibility(int layer)
            {
                  Tools.visibleLayers ^= (1 << layer);
            }
      }
}