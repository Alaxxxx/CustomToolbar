# Custom Toolbar for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/Alaxxxx/CustomToolbar?style=flat-square)](https://github.com/Alaxxxx/CustomToolbar/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-green.svg)](https://unity3d.com/get-unity/download)

An advanced and highly customizable toolbar extension for the Unity Editor, designed to streamline your workflow and integrate your custom tools seamlessly.

This project was originally inspired by the great work of [smkplus/CustomToolbar](https://github.com/smkplus/CustomToolbar) and has been significantly expanded and refactored with a more robust architecture and many additional features.

<br>

<img width="1372" height="33" alt="Screenshot_1" src="https://github.com/user-attachments/assets/37aba9f3-9023-489a-b2c2-81274acfc632" />

<br>

## ✨ Features

* **Powerful Configuration:** A polished, intuitive settings window to manage every aspect of your toolbar.
* **Group Management:** Organize your tools into groups, reorder them with simple buttons, and assign them to the left or right side of the play controls.
* **Extensible by Design:** Easily create your own toolbar elements by inheriting from a simple base class.
* **The Toolbox:** A fully customizable dropdown menu for shortcuts to any Unity window, project asset, URL, or even static C# methods (macros).
* **Built-in Elements:** Comes with a rich set of essential tools out of the box, ready to be added to your toolbar.

<br>

## 🚀 Getting Started

### Installation

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

1.  Go to the [**Releases**](https://github.com/Alaxxxx/CustomToolbar/releases) page.
2.  Download the `.unitypackage` file from the latest release.
3.  In your Unity project, go to **`Assets > Import Package > Custom Package...`** and select the downloaded file.
</details>

<details>
<summary><strong>3. Manual Installation (from .zip)</strong></summary>
<br>

1.  Download this repository as a ZIP file by clicking **`Code > Download ZIP`** on the main repository page.
2.  Unzip the downloaded file.
3.  Drag and drop the main asset folder (the one containing all the scripts and resources) into the `Assets` folder of your Unity project.
</details>

### How to Use

Once installed, the custom toolbar will appear automatically with a default layout. All customization is done through the Project Settings window.

<br>

## ⚙️ Configuring the Toolbar

All customization is done from a two-panel interface in **`Project Settings > Custom Toolbar`**.

<img width="865" height="618" alt="Screenshot_Settings" src="https://github.com/user-attachments/assets/e92ffde0-55e5-47b3-b540-903a5909c478" />

* **Left Panel:**
    * View all your groups, neatly separated by **Left Side** and **Right Side**.
    * Use the **▲/▼ buttons** to reorder groups within their sides.
    * Use the **search bar** at the top to quickly find groups or Toolbox shortcuts.
    * Manage your **Toolbox Shortcuts** which can be organized into sub-menus.

* **Right Panel:**
    * When a group is selected, you can rename it, enable/disable it, or move it to the other side of the toolbar.
    * Add, remove, and reorder the elements within that group.
    * When a Toolbox shortcut is selected, use the **Shortcut Editor** to configure its action.
 
<br>

  ## 🧰 Built-in Elements

Here is a list of the tools included with the package, ready to be added to your toolbar.

<details>
<summary><strong>Scene Management</strong></summary>
<br>

* **Scene Selection:** A dropdown that lists all scenes from `Assets/Scenes/`, allowing you to open them quickly. It also indicates which scenes are in the build settings.
* **Start From First Scene:** Starts Play Mode from the first scene listed in your Build Settings, then returns you to the original scene when you exit.
* **Reload Scene:** A button to reload the currently active scene while in Play Mode.
* **Scene Bookmarks:** Save and quickly navigate to specific camera positions in your scene. A manager window allows you to add, delete, reorder, and generate thumbnails for your bookmarks.

</details>

<details>
<summary><strong>Development & Debugging</strong></summary>
<br>

* **Find Missing References:** Scans the active scene for missing component references and displays them in an organized, user-friendly window.
* **Clear PlayerPrefs:** Deletes all data saved in PlayerPrefs with a confirmation dialog to prevent accidental data loss.
* **Recompile Scripts:** Manually trigger a script compilation.
* **Reserialize All Assets:** Forces a reserialization of all assets in the project. Useful for fixing serialization errors or after a Unity upgrade.
* **Play Mode Options:** A dropdown to configure the "Enter Play Mode" settings, allowing you to disable domain/scene reloads for faster iteration.

</details>

<details>
<summary><strong>Utilities</strong></summary>
<br>

* **Save Project:** A single button to save both the current scene(s) and all modified project assets.
* **Git Status:** If your project is a Git repository, this shows the current branch and indicates if there are uncommitted changes. It also allows you to switch between local branches.
* **Screenshot:** A dropdown menu to capture the Game View or Scene View and save it to a `Screenshots` folder. It also provides a shortcut to open this folder.
* **FPS Slider:** Controls the application's target frame rate (`Application.targetFrameRate`).
* **Timescale Slider:** Controls the game's speed (`Time.timeScale`) during play mode, perfect for slow-motion analysis or fast-forwarding.
* **Toolbox:** Your personal, customizable dropdown menu for shortcuts.

</details>

<br>

## 🧰 The Toolbox: Your Ultimate Shortcut Menu

The Toolbox is one of the most powerful features of Custom Toolbar. It's a special, fully customizable dropdown menu designed to hold all your essential shortcuts. Instead of navigating through complex menus, you can access your most-used windows, assets, tools, and even custom C# methods with a single click.

<img width="213" height="76" alt="Capture d'écran 2025-07-21 094324" src="https://github.com/user-attachments/assets/df7f30e8-55d8-4fd1-91d3-38579fe2d4f8" />

<br>

### The Advanced Shortcut Editor

Configuring shortcuts is made incredibly simple thanks to the **Advanced Shortcut Editor**. Instead of forcing you to manually type commands and paths, the editor provides a guided, context-aware interface that adapts to the type of shortcut you want to create.

1.  **Select an Action Type:** First, choose what you want your shortcut to do. The available actions are:
    * **Window:** Opens any built-in or custom Unity Editor window (e.g., `Project Settings`, `Animation`, `Profiler`). A browser is included to help you find the exact menu path.
    * **Project Asset:** Instantly opens or highlights any asset in your project (e.g., a specific prefab, a material, a scene file). Simply drag and drop the asset into the object field.
    * **Project Folder:** Pings a specific folder in your Project window, allowing you to navigate to it instantly.
    * **URL:** Opens a web link in your default browser. Perfect for documentation, bug trackers, or team resources.
    * **GameObject:** Selects and pings a specific GameObject in the current scene's hierarchy.
    * **Method:** Calls a static C# method from any script in your project. This is perfect for creating custom tools and macros.

2.  **Use the Interactive Fields:** Based on your choice, the UI adapts to give you the right fields. You'll get an object field for assets and GameObjects, a simple text field for URLs, and the powerful Method Editor for C# methods.

3.  **Unleash the Power of the Method Editor:** The "Method" action type transforms the Toolbox into a powerful script runner.
    * Simply drag your C# script file into the "Script File" field.
    * The editor will automatically inspect the script and list all available `public static` methods.
    * Once you select a method, the editor generates fields for each of its parameters (`int`, `float`, `string`, `bool`, and enums are supported).
    * This removes all the guesswork and potential for typos, allowing you to create complex macro shortcuts with ease.
  
<img width="860" height="618" alt="Capture d'écran 2025-07-21 094307" src="https://github.com/user-attachments/assets/2520f541-ec91-43e9-8175-3675679e3565" />

<br>

##  Extending the Toolbar

Creating a new button or tool for the toolbar is straightforward.

1.  Create a new C# script that inherits from `BaseToolbarElement`.
2.  Implement the abstract `Name` property and the `OnDrawInToolbar()` method.
3.  Your new element will automatically appear in the "Add Element" dropdown in the settings window!

**Example:**
```csharp
using CustomToolbar.Editor.ToolbarElements;
using UnityEditor;
using UnityEngine;

// For consistency, name your class starting with "Toolbar".
// The settings UI will automatically remove this prefix for a cleaner display name.
public class ToolbarMyCustomButton : BaseToolbarElement
{
    private GUIContent _buttonContent;

    // The name displayed in the settings window.
    protected override string Name => "My Custom Button";
    
    // The tooltip displayed when hovering over the element.
    protected override string Tooltip => "This button logs a message to the console.";

    // Called once when the toolbar is initialized. Use it for setup.
    public override void OnInit()
    {
        _buttonContent = new GUIContent("Log", this.Tooltip);
    }

    // Called every frame to draw the element in the toolbar.
    public override void OnDrawInToolbar()
    {
        if (GUILayout.Button(_buttonContent, GUILayout.Width(60)))
        {
            Debug.Log("My custom button was clicked!");
        }
    }

    // Called when the editor's play mode state changes.
    public override void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Example: disable the button in play mode.
            this.Enabled = false; 
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            this.Enabled = true;
        }
    }
}
```

<br>

## Adding Your New Element
After saving your script, Unity will recompile. Your new element is now ready to be used!

1. Navigate to Edit > Project Settings > Custom Toolbar.

2. Select the group where you want to add your new button.

3. In the right panel, click the "Add Element" button. A menu will appear with all available elements, including the one you just created.

4. Select your element from the list to add it to the group.

Click "Save and Recompile". Your new button will appear on the main toolbar!
