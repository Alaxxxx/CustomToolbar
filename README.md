# Custom Toolbar for Unity

<p align="center">
  <a href="https://github.com/Alaxxxx/CustomToolbar/stargazers"><img src="https://img.shields.io/github/stars/Alaxxxx/CustomToolbar?style=flat-square&logo=github&color=FFC107" alt="GitHub Stars"></a>
  &nbsp;
  <a href="https://github.com/Alaxxxx?tab=followers"><img src="https://img.shields.io/github/followers/Alaxxxx?style=flat-square&logo=github&label=Followers&color=282c34" alt="GitHub Followers"></a>
  &nbsp;
  <a href="https://github.com/Alaxxxx/CustomToolbar/commits/main"><img src="https://img.shields.io/github/last-commit/Alaxxxx/CustomToolbar?style=flat-square&logo=github&color=blueviolet" alt="Last Commit"></a>
</p>
<p align="center">
  <a href="https://github.com/Alaxxxx/CustomToolbar/releases"><img src="https://img.shields.io/github/v/release/Alaxxxx/CustomToolbar?style=flat-square" alt="Release"></a>
  &nbsp;
  <a href="https://package.openupm.com"><img src="https://img.shields.io/npm/v/com.opalstudio.customtoolbar?label=openupm&registry_uri=https://package.openupm.com&style=flat-square" alt="OpenUPM"></a>
  &nbsp;
  <a href="https://unity.com/"><img src="https://img.shields.io/badge/Unity-2021.3+-2296F3.svg?style=flat-square&logo=unity" alt="Unity Version"></a>
  &nbsp;
  <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square" alt="License: MIT"></a>
</p>

An advanced and highly customizable toolbar extension for the Unity Editor, designed to streamline your workflow and integrate your custom tools seamlessly.

This project was originally inspired by the great work of [smkplus/CustomToolbar](https://github.com/smkplus/CustomToolbar) and has been significantly expanded and refactored with a more robust architecture and many additional features.

<br>

<img width="1525" height="276" alt="Toolbar" src="https://github.com/user-attachments/assets/4a0a57eb-2476-478e-8523-a5d76893165e" />

<br>

## ✨ Features

* **Powerful Configuration:** A polished, intuitive settings window to manage every aspect of your toolbar.
* **Group Management:** Organize your tools into groups, reorder them with simple buttons, and assign them to the left or right side of the play controls.
* **Extensible by Design:** Easily create your own toolbar elements by inheriting from a simple base class.
* **The Toolbox:** A fully customizable dropdown menu for shortcuts to any Unity window, project asset, URL, or even static C# methods.
* **Built-in Elements:** Comes with a rich set of essential tools out of the box, ready to be added to your toolbar.

<br>

## 🚀 Getting Started

### Installation

<details>
<summary><strong>1. Install via Unity Asset Store (Recommended)</strong></summary>
<br>
This is the easiest way to install and receive updates.

1.  Open the <a href="https://assetstore.unity.com/packages/tools/utilities/customtoolbar-327041">CustomToolbar page on the Unity Asset Store</a> and click <b>"Add to My Assets"</b>.
2.  In your Unity project, open the **Package Manager** (`Window > Package Manager`).
3.  In the Package Manager window, select the **"Packages: My Assets"** view from the top-left dropdown menu.
4.  Find **CustomToolbar** in the list and click the **"Import"** button.

</details>

<details>
<summary><strong>1. Install via Git URL</strong></summary>
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
<summary><strong>1. Install via OpenUPM </strong></summary>
<br>
This is the easiest way to install the package and keep it up to date.

**A) With the OpenUPM CLI:**

1. Install the [OpenUPM CLI](https://openupm.com/docs/cli/).
2. Run the following command in your Unity project's root folder:
   ```sh
   openupm add com.opalstudio.customtoolbar
   ```

**B) Manually in Unity Editor:**

