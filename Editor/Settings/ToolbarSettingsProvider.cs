using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace CustomToolbar.Editor.Settings
{
      /// <summary>
      /// Unity Settings Provider for Custom Toolbar configuration.
      /// Integrates the toolbar settings into Unity's Project Settings window.
      /// </summary>
      public sealed class ToolbarSettingsProvider : SettingsProvider, IDisposable
      {
            // SerializedObject wrapper for the toolbar configuration asset.
            // Enables Unity's property field system to automatically handle GUI drawing,
            // undo/redo operations, and change tracking for the configuration data.
            private SerializedObject _serializedConfig;

            private ToolbarSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
            {
            }

            // Static factory method that Unity calls to create and register this settings provider.
            // The [SettingsProvider] attribute tells Unity to automatically discover and register this method.
            [SettingsProvider]
            public static SettingsProvider CreateToolbarSettingsProvider()
            {
                  // Create the provider with the menu path "Project/Custom Toolbar"
                  var provider = new ToolbarSettingsProvider("Project/Custom Toolbar");

                  provider.OnActivate(null, null);

                  return provider;
            }

            // Called when the settings provider becomes active/visible in the Project Settings window.
            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                  // Create a SerializedObject wrapper around the toolbar configuration singleton
                  _serializedConfig = new SerializedObject(ToolbarSettings.Instance);
            }

            // Called every frame to draw the settings provider's GUI using IMGUI.
            public override void OnGUI(string searchContext)
            {
                  if (_serializedConfig == null || _serializedConfig.targetObject == null)
                  {
                        EditorGUILayout.HelpBox("Settings asset could not be loaded.", MessageType.Warning);

                        return;
                  }

                  // Update the SerializedObject to reflect any external changes to the asset
                  _serializedConfig.Update();

                  // Draw the settings Header
                  EditorGUILayout.LabelField("Manage Groups", EditorStyles.boldLabel);
                  EditorGUILayout.Space();

                  // Draw the "groups" property using Unity's automatic property field system with ToolbarGroupDrawer
                  EditorGUILayout.PropertyField(_serializedConfig.FindProperty("groups"));

                  EditorGUILayout.Space(20);

                  // Draw the "toolboxShortcuts" property using Unity's automatic property field system with ToolboxShortcutDrawer
                  EditorGUILayout.LabelField("Manage Toolbox Shortcuts", EditorStyles.boldLabel);
                  EditorGUILayout.PropertyField(_serializedConfig.FindProperty("toolboxShortcuts"));

                  _serializedConfig.ApplyModifiedProperties();
            }

            public void Dispose()
            {
                  _serializedConfig?.Dispose();
            }
      }
}