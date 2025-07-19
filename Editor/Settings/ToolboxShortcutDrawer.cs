using CustomToolbar.Editor.Settings.Data;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.Settings
{
      [CustomPropertyDrawer(typeof(ToolboxShortcut))]
      public class ToolboxShortcutDrawer : PropertyDrawer
      {
            private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
            private const float VerticalSpacing = 10f;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                  EditorGUI.BeginProperty(position, label, property);

                  SerializedProperty enabledProp = property.FindPropertyRelative("isEnabled");
                  SerializedProperty nameProp = property.FindPropertyRelative("displayName");
                  SerializedProperty subMenuProp = property.FindPropertyRelative("subMenuPath");
                  SerializedProperty pathProp = property.FindPropertyRelative("menuItemPath");

                  Rect backgroundRect = position;
                  backgroundRect.y += 1;
                  backgroundRect.height -= 2;
                  EditorGUI.DrawRect(backgroundRect, new Color(0, 0, 0, 0.1f));

                  var contentRect = new Rect(position.x + 5, position.y + 5, position.width - 10, position.height - 10);

                  float currentY = contentRect.y;

                  var line1Rect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
                  var toggleRect = new Rect(line1Rect.x, line1Rect.y, 20, line1Rect.height);
                  var nameRect = new Rect(toggleRect.xMax, line1Rect.y, line1Rect.width - toggleRect.width, line1Rect.height);

                  enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);

                  nameProp.stringValue = EditorGUI.TextField(nameRect, nameProp.stringValue);
                  currentY += lineHeight + VerticalSpacing;

                  var menuRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
                  var menuLabel = new GUIContent("Menu (Optional)", "Chemin du sous-menu (ex: 'Macros/Creation')");
                  subMenuProp.stringValue = EditorGUI.TextField(menuRect, menuLabel, subMenuProp.stringValue);
                  currentY += lineHeight + VerticalSpacing;

                  var pathLabelRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight);
                  var pathLabel = new GUIContent("Action Path", "Chemin de l'action à exécuter (ex: 'settings:Project/Custom Toolbar')");
                  EditorGUI.LabelField(pathLabelRect, pathLabel);
                  currentY += lineHeight;

                  var textAreaRect = new Rect(contentRect.x, currentY, contentRect.width, lineHeight * 2);

                  pathProp.stringValue = EditorGUI.TextArea(textAreaRect, pathProp.stringValue, EditorStyles.textArea);

                  EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                  return (lineHeight * 5) + (VerticalSpacing * 4);
            }
      }
}