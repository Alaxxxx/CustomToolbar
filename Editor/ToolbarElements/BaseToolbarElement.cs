using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomToolbar.Editor.ToolbarElements
{
      /// <summary>
      /// The base contract for any element that can be displayed on the Custom Toolbar.
      /// To create a new element, inherit from this class.
      /// </summary>
      [Serializable]
      internal abstract class BaseToolbarElement : IComparable<BaseToolbarElement>
      {
            [NonSerialized]
            private static string rootPath;

            /// <summary>
            /// The display name of the element in the settings UI.
            /// </summary>
            public abstract string Name { get; }

            /// <summary>
            /// The tooltip displayed when hovering over the element in the toolbar.
            /// </summary>
            public virtual string Tooltip => string.Empty;

            /// <summary>
            /// Gets or sets a value indicating whether this element is drawn in the toolbar.
            /// </summary>
            [field: SerializeField]
            public bool Visible { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether this element is interactable (e.g., clickable).
            /// If false, the element will appear greyed out.
            /// </summary>
            [field: SerializeField]
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// Gets or sets the width of the element in the toolbar, in pixels.
            /// </summary>
            [field: SerializeField]
            public float Width { get; set; } = 32;

#region EventLoop

            /// <summary>
            /// One-time initialization logic for the element.
            /// Called once when the editor starts. Use this to load icons or cache styles.
            /// </summary>
            public virtual void OnInit()
            {
            }

            /// <summary>
            /// Called every time the play mode state changes (e.g., entering or exiting play mode).
            /// Allows the element to change its behavior dynamically.
            /// </summary>
            public virtual void OnPlayModeStateChanged(PlayModeStateChange state)
            {
            }

            /// <summary>
            /// Called when the user's selection changes in the editor.
            /// </summary>
            public virtual void OnSelectionChanged()
            {
            }

#endregion

#region Methods

            /// <summary>
            /// Defines how the element should be drawn in the main toolbar.
            /// </summary>
            public abstract void OnDrawInToolbar();

#endregion

            public int CompareTo(BaseToolbarElement other)
            {
                  return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            }

            protected static string GetRootPath()
            {
                  if (!string.IsNullOrEmpty(rootPath))
                  {
                        return rootPath;
                  }

                  // Find the path of this script (BaseToolbarElement.cs)
                  string[] guids = AssetDatabase.FindAssets("t:script BaseToolbarElement");

                  if (guids.Length > 0)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                        // The root is two levels up from the script's directory (ToolbarElements/Editor/CustomToolbar)
                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(path));
                  }
                  else
                  {
                        // Fallback in case the script cannot be found
                        rootPath = "Assets/CustomToolbar";
                  }

                  return rootPath;
            }
      }
}