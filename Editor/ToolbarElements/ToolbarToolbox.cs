using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.Core.Data;
using OpalStudio.CustomToolbar.Editor.Settings;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarToolbox : BaseToolbarElement
      {
            private GUIContent buttonContent;
            protected override string Name => "Toolbox";
            protected override string Tooltip => "Custom toolbox shortcuts";

            public override void OnInit()
            {
                  Texture icon = EditorGUIUtility.IconContent("d_Toolbar Plus More").image;
                  buttonContent = new GUIContent(icon, this.Tooltip);
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive, ToolbarStyles.CommandButtonStyle, GUILayout.Width(this.Width)))
                  {
                        GenerateMenu().ShowAsContext();
                  }
            }

            private static GenericMenu GenerateMenu()
            {
                  var menu = new GenericMenu();
                  ToolbarConfiguration config = ToolbarSettings.Instance;

                  menu.AddItem(new GUIContent("Toolbar Settings"), false, OpenToolbarSettings);
                  menu.AddSeparator("");

                  if (config.toolboxShortcuts.Count > 0)
                  {
                        foreach (ToolboxShortcut shortcut in config.toolboxShortcuts)
                        {
                              if (shortcut.isEnabled && !string.IsNullOrEmpty(shortcut.menuItemPath))
                              {
                                    string menuPath = !string.IsNullOrEmpty(shortcut.subMenuPath)
                                                ? $"{shortcut.subMenuPath.Trim('/')}/{shortcut.displayName}"
                                                : shortcut.displayName;

                                    string path = shortcut.menuItemPath;
                                    menu.AddItem(new GUIContent(menuPath), false, () => ExecuteShortcut(path));
                              }
                        }
                  }
                  else
                  {
                        menu.AddDisabledItem(new GUIContent("No shortcuts configured..."));
                  }

                  return menu;
            }

            private static void ExecuteShortcut(string path)
            {
                  int prefixIndex = path.IndexOf(':');

                  if (prefixIndex > 0)
                  {
                        string prefix = path.Substring(0, prefixIndex).ToLowerInvariant();
                        string value = path.Substring(prefixIndex + 1);

                        switch (prefix)
                        {
                              case "settings":
                                    SettingsService.OpenProjectSettings(value);

                                    return;
                              case "select":
                                    GameObject obj = GameObject.Find(value);

                                    if (obj != null)
                                    {
                                          Selection.activeGameObject = obj;
                                          EditorGUIUtility.PingObject(obj);
                                    }
                                    else
                                    {
                                          Debug.LogWarning($"[CustomToolbar] GameObject not found in scene: {value}");
                                    }

                                    return;

                              case "asset":
                                    var asset = AssetDatabase.LoadAssetAtPath<Object>(value);

                                    if (asset != null)
                                    {
                                          AssetDatabase.OpenAsset(asset);
                                    }
                                    else
                                    {
                                          Debug.LogWarning($"[CustomToolbar] Asset not found at path: {value}");
                                    }

                                    return;

                              case "url":
                                    Application.OpenURL(value);

                                    return;

                              case "folder":
                                    var folderObject = AssetDatabase.LoadAssetAtPath<Object>(value);

                                    if (folderObject != null)
                                    {
                                          EditorGUIUtility.PingObject(folderObject);
                                    }
                                    else
                                    {
                                          Debug.LogWarning($"[CustomToolbar] Folder not found at path: {value}");
                                    }

                                    return;

                              case "method":
                                    ExecuteStaticMethod(value);

                                    return;
                        }
                  }

                  if (!EditorApplication.ExecuteMenuItem(path))
                  {
                        Debug.LogError($"[CustomToolbar] Failed to execute shortcut. Check if the menu item path is correct: '{path}'");
                  }
            }

            private static void ExecuteStaticMethod(string methodData)
            {
                  string[] parts = methodData.Split(new[] { '|' }, 2);
                  string methodPath = parts[0];
                  string paramsString = parts.Length > 1 ? parts[1] : null;

                  int lastDot = methodPath.LastIndexOf('.');

                  if (lastDot <= 0)
                  {
                        throw new ArgumentException("Invalid method path format.");
                  }

                  string className = methodPath.Substring(0, lastDot);
                  string methodName = methodPath.Substring(lastDot + 1);

                  Type type = FindTypeInAllAssemblies(className);

                  if (type == null)
                  {
                        throw new TypeLoadException($"Could not find class: {className}");
                  }

                  string[] stringArgs = string.IsNullOrEmpty(paramsString) ? Array.Empty<string>() : paramsString.Split(',');

                  MethodInfo methodInfo = FindMethodBySignature(type, methodName, stringArgs.Length);

                  if (methodInfo == null)
                  {
                        throw new MissingMethodException($"Method '{methodName}' with {stringArgs.Length} parameters not found in class '{className}'.");
                  }

                  object[] parameters = ConvertArguments(methodInfo.GetParameters(), stringArgs);
                  methodInfo.Invoke(null, parameters);
            }

            private static MethodInfo FindMethodBySignature(Type type, string methodName, int paramCount)
            {
                  return Array.Find(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                              m => m.Name == methodName && m.GetParameters().Length == paramCount);
            }

            private static object[] ConvertArguments(ParameterInfo[] paramInfos, string[] stringArgs)
            {
                  if (paramInfos.Length == 0)
                  {
                        return null;
                  }

                  object[] convertedArgs = new object[paramInfos.Length];

                  for (int i = 0; i < paramInfos.Length; i++)
                  {
                        Type paramType = paramInfos[i].ParameterType;
                        string stringValue = stringArgs[i].Trim();
                        convertedArgs[i] = Convert.ChangeType(stringValue, paramType, CultureInfo.InvariantCulture);
                  }

                  return convertedArgs;
            }

            private static Type FindTypeInAllAssemblies(string typeName)
            {
                  return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(typeName)).FirstOrDefault(static type => type != null);
            }

            private static void OpenToolbarSettings()
            {
                  SettingsService.OpenProjectSettings("Project/Custom Toolbar");
            }
      }
}