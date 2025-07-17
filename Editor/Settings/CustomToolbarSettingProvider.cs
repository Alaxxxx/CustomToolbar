using CustomToolbar.Editor.Core;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomToolbar.Editor.Settings
{
      sealed internal class CustomToolbarSettingProvider : SettingsProvider
      {
            private SerializedObject mToolbarSetting;
            private CustomToolbarSetting setting;

            private Vector2 scrollPos;
            private ReorderableList elementsList;

            private class Styles
            {
                  public readonly static GUIContent MinFPS = new GUIContent("Minimum FPS");
                  public readonly static GUIContent MaxFPS = new GUIContent("Maximum FPS");
                  public readonly static GUIContent LimitFPS = new GUIContent("Limit FPS");
            }

            private const string SettingPath = "Assets/Editor/Setting/CustomToolbarSetting.asset";

            public CustomToolbarSettingProvider(string path, SettingsScope scopes = SettingsScope.User) : base(path, scopes)
            {
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                  mToolbarSetting = CustomToolbarSetting.GetSerializedSetting();
                  setting = (mToolbarSetting.targetObject as CustomToolbarSetting);
            }

            public static bool IsSettingAvailable()
            {
                  return ScriptableSingleton<CustomToolbarSetting>.instance != null;
            }

            public override void OnGUI(string searchContext)
            {
                  base.OnGUI(searchContext);

                  scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                  elementsList ??= CustomToolbarReorderableList.Create(setting.ToolbarElements, OnMenuItemAdd);
                  elementsList.DoLayoutList();

                  EditorGUILayout.EndScrollView();

                  mToolbarSetting.ApplyModifiedProperties();

                  if (GUI.changed)
                  {
                        EditorUtility.SetDirty(mToolbarSetting.targetObject);
                        ToolbarExtender.OnGUI();
                        setting.Save();
                  }
            }

            private void OnMenuItemAdd(object target)
            {
                  setting.ToolbarElements.Add(target as BaseToolbarElement);
                  mToolbarSetting.ApplyModifiedProperties();
                  setting.Save();
            }

            [SettingsProvider]
            public static SettingsProvider CreateCustomToolbarSettingProvider()
            {
                  if (IsSettingAvailable())
                  {
                        var provider = new CustomToolbarSettingProvider("Project/Custom Toolbar", SettingsScope.Project)
                        {
                                    keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
                        };

                        return provider;
                  }

                  return null;
            }
      }
}