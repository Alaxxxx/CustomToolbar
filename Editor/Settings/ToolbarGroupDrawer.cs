using System;
using System.Linq;
using CustomToolbar.Editor.Settings.Data;
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      /// <summary>
      /// Custom property drawer for ToolbarGroup objects in the Unity Inspector.
      /// </summary>
      [CustomPropertyDrawer(typeof(ToolbarGroup))]
      public class ToolbarGroupDrawer : PropertyDrawer
      {
            // Cache of ReorderableList instances keyed by a property path.
            // This prevents recreating lists every frame and maintains state across redrawing.
            private readonly System.Collections.Generic.Dictionary<string, ReorderableList> _lists = new();

            // Calculates the total height needed to draw this property in the inspector.
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                  // Try to get the cached ReorderableList for this property
                  if (!_lists.TryGetValue(property.propertyPath, out ReorderableList list))
                  {
                        // If no list exists yet, return a single line height as fallback
                        return EditorGUIUtility.singleLineHeight;
                  }

                  // Return the calculated height of the reorderable list
                  return list.GetHeight();
            }

            // Draws the custom GUI for the ToolbarGroup property.
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                  // Try to get the cached ReorderableList for this property
                  if (!_lists.TryGetValue(property.propertyPath, out ReorderableList list))
                  {
                        // If no list exists yet, create a new one and cache it
                        list = CreateList(property);
                        _lists[property.propertyPath] = list;
                  }

                  // Draw the reorderable list
                  list.DoList(position);
            }

            // Creates a new ReorderableList for managing toolbar elements within a group.
            private static ReorderableList CreateList(SerializedProperty property)
            {
                  // Get reference to the "elements" array property within the ToolbarGroup
                  SerializedProperty elementsProperty = property.FindPropertyRelative("elements");

                  // Create the ReorderableList
                  var list = new ReorderableList(property.serializedObject, elementsProperty, true, true, true, true)
                  {
                              // Callback to draw the header of the reorderable list
                              drawHeaderCallback = (rect) =>
                              {
                                    // Get references to the group's properties
                                    SerializedProperty nameProp = property.FindPropertyRelative("groupName");
                                    SerializedProperty sideProp = property.FindPropertyRelative("side");
                                    SerializedProperty enabledProp = property.FindPropertyRelative("isEnabled");

                                    // Calculate layout rectangles for each control in the header
                                    // 20 px wide starting 10 px from the left
                                    var toggleRect = new Rect(rect.x + 10, rect.y, 20, EditorGUIUtility.singleLineHeight);

                                    // 60% of the remaining width
                                    var nameRect = new Rect(toggleRect.xMax, rect.y, rect.width * 0.6f - 20, EditorGUIUtility.singleLineHeight);

                                    // 40% of the remaining width, starting 5 px after the name field
                                    var sideRect = new Rect(nameRect.xMax + 5, rect.y, rect.width * 0.4f - 15, EditorGUIUtility.singleLineHeight);

                                    // Begin tracking changes to apply them if any occur
                                    EditorGUI.BeginChangeCheck();

                                    // Draw the enabled toggle checkbox, name text field, and side dropdown
                                    enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);
                                    nameProp.stringValue = EditorGUI.TextField(nameRect, nameProp.stringValue);
                                    sideProp.enumValueIndex = (int)(ToolbarSide)EditorGUI.EnumPopup(sideRect, (ToolbarSide)sideProp.enumValueIndex);

                                    // If any changes were made, apply them to the SerializedObject
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                          property.serializedObject.ApplyModifiedProperties();
                                    }
                              },

                              // Callback to draw each element in the list
                              drawElementCallback = (rect, index, _, _) =>
                              {
                                    // Get the toolbar element at the specified index
                                    SerializedProperty elementProp = elementsProperty.GetArrayElementAtIndex(index);
                                    SerializedProperty typeNameProp = elementProp.FindPropertyRelative("name");
                                    SerializedProperty enabledProp = elementProp.FindPropertyRelative("isEnabled");

                                    // Convert the type name to a user-friendly display name
                                    string displayName = GetDisplayName(typeNameProp.stringValue);

                                    // Calculate layout rectangles for toggle and label
                                    var toggleRect = new Rect(rect.x, rect.y + 2, 20, EditorGUIUtility.singleLineHeight);
                                    var labelRect = new Rect(toggleRect.xMax, rect.y + 2, rect.width - 20, EditorGUIUtility.singleLineHeight);

                                    // Begin tracking changes
                                    EditorGUI.BeginChangeCheck();

                                    // Draw enabled toggle and element name label
                                    enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);
                                    EditorGUI.LabelField(labelRect, displayName);

                                    // Apply changes if any occurred
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                          property.serializedObject.ApplyModifiedProperties();
                                    }
                              },

                              // Callback for the button add dropdown menu
                              onAddDropdownCallback = (_, _) =>
                              {
                                    // Create a context menu for selecting toolbar element types
                                    var menu = new GenericMenu();

                                    // Get all types that derive from BaseToolbarElement using Unity's TypeCache
                                    TypeCache.TypeCollection elementTypes = TypeCache.GetTypesDerivedFrom<BaseToolbarElement>();

                                    // Process each non-abstract toolbar element type
                                    foreach (Type type in elementTypes.Where(static t => !t.IsAbstract))
                                    {
                                          // Skip ToolbarSpace as it's handled automatically by the system
                                          if (type == typeof(ToolbarSpace))
                                          {
                                                continue;
                                          }

                                          // Add menu item with cleaned-up type name (remove "Toolbar" prefix)
                                          menu.AddItem(new GUIContent(type.Name.Replace("Toolbar", "", StringComparison.Ordinal)), false, () =>
                                          {
                                                int newIndex = elementsProperty.arraySize;
                                                elementsProperty.InsertArrayElementAtIndex(newIndex);

                                                // Configure the new element with the selected type
                                                SerializedProperty newElementProp = elementsProperty.GetArrayElementAtIndex(newIndex);

                                                // Store the full assembly-qualified name for reliable type resolution
                                                newElementProp.FindPropertyRelative("name").stringValue = type.AssemblyQualifiedName;

                                                // Enable the element by default
                                                newElementProp.FindPropertyRelative("isEnabled").boolValue = true;

                                                // Apply the changes to persist them
                                                property.serializedObject.ApplyModifiedProperties();
                                          });
                                    }

                                    // Show the context menu at the current mouse position
                                    menu.ShowAsContext();
                              }
                  };

                  return list;
            }

            // Converts a full type name to a user-friendly display name for the inspector.
            private static string GetDisplayName(string typeName)
            {
                  if (string.IsNullOrEmpty(typeName))
                  {
                        return "None";
                  }

                  // Attempt to resolve the type from the name
                  var type = Type.GetType(typeName);

                  // Return cleaned name if type exists, otherwise indicate a missing type
                  return type != null ? type.Name.Replace("Toolbar", "", StringComparison.Ordinal) : "(Missing Type)";
            }
      }
}