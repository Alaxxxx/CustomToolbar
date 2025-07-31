using UnityEditor;

namespace OpalStudio.CustomToolbar.Editor.Utils
{
      internal static class SerializeAssetsUtils
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
      }
}