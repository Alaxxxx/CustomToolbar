# Custom Toolbar for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An advanced and highly customizable toolbar extension for the Unity Editor, designed to streamline your workflow and integrate your custom tools seamlessly.

This project was originally inspired by the great work of [smkplus/CustomToolbar](https://github.com/smkplus/CustomToolbar) and has been significantly expanded and refactored with a more robust architecture and many additional features.

<img width="1295" height="32" alt="Screenshot_1" src="https://github.com/user-attachments/assets/62b55f11-0be6-4d7b-8fc7-86ab60c27ee4" />

## Features

-   **Fully Configurable:** Organize your tools into groups, enable or disable any element, and arrange them on the left or right side of the play buttons.
-   **Extensible Toolbar:** Easily create your own toolbar elements by inheriting from a simple base class.
-   **Powerful Toolbox:** Create a dropdown menu of custom shortcuts to any Unity or user-made editor window, asset, URL, or even static C# methods ("macros").
-   **Built-in Tools:** Comes with a rich set of essential tools out of the box, including scene management, screenshot utilities, play mode controls, and Git integration.

## Installation

<details>
<summary><strong>1. Install via Git URL (Recommended)</strong></summary>
<br>
This method installs the package directly from the GitHub repository and allows you to easily update to the latest version.

1.  In Unity, open the **Package Manager** (`Window > Package Management > Package Manager`).
2.  Click the **+** button in the top-left corner and select **"Add package from git URL..."**.
3.  Enter the following URL and click "Install":
    ```
    https://github.com/Alaxxxx/CustomToolbar.git
    ```
</details>

<details>
<summary><strong>2. Install via .unitypackage</strong></summary>
<br>
This method is great if you prefer a specific, stable version of the asset.

1.  Go to the [**Releases**](https://github.com/Alaxxxx/CustomToolbar/releases) page of this repository.
2.  Download the `.unitypackage` file from the latest release (or a specific version you want).
3.  In your Unity project, go to **`Assets > Import Package > Custom Package...`**.
4.  Locate the downloaded `.unitypackage` file and import it.
</details>

<details>
<summary><strong>3. Manual Installation (from .zip)</strong></summary>
<br>

1.  Download this repository as a ZIP file by clicking **`Code > Download ZIP`** on the main repository page.
2.  Unzip the downloaded file.
3.  Drag and drop the main asset folder (the one containing all the scripts and resources) into the `Assets` folder of your Unity project.
</details>

## How to Use

Once installed, the custom toolbar will appear automatically with a default layout. All customization is done through the Project Settings window.

### Configuring the Toolbar

To configure the toolbar, go to **`Edit > Project Settings > Custom Toolbar`**.

<img width="859" height="620" alt="Screen_Settings1" src="https://github.com/user-attachments/assets/c33d5876-5a3c-4e80-a3ba-4814a99419e7" />

From this panel, you can:
* **Manage Groups:** Create, delete, or rename groups. The group name is for your organization only.
* **Organize Elements:** Drag and drop elements to reorder them within a group, or move them between groups.
* **Enable/Disable:** Use the checkboxes to toggle the visibility of any group or individual element.
* **Add New Elements:** Click the `+` button at the bottom of a group to add a new element, including any custom ones you've created.

## Toolbar Elements

### Version Control
* **Git Status**: Displays all Git project inside the Unity project, the current Git branch and an indicator (`*`) for uncommitted changes. Click to quickly switch between your branches.

<img width="114" height="31" alt="Screen_Git" src="https://github.com/user-attachments/assets/54a17c9d-42ad-4f6d-9833-55e49c0b704f" />

### Utilities
* **Screenshot**: A dropdown menu to take a high-quality screenshot of either the Game View or the Scene View, with an option to directly open the screenshots folder.
* * **Clear PlayerPrefs**: Deletes all `PlayerPrefs` data for your project with a single click.
* **Save Project**: Saves your current scene and any pending project changes.

<img width="116" height="33" alt="Screen_Utility" src="https://github.com/user-attachments/assets/6bfbc5b6-cd9d-4ec8-b8fc-996be33f2391" />

### Scene Management
* **Scene Selection**: A dropdown to quickly open any scene located in your `Assets/Scenes` folder and its subdirectories. Scenes included in the build are marked with their build index.
* **Start From First Scene**: Starts the game from scene 0 in your Build Settings, then automatically returns you to the scene you were editing when you exit Play Mode.
* **Reload Scene**: Instantly reloads the currently active scene while in Play Mode.

<img width="205" height="30" alt="Screen_Scene" src="https://github.com/user-attachments/assets/17e91934-6d68-4e93-83c7-5a51f3368105" />

### Play Mode Controls
* **FPS Slider**: A slider to control `Application.targetFrameRate`. Set to 0 for unlimited FPS.
* **Time Slider**: A slider to adjust `Time.timeScale`, allowing you to slow down or speed up your game in real-time.

<img width="373" height="27" alt="Screen_Control" src="https://github.com/user-attachments/assets/af77f9b1-ac61-492f-9732-ed09aa1156b6" />

### Debugging & Advanced
* **Enter Play Mode Options**: A quick access dropdown to configure Unity's "Enter Play Mode" settings, letting you disable domain and scene reloads to start Play Mode faster.
* **Recompile**: Manually triggers a script recompilation.
* **Reserialize All Assets**: Forces Unity to reserialize all assets in the project, which can help fix data corruption or serialization issues.

<img width="231" height="29" alt="Screen_Debug" src="https://github.com/user-attachments/assets/b2f4b9e0-55ac-4f7f-83f0-eb953288add0" />

### Toolbox
* **Toolbox**: A fully customizable dropdown menu for all your favorite shortcuts. This powerful tool lets you launch any editor window, asset, URL, or even custom scripts ("macros") with a single click. (More details in its own section).

<img width="47" height="33" alt="Screen_Toolbox" src="https://github.com/user-attachments/assets/76fedc84-a370-4495-a578-0748260b2b6f" />

## Creating Your Own Toolbar Elements

Creating a new button or tool for the toolbar is straightforward.

1.  Create a new C# script that inherits from `BaseToolbarElement`.
2.  Implement the abstract `Name` property and the `OnDrawInToolbar()` method.
3.  Your new element will automatically appear in the "Add Element" dropdown in the settings window.

Here is a comprehensive example demonstrating how to use the various override methods available.

**Example:**
```csharp
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;

namespace Custom
{
      public class MyCustomButton : BaseToolbarElement
      {
            // This is optional, but you can use it to set the button's content such as icon and tooltip.
            private GUIContent _buttonContent;

            // Name is used to identify the button in the toolbar and in the settings and should be implemented.
            protected override string Name => "My Button";

            // Tooltip is optional and can be used to provide additional information about the button.
            protected override string Tooltip => "This is a custom button in the toolbar";

            // This method is called once when the toolbar is initialized.
            public override void OnInit()
            {
                  this.Width = 100; // Set the width of the button in the toolbar. Could also be set in OnDrawInToolbar.

                  this.Enabled = true; // This is true by default but just so that you know it. You can also set it to false if you want it to be disabled initially.

                  // You can set the button content here or directly in OnDrawInToolbar.
                  _buttonContent = new GUIContent(Name, Tooltip);
            }

            // This method is called to draw the button in the toolbar.
            public override void OnDrawInToolbar()
            {
                  // This wait for a click on the button and executes the code inside if clicked.
                  if (GUILayout.Button(_buttonContent))
                  {
                        // This is where you define what happens when the button is clicked.
                        Debug.Log("My custom button was clicked!");
                  }
            }

            public override void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                  // This method is called when the play mode state changes.
                  // You can use it to enable or disable the button based on the play mode state.
                  if (state == PlayModeStateChange.EnteredPlayMode)
                  {
                        this.Enabled = false; // Disable the button when entering play mode.
                  }
                  else if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                  {
                        this.Enabled = true; // Enable the button when exiting play mode or entering edit mode.
                  }
            }
      }
}
```

After creating the script, Unity will recompile, and your new element will become available in the toolbar settings (`Edit > Project Settings > Custom Toolbar`).

Find your new element in the `+` dropdown menu at the bottom of any group and add it to your layout. It will then appear on the main toolbar.

<img width="229" height="338" alt="Screen_3" src="https://github.com/user-attachments/assets/14cc2005-7f64-46dd-be2d-be3cac4d6b29" />

> **Note on Naming Conventions**
> 
> For consistency with the built-in elements, it's a good practice to name your class starting with `Toolbar` (e.g., `ToolbarMyCustomButton`). The settings UI will automatically remove this prefix from the display name for a cleaner look.

If your new element doesn't appear in the settings right away, a manual recompilation might be needed. You can use the built-in **Recompile** button for this purpose.

And then the button will appear and respond on click.

<img width="133" height="30" alt="Screen_Custom" src="https://github.com/user-attachments/assets/181be404-7300-4d8b-bcf1-9277006c5b5d" />

## The Toolbox: Your Customizable Shortcut Menu

The Toolbox is one of the most powerful features of CustomToolbar. It's a special dropdown menu designed to hold all your custom shortcuts, giving you instant access to any window, asset, URL, or even custom scripts ("macros") without ever leaving the toolbar.

By default, the Toolbox comes with a handy shortcut to directly open its own configuration window.

### Configuration

All shortcuts are managed from the `Project Settings > Custom Toolbar` window, in the **Toolbox Shortcuts** section.

<img width="376" height="150" alt="Screen_Toolbox2" src="https://github.com/user-attachments/assets/600cf5cf-f8c9-4d37-8b80-96823da79539" />

For each shortcut, you can configure:

* **Display Name:** The text that will appear in the dropdown menu.
* **Menu (Optional):** A path to organize your shortcuts into sub-menus for better organization. For example, `Macros/Creation` will place the shortcut inside a "Creation" sub-menu, which is itself inside a "Macros" menu.
* **Action Path:** This is the core of the shortcut. It tells the Toolbox what action to perform.

<img width="851" height="730" alt="Screen_Toolbox3" src="https://github.com/user-attachments/assets/a2319570-ccc0-486a-a081-3176dca6211e" />

### The Power of the Action Path

The `Action Path` field uses a prefix-based system to execute different types of commands. This makes it incredibly flexible.

Here is a list of all available prefixes and how to use them:

| Prefix | Example | Description |
| :--- | :--- | :--- |
| (none) | `Window/Analysis/Profiler` | Executes a standard Unity menu item from the top menu bar. |
| `settings:` | `settings:Project/Player` | Opens a specific page in the Project Settings window. |
| `asset:` | `asset:Assets/Prefabs/MyPrefab.prefab` | Selects and pings an asset in the Project view. |
| `folder:` | `folder:Assets/Art/Textures` | Pings a folder in the Project view. |
| `url:` | `url:https://your-project-docs.com` | Opens any URL in your default web browser. |
| `select:` | `select:/Managers/GameManager` | Finds and selects a GameObject by its path in the current scene hierarchy. |
| `method:` | `method:MyNamespace.MyClass.MyMethod\|param1,true,10` | Executes a static C# method with **zero or more parameters**. Use `\|` to separate the method path from the parameters, and `,` to separate the parameters themselves. |
