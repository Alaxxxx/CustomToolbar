using System;
using UnityEditor;

namespace CustomToolbar.Editor.Utils
{
      public static class ForceReserializeAssetsUtils
      {
            public static void ForceReserializeAllAssets()
            {
                  if (!EditorUtility.DisplayDialog("Attention",
                                  "Do you want to force reserialize all assets? This can be time heavy operation and result in massive list of changes.", "Ok", "Cancel"))
                  {
                        return;
                  }

                  AssetDatabase.ForceReserializeAssets();
            }

            public static void ForceReserializeSelectedAssets()
            {
                  string[] assetGUIDs = Selection.assetGUIDs;

                  if (assetGUIDs.Length == 0)
                  {
                        EditorUtility.DisplayDialog("Attention", "No assets are selected.", "Ok");

                        return;
                  }

                  string[] assetPaths = Array.ConvertAll(assetGUIDs, AssetDatabase.GUIDToAssetPath);
                  AssetDatabase.ForceReserializeAssets(assetPaths);
            }
      }
}