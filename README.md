# Custom Toolbar for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/Alaxxxx/CustomToolbar?style=flat-square)](https://github.com/Alaxxxx/CustomToolbar/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-green.svg)](https://unity3d.com/get-unity/download)

An advanced and highly customizable toolbar extension for the Unity Editor, designed to streamline your workflow and integrate your custom tools seamlessly.

This project was originally inspired by the great work of [smkplus/CustomToolbar](https://github.com/smkplus/CustomToolbar) and has been significantly expanded and refactored with a more robust architecture and many additional features.

<img width="1295" height="32" alt="Screenshot_1" src="https://github.com/user-attachments/assets/62b55f11-0be6-4d7b-8fc7-86ab60c27ee4" />

## ✨ Features

* **Powerful Configuration:** A polished, intuitive settings window to manage every aspect of your toolbar.
* **Group Management:** Organize your tools into groups, reorder them with simple buttons, and assign them to the left or right side of the play controls.
* **Extensible by Design:** Easily create your own toolbar elements by inheriting from a simple base class.
* **The Toolbox:** A fully customizable dropdown menu for shortcuts to any Unity window, project asset, URL, or even static C# methods (macros).
* **Built-in Elements:** Comes with a rich set of essential tools out of the box, including scene management, screenshot utilities, play mode controls, and Git integration.

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

## ⚙️ Configuring the Toolbar

All customization is done from a two-panel interface in **`Project Settings > Custom Toolbar`**.

<img width="862" height="618" alt="Screenshot_SettingsUI" src="https://github.com/user-attachments/assets/4b4107a7-af30-47ae-9e6d-80ef0e092d97" />

* **Left Panel:**
    * View all your groups, neatly separated by **Left Side** and **Right Side**.
    * Use the **▲/▼ buttons** to reorder groups within their sides.
    * Use the **search bar** at the top to quickly find groups or Toolbox shortcuts.
    * Manage your **Toolbox Shortcuts** with the same folder structure.

* **Right Panel:**
    * When a group is selected, you can rename it, enable/disable it, or move it to the other side of the toolbar.
    * Add, remove, and reorder the elements within that group.
    * When a Toolbox shortcut is selected, use the **Shortcut Editor** to configure its action.

## 🧰 The Toolbox: Your Shortcut Menu

The Toolbox is one of the most powerful features. It's a special dropdown designed to hold all your custom shortcuts.

### The Advanced Shortcut Editor

Instead of manually typing commands, the editor guides you:
1.  **Select an Action Type:** Choose between opening a Window, an Asset, a URL, a GameObject, a Folder or calling a static C# Method.
2.  **Use the Interactive Fields:** The UI adapts to your choice, providing object fields for assets, a text field for URLs, or the a Method Editor.
3.  **The Method Editor:** Simply drag your C# script into the "Script File" field, and the editor will automatically list all available `public static` methods. Select one, and the editor will generate fields for each parameter, completely removing the guesswork.

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

## Adding Your New Element
After saving your script, Unity will recompile. Your new element is now ready to be used!

1. Navigate to Edit > Project Settings > Custom Toolbar.

2. Select the group where you want to add your new button.

3. In the right panel, click the "Add Element" button. A menu will appear with all available elements, including the one you just created.

4. Select your element from the list to add it to the group.

Click "Save and Recompile". Your new button will appear on the main toolbar!
