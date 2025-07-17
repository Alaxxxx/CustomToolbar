using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      internal static class CustomToolbarReorderableList
      {
            internal static ReorderableList Create(List<BaseToolbarElement> configsList, GenericMenu.MenuFunction2 menuItemHandler)
            {
                  var reorderableList = new ReorderableList(configsList, typeof(BaseToolbarElement), true, false, true, true);

                  reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
                  reorderableList.drawElementCallback = (position, index, isActive, isFocused) => { configsList[index].DrawInList(position); };

                  reorderableList.onAddDropdownCallback = (buttonRect, list) =>
                  {
                        BaseToolbarElement[] elements = GetNewObjectsOfType<BaseToolbarElement>().ToArray();

                        var menu = new GenericMenu();

                        int lastSortingGroup = 0;

                        foreach (var element in elements)
                        {
                              if (lastSortingGroup != element.SortingGroup)
                              {
                                    menu.AddSeparator("");
                                    lastSortingGroup = element.SortingGroup;
                              }

                              menu.AddItem(new GUIContent(element.NameInList), false, menuItemHandler, element);
                        }

                        menu.ShowAsContext();
                  };

                  return reorderableList;
            }

            private static List<T> GetNewObjectsOfType<T>() where T : class, IComparable<T>
            {
                  toolbarOptions ??= FindConstructors<T>();

                  var objects = new List<T>();

                  foreach (ConstructorOption item in toolbarOptions)
                  {
                        objects.Add((T)item.Constructor.Invoke(item.Parameters));
                  }

                  objects.Sort();

                  return objects;
            }

            private static ConstructorOption[] FindConstructors<T>() where T : class, IComparable<T>
            {
                  var typeConstructorList = new List<ConstructorOption>();

                  foreach (Type type in Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                  {
                        ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                        object[] constructorParameters = null;

                        if (constructor == null)
                        {
                              ConstructorInfo[] constructors = type.GetConstructors();

                              foreach (var item in constructors)
                              {
                                    ParameterInfo[] parameters = item.GetParameters();
                                    object[] tempParameters = new object[parameters.Length];

                                    int validParameters = 0;

                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                          ParameterInfo parameter = parameters[i];
                                          object defaultValue = parameter.RawDefaultValue;

                                          if (defaultValue != DBNull.Value)
                                          {
                                                validParameters++;
                                                tempParameters[i] = defaultValue;
                                          }
                                    }

                                    if (validParameters == parameters.Length)
                                    {
                                          constructor = item;
                                          constructorParameters = tempParameters;
                                    }
                              }
                        }

                        if (constructor != null)
                        {
                              typeConstructorList.Add(new ConstructorOption(constructor, constructorParameters));
                        }
                        else
                        {
                              Debug.LogWarning("Custom Toolbar was unable to find a suitable constructor for the class " + type +
                                               ". To show up in the Project Settings it must either contain a default constructor (no parameters), or a constructor where all parameters have a default value.");
                        }
                  }

                  return typeConstructorList.ToArray();
            }

            private static ConstructorOption[] toolbarOptions;

            private struct ConstructorOption
            {
                  public readonly ConstructorInfo Constructor;
                  public readonly object[] Parameters;

                  public ConstructorOption(ConstructorInfo constructor, object[] parameters)
                  {
                        this.Constructor = constructor;
                        this.Parameters = parameters;
                  }
            }
      }
}