1. In Unity, go to **`Edit > Project Settings > Package Manager`**.
2. Add a new **Scoped Registry** with the following details:
   - **Name:** OpenUPM
   - **URL:** https://package.openupm.com
   - **Scope(s):** com.opalstudio.customtoolbar
3. Click **Save**.
4. Open the Package Manager (`Window > Package Management > Package Manager`).
5. Select **"My Registries"** from the dropdown.
6. Choose the **CustomToolbar** package, and click **Install**.

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

> [!NOTE]
> By default, all tools are enabled to showcase the full capabilities of Custom Toolbar. For an optimal experience, especially on smaller screens, customizing the layout is recommended. See the [**Configuring the Toolbar**](#️-configuring-the-toolbar) section for details on how to create a personalized and efficient workflow.

<br>

## ⚙️ Configuring the Toolbar

All customization is done from a two-panel interface in **`Project Settings > Custom Toolbar`**.

<img width="1210" height="735" alt="Settings" src="https://github.com/user-attachments/assets/0cfbf822-009e-4000-ab21-1d25b7770b62" />

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
 
> [!IMPORTANT]
> After modifying **groups** or their **elements** (adding, removing, or reordering), you must click the **Save and Recompile** button at the bottom of the settings window.
>
> **Why?** The toolbar's layout and its elements are built and injected into the Unity editor only once when scripts are compiled. A recompile is necessary to apply these structural changes.
>
> This step is **not** required when adding or editing **Toolbox shortcuts**. They are loaded dynamically each time you open the menu, so changes are reflected instantly.
 
<br>

  ## 🧰 Built-in Elements

Here is a list of the tools included with the package, ready to be added to your toolbar.

<details>
<summary><strong>Scene Management</strong></summary>
<br>

* **Scene Selection:** A dropdown that lists all scenes from `Assets/Scenes/`, allowing you to open them quickly. It also clearly indicates which scenes are included in the build settings.
* **Start From First Scene:** Starts Play Mode from the first scene listed in your Build Settings, then automatically returns you to your original scene when you exit.
* **Reload Scene:** A button to instantly reload the currently active scene while in Play Mode.
* **Scene Bookmarks:** Save specific camera positions and angles in your scene and navigate to them instantly. A dedicated manager window allows you to add, delete, reorder, and even generate thumbnails for your bookmarks.
* **Layer Visibility:** Control which layers are visible in the Scene View with a handy dropdown. Show, hide, or isolate specific layers to reduce visual clutter while working.

</details>

<details>
<summary><strong>Development & Debugging</strong></summary>
<br>

* **Find Missing References:** Scans the active scene for broken or missing component references and displays them in a clean, user-friendly window, letting you select the problematic GameObjects directly.
* **Clear PlayerPrefs:** Deletes all data saved in PlayerPrefs with a single click, including a confirmation dialog to prevent accidental data loss.
* **Recompile Scripts:** Manually trigger a script compilation without having to modify a file.
* **Reserialize All Assets:** Forces a reserialization of all assets in your project. A powerful tool for fixing stubborn serialization errors or after a major Unity upgrade.
* **Play Mode Options:** Quickly configure the "Enter Play Mode" settings directly from the toolbar, allowing you to disable domain and scene reloads for lightning-fast iteration times.

</details>

<details>
<summary><strong>Utilities</strong></summary>
<br>

* **Save Project:** A convenient button that saves both the current scene(s) and all modified project assets in one go.
* **Git Status:** If your project is a Git repository, this displays the current branch and an icon indicating if there are uncommitted changes. It also lets you switch between local branches directly from the editor.
* **Screenshot:** A dropdown menu to instantly capture the Game View or Scene View and save it to a `Screenshots` folder. Also includes a shortcut to open this folder directly.
* **FPS Slider:** Controls the application's target frame rate (`Application.targetFrameRate`), perfect for testing performance under different conditions.
* **Timescale Slider:** Controls the game's speed (`Time.timeScale`) during play mode. Invaluable for slow-motion analysis or fast-forwarding through tedious sequences.
* **Toolbox:** Your personal, fully customizable dropdown menu for shortcuts. See the dedicated section below for more details.
* **Favorites:** Opens a powerful window to create and manage custom lists of your most-used assets. Add, remove, and reorder assets with drag-and-drop ease.

</details>

<br>

## 🛠️ The Toolbox: Your Ultimate Shortcut Menu

The Toolbox is one of the most powerful features of Custom Toolbar. It's a special, fully customizable dropdown menu designed to hold all your essential shortcuts. Instead of navigating through complex menus, you can access your most-used windows, assets, tools, and even custom C# methods with a single click.

<br>

<img width="235" height="166" alt="Shortcut_2" src="https://github.com/user-attachments/assets/eb9caa1f-eccf-4c00-8e85-aa1a6ea18508" />

<br>

### The Advanced Shortcut Editor

Configuring shortcuts is made incredibly simple thanks to the **Advanced Shortcut Editor** in the settings window. Instead of forcing you to manually type commands and paths, the editor provides a guided, context-aware interface that adapts to the type of shortcut you want to create.

<br>

<img width="1208" height="735" alt="Shortcut_1" src="https://github.com/user-attachments/assets/029be315-2044-4166-adda-3aa954c291a1" />

<br>

1.  **Select an Action Type:** First, choose what you want your shortcut to do. The available actions are:
    * **Window:** Opens any built-in or custom Unity Editor window (e.g., `Project Settings`, `Animation`, `Profiler`). A handy browser is included to help you find the exact menu path without guesswork.
    * **Project Asset:** Instantly opens or highlights any asset in your project (e.g., a specific prefab, a material, a scene file). Simply drag and drop the asset into the object field.
    * **Project Folder:** Pings a specific folder in your Project window, allowing you to navigate to it instantly.
    * **URL:** Opens a web link in your default browser. Perfect for documentation, bug trackers, or team resources.
    * **GameObject:** Selects and pings a specific GameObject in the current scene's hierarchy.
    * **Method:** Calls a static C# method from any script in your project. This is perfect for creating custom tools and powerful one-click macros.

2.  **Use the Interactive Fields:** Based on your choice, the UI adapts to give you the right fields. You'll get an object field for assets and GameObjects, a simple text field for URLs, and the powerful Method Editor for C# methods.

3.  **Unleash the Power of the Method Editor:** The "Method" action type transforms the Toolbox into a potent script runner.
    * Simply drag your C# script file into the "Script File" field.
    * The editor automatically inspects the script and lists all available `public static` methods.
    * Once you select a method, the editor intelligently generates fields for each of its parameters (`int`, `float`, `string`, `bool`, and `enum` types are supported).
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

## ✨ Adding Your New Element

After your script recompiles, your new element is ready to be used!

1.  Navigate to `Edit > Project Settings > Custom Toolbar`.
2.  Select the group where you want to add your new button.
3.  In the right panel, click the **`Add Element`** button. A menu will appear with all available elements.
4.  Select your new element from the list to add it to the group.

> [!IMPORTANT]
> Click **`Save and Recompile`** at the bottom of the window. Your new button will now appear on the main toolbar!
<br>

## 🤝 Contributing & Supporting

This project is open-source and community-driven. Any form of contribution is welcome and greatly appreciated!

First and foremost, if you find `Custom Toolbar` useful, please **give it a star ⭐️ on GitHub!** It's the easiest way to show your support and helps the project gain visibility.

Here are other ways you can contribute:

* **💡 Share Ideas & Report Bugs:** Have a great idea for a new feature or found a bug? [Open an issue](https://github.com/Alaxxxx/CustomToolbar/issues) to share the details. Well-documented reports are fantastic.

* **🔌 Contribute Code:** Feel free to fork the repository and submit a pull request for bug fixes or new features.

* **🗣️ Spread the Word:** Share this tool with other Unity developers who might find it useful.

Every contribution, from a simple star to a pull request, is incredibly valuable. Thank you for your support!
