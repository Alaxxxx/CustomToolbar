using System;
using System.IO;
using UnityEditor;

namespace CustomToolbar.Editor.ToolbarElements
{
      /// <summary>
      /// The base contract for any element that can be displayed on the Custom Toolbar.
      /// To create a new element, inherit from this class.
      /// </summary>
      internal abstract class BaseToolbarElement : IComparable<BaseToolbarElement>
      {
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
            /// Gets or sets a value indicating whether this element is interactable (e.g., clickable).
            /// If false, the element will appear greyed out.
            /// </summary>
            protected bool Enabled { get; set; } = true;

            /// <summary>
            /// Gets or sets the width of the element in the toolbar, in pixels.
            /// </summary>
            protected float Width { get; set; } = 32;

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

                  string[] guids = AssetDatabase.FindAssets("t:script BaseToolbarElement");

                  if (guids.Length > 0)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(path));
                  }
                  else
                  {
                        rootPath = "Assets/CustomToolbar";
                  }

                  return rootPath;
            }
      }
}