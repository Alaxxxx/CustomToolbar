using System.IO;
using CustomToolbar.Editor.Settings.Data;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      /// <summary>
      /// Manages the loading, creation, and caching of toolbar configuration settings.
      /// Provides a singleton-like access pattern for the toolbar configuration ScriptableObject.
      /// </summary>
      internal static class ToolbarSettings
      {
            // The asset path where the toolbar configuration will be stored.
            private const string ConfigAssetPath = "Assets/Settings/CustomToolbar/CustomToolbarSettings.asset";

            // Cached reference to the loaded configuration instance.
            private static ToolbarConfiguration instance;

            // Public property providing singleton-like access to the toolbar configuration.
            public static ToolbarConfiguration Instance
            {
                  get
                  {
                        instance = instance ? instance : LoadOrCreateConfiguration();

                        return instance;
                  }
            }

            // Loads an existing configuration from disk or creates a new one if none exists.
            private static ToolbarConfiguration LoadOrCreateConfiguration()
            {
                  // Attempt to load the existing configuration from the predefined asset path
                  var config = AssetDatabase.LoadAssetAtPath<ToolbarConfiguration>(ConfigAssetPath);

                  // If configuration exists, return it
                  if (config != null)
                  {
                        return config;
                  }

                  // Create a new configuration instance using Unity's ScriptableObject system
                  var newConfig = ScriptableObject.CreateInstance<ToolbarConfiguration>();

                  // Set the package root path for relative path calculations
                  newConfig.packageRootPath = FindRootPath();

                  // Populate the new configuration with default toolbar elements
                  PopulateWithDefaultData(newConfig);

                  // Ensure the directory structure exists before creating the asset
                  Directory.CreateDirectory(Path.GetDirectoryName(ConfigAssetPath)!);

                  // Create the ScriptableObject asset at the specified path
                  AssetDatabase.CreateAsset(newConfig, ConfigAssetPath);

                  // Save the asset and refresh the AssetDatabase to make it visible in the editor
                  AssetDatabase.SaveAssets();
                  AssetDatabase.Refresh();

                  return newConfig;
            }

            // Attempts to automatically determine the root path of the CustomToolbar package.
            private static string FindRootPath()
            {
                  // Search for the ToolbarSettingsProvider script using Unity's GUID system
                  string[] guids = AssetDatabase.FindAssets("t:script ToolbarSettingsProvider");

                  // If we found at least one matching script
                  if (guids.Length > 0)
                  {
                        // Convert the first GUID to an actual file path
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                        // Navigate up the directory hierarchy to find the package root
                        return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
                  }

                  // Fallback path if automatic detection fails
                  return "Assets/CustomToolbar";
            }

            // Used to populate the configuration with default data
            private static void PopulateWithDefaultData(ToolbarConfiguration config)
            {
                  // Groupe 0 : Version Control
                  var versionControlGroup = new ToolbarGroup { groupName = "Version Control", side = ToolbarSide.Left };
                  versionControlGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarGitStatus).AssemblyQualifiedName });
                  config.groups.Add(versionControlGroup);

                  // Groupe 1 : Utilities
                  var utilsGroup = new ToolbarGroup { groupName = "Utilities", side = ToolbarSide.Left };
                  utilsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarScreenshot).AssemblyQualifiedName });
                  utilsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarClearPlayerPrefs).AssemblyQualifiedName });
                  utilsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarSaveProject).AssemblyQualifiedName });
                  config.groups.Add(utilsGroup);

                  // Groupe 2 : Scene Management
                  var sceneGroup = new ToolbarGroup { groupName = "Scenes", side = ToolbarSide.Left };
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarSceneSelection).AssemblyQualifiedName });
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarStartFromFirstScene).AssemblyQualifiedName });
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarReloadScene).AssemblyQualifiedName });
                  config.groups.Add(sceneGroup);

                  // Groupe 3 : Controls bar
                  var controlsGroup = new ToolbarGroup { groupName = "Controls", side = ToolbarSide.Right };
                  controlsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarFpsSlider).AssemblyQualifiedName });
                  controlsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarTimeSlider).AssemblyQualifiedName });
                  config.groups.Add(controlsGroup);

                  // Groupe 4 : Debugging
                  var debugGroup = new ToolbarGroup { groupName = "Debugging", side = ToolbarSide.Right };
                  debugGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarEnterPlayMode).AssemblyQualifiedName });
                  debugGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarRecompile).AssemblyQualifiedName });
                  debugGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarReserializeAll).AssemblyQualifiedName });
                  config.groups.Add(debugGroup);
            }
      }
}