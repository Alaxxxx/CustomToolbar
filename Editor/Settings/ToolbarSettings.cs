using System.IO;
using OpalStudio.CustomToolbar.Editor.Core.Data;
using OpalStudio.CustomToolbar.Editor.ToolbarElements;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.Favorites;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.SceneBookmarks;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.Settings
{
      /// <summary>
      /// Manages the loading, creation, and caching of toolbar configuration settings.
      /// Provides a singleton-like access pattern for the toolbar configuration ScriptableObject.
      /// </summary>
      internal static class ToolbarSettings
      {
            private const string ConfigAssetPath = "Assets/Settings/CustomToolbar/CustomToolbarSettings.asset";

            private static ToolbarConfiguration instance;

            public static ToolbarConfiguration Instance
            {
                  get
                  {
                        instance = instance ? instance : LoadOrCreateConfiguration();

                        return instance;
                  }
            }

            private static ToolbarConfiguration LoadOrCreateConfiguration()
            {
                  // Attempt to load the existing configuration from the predefined asset path
                  var config = AssetDatabase.LoadAssetAtPath<ToolbarConfiguration>(ConfigAssetPath);

                  if (config != null)
                  {
                        return config;
                  }

                  var newConfig = ScriptableObject.CreateInstance<ToolbarConfiguration>();

                  PopulateWithDefaultData(newConfig);

                  Directory.CreateDirectory(Path.GetDirectoryName(ConfigAssetPath)!);

                  AssetDatabase.CreateAsset(newConfig, ConfigAssetPath);

                  AssetDatabase.SaveAssets();
                  AssetDatabase.Refresh();

                  return newConfig;
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
                  utilsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarFindMissingReferences).AssemblyQualifiedName });
                  utilsGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarSceneBookmarks).AssemblyQualifiedName });
                  config.groups.Add(utilsGroup);

                  // Groupe 2 : Scene Management
                  var sceneGroup = new ToolbarGroup { groupName = "Scenes", side = ToolbarSide.Left };
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarSceneSelection).AssemblyQualifiedName });
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarStartFromFirstScene).AssemblyQualifiedName });
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarReloadScene).AssemblyQualifiedName });
                  sceneGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarLayerVisibility).AssemblyQualifiedName });
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

                  // Groupe 5: Toolbox
                  var toolboxGroup = new ToolbarGroup { groupName = "Toolbox", side = ToolbarSide.Right };
                  toolboxGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarToolbox).AssemblyQualifiedName });
                  toolboxGroup.elements.Add(new ToolbarElement { name = typeof(ToolbarFavorites).AssemblyQualifiedName });
                  config.groups.Add(toolboxGroup);
            }
      }
}