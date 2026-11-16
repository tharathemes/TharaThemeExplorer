# Thara Theme Explorer

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white)](https://tharathemes.com)
[![Architecture](https://img.shields.io/badge/Architecture-Universal%20AnyCPU%20(x86%20%2F%20x64)-blue)](#system-requirements)
[![Framework](https://img.shields.io/badge/.NET-Framework%204.0%2B-512BD4?logo=dotnet)](#system-requirements)
[![Website](https://img.shields.io/badge/Website-tharathemes.com-ec4899)](https://tharathemes.com)

A modern, lightweight Windows utility to customize File Explorer navigation, layout, context menus, fonts, and visuals.

---

## ✨ Features

* **Explorer Layout:** Choose Windows 7-style, Windows 10-style, or Windows 11-style layouts on supported Windows 11 builds.
* **Context Menu:**
  * **Windows 10:** Instant classic full right-click menu (no "Show more options" delay).
  * **Windows 11:** Modern Fluent acrylic context menu.
* **Navigation Pane Customization:**
  * **Hide Items:** Toggle visibility for Home, Gallery, OneDrive, and Libraries.
  * **Add Items:** Show or hide Recycle Bin, Control Panel, User folder, and Network, and add custom files or folders.
  * **Order:** Customize Home and Gallery position (**Top** / **Bottom**).
* **Visuals & Window Chrome:**
  * Toggle **Mica effect** in Explorer windows for higher contrast and snappiness.
* **Typography:**
  * Customize system fonts and font sizes for menus, window titles, icons, and dialogs.
* **Shell Integration:**
  * One-click **Take Ownership** context menu shortcut for files and folders.
* **Lightweight & Clean:**
  * Portable executable using .NET Framework.
  * High-DPI aware (`PerMonitorV2`).
  * Automatic Explorer process recovery.

---

## 💻 System Requirements

* **Operating System:** Windows 10 version 1809 or later (32-bit or 64-bit), or Windows 11 (64-bit).
* **Runtime:** .NET Framework 4.0 or later.
* Feature behavior can vary between Windows versions and builds. Some changes request administrator approval and an Explorer restart.

---

## 🚀 Download & Installation

1. Go to the [Releases](https://github.com/tharathemes/TharaThemeExplorer/releases) page.
2. Download `TharaThemeExplorer v2.2.0.exe` from the version 2.2.0 release.
3. Run the executable.
4. Choose your preferred settings, then click **Apply** or **OK**.

> **Note on First Launch:**  
> This executable is not digitally signed. Windows SmartScreen may show an unrecognized-app warning on first run.

---

## 🛠️ Building from Source

### Prerequisites
* Windows 10 or 11
* .NET Framework 4.0+ developer tools or Visual Studio / MSBuild

### Build via Command Line
Run the included batch script inside the `src` folder:
```cmd
cd src
build.bat
```
The compiled executable will be generated at `src\bin\Release\TharaThemeExplorer.exe`.

---

## 📄 License

This software is distributed for personal, non-commercial use. See [LICENSE.txt](LICENSE.txt) for details.

* Official Website: [tharathemes.com](https://tharathemes.com)
