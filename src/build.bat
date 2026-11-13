@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set SCRIPT_DIR=%~dp0
set OUT=%SCRIPT_DIR%bin\Release\TharaThemeExplorer.exe
set ICON=%SCRIPT_DIR%Resources\TharaThemeExplorer.ico
set RESDIR=%SCRIPT_DIR%Resources
if not exist "%SCRIPT_DIR%bin\Release" mkdir "%SCRIPT_DIR%bin\Release"

pushd "%SCRIPT_DIR%"
"%CSC%" /target:winexe /platform:anycpu /optimize+ /win32icon:"%ICON%" /win32manifest:app.manifest ^
  /resource:"%RESDIR%\logo.png",logo.png ^
  /resource:"%RESDIR%\opt_w7_exp.png",opt_w7_exp.png ^
  /resource:"%RESDIR%\opt_w10_exp.png",opt_w10_exp.png ^
  /resource:"%RESDIR%\opt_w11_exp.png",opt_w11_exp.png ^
  /resource:"%RESDIR%\opt_w10_menu.png",opt_w10_menu.png ^
  /resource:"%RESDIR%\opt_w11_menu.png",opt_w11_menu.png ^
  /out:"%OUT%" ^
  Properties\AssemblyInfo.cs ^
  Core\NativeMethods.cs ^
  Core\ThemeColors.cs ^
  Core\RegOpt.cs ^
  Core\ShellHelper.cs ^
  Core\TypographyHelper.cs ^
  Core\Watcher.cs ^
  UI\ToggleSwitch.cs ^
  UI\SegmentedControl.cs ^
  UI\ThumbnailSelector.cs ^
  UI\PillButton.cs ^
  UI\CommonControls.cs ^
  UI\FolderBox.cs ^
  UI\FontPicker.cs ^
  UI\TabStrip.cs ^
  UI\CardPanel.cs ^
  UI\ColumnsCard.cs ^
  UI\HeaderPanel.cs ^
  Forms\MainForm.cs ^
  Program.cs
popd
