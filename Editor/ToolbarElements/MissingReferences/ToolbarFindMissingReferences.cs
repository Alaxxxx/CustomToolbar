using System.Collections.Generic;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences.Data;
using OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements.MissingReferences
{
      sealed internal class ToolbarFindMissingReferences : BaseToolbarElement
      {
            private GUIContent buttonContent;

            protected override string Name => "Find Missing References";
            protected override string Tooltip => "Scan the scene and opens a window with the missing references.";

            private readonly static HashSet<string> IgnoredComponentTypes = new()
            {
                        "UniversalAdditionalCameraData", "UniversalAdditionalLightData",
            };

            private readonly static HashSet<string> IgnoredFieldNames = new()
            {
                        "m_VolumeTrigger"
            };

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_Search Icon").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  this.Enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

                  using (new EditorGUI.DisabledScope(!this.Enabled))
                  {
                        if (GUILayout.Button(buttonContent, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                        {
                              ScanSceneForMissingReferences();
                        }
                  }
            }

            private static void ScanSceneForMissingReferences()
            {
                  Scene scene = SceneManager.GetActiveScene();
                  GameObject[] allGameObjects = scene.GetRootGameObjects();

                  var results = new Dictionary<GameObject, List<MissingReferenceInfo>>();

                  foreach (GameObject go in allGameObjects)
                  {
                        ScanGameObjectRecursive(go, results);
                  }

                  MissingReferencesWindow.ShowWindow(results);
            }

            private static void ScanGameObjectRecursive(GameObject go, Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();

                  foreach (MonoBehaviour component in components)
                  {
                        if (component == null)
                        {
                              AddResult(go, new MissingReferenceInfo { IsScriptMissing = true }, results);

                              continue;
                        }

                        if (IgnoredComponentTypes.Contains(component.GetType().Name))
                        {
                              continue;
                        }

                        var serializedObject = new SerializedObject(component);
                        SerializedProperty property = serializedObject.GetIterator();

                        while (property.NextVisible(true))
                        {
                              if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != null)
                              {
                                    continue;
                              }

                              if (IgnoredFieldNames.Contains(property.name))
                              {
                                    continue;
                              }

                              AddResult(go, new MissingReferenceInfo
                              {
                                          ComponentName = component.GetType().Name,
                                          FieldName = property.displayName,
                                          IsScriptMissing = false
                              }, results);
                        }
                  }

                  foreach (Transform child in go.transform)
                  {
                        ScanGameObjectRecursive(child.gameObject, results);
                  }
            }

            private static void AddResult(GameObject go, MissingReferenceInfo info, Dictionary<GameObject, List<MissingReferenceInfo>> results)
            {
                  if (!results.ContainsKey(go))
                  {
                        results[go] = new List<MissingReferenceInfo>();
                  }

                  results[go].Add(info);
            }
      }
}