using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace TharaThemeExplorer.Core
{
    internal static class ShellHelper
    {
        public const string MyName = "Thara Theme Explorer";
        public static readonly string MyExe = Assembly.GetExecutingAssembly().Location;
        public const string MyKey = @"HKEY_CURRENT_USER\Software\TharaThemeExplorer";
        public const string HKCUP = "HKEY_CURRENT_USER\\";

        public const string E10A = @"Software\Classes\CLSID\{2aa9162e-c906-4dd9-ad0b-3d24a8eef5a0}";
        public const string E10B = @"Software\Classes\CLSID\{6480100b-5a83-4d1e-9f69-8ae5a88e9a33}";
        public const string CCMA = @"Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}";
        public static readonly string CCMB = CCMA + @"\InprocServer32";
        public const string RIBBONCLSID = @"Software\Classes\CLSID\{926749FA-2615-4987-8845-C33E65F2B957}";
        public const string ADVK = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        public const string ADVF = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        public const string TBAR = @"Software\Microsoft\Internet Explorer\Toolbar";
        public const string RUNKEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        public const string WATCHVAL = "TharaThemeExplorerWatcher";

        public static readonly string[] TOKEYS = new string[]
        {
            @"Software\Classes\*\shell\runas",
            @"Software\Classes\Directory\shell\runas"
        };
        public const string TOLABEL = "Take Ownership";

        public static readonly string[] PCGUIDS = new string[]
        {
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}",
            "{d3162b92-9365-467a-956b-92703aca08af}",
            "{088e3905-0323-4b02-9826-5d99428e115f}",
            "{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}",
            "{24ad3ad4-a569-4530-98e1-ab02f9417aa8}",
            "{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}"
        };
        public const string PCKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\";

        public static readonly string[,] NavItems = new string[,]
        {
            { "Home", "{f874310e-b6b7-47dc-bc84-b9e6b38f5903}" },
            { "Gallery", "{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}" },
            { "OneDrive", "{018D5C66-4533-4307-9B53-224DE2ED1FE6}" },
            { "Libraries", "{031E4825-7B94-4dc3-B131-E946B44C8DD5}" }
        };

        public static int WinBuild { get; private set; }
        public static bool IsWin11 { get; private set; }
        private static readonly List<string> runLog = new List<string>();
        public static List<string> RunLog { get { return runLog; } }

        static ShellHelper()
        {
            try
            {
                string cvKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion";
                object buildObj = Registry.GetValue(cvKey, "CurrentBuild", "0");
                int build;
                int.TryParse(buildObj != null ? buildObj.ToString() : "0", out build);
                WinBuild = build;
                IsWin11 = build >= 21996;
            }
            catch
            {
                WinBuild = 0;
                IsWin11 = false;
            }
        }

        public static void Log(string m)
        {
            RunLog.Add(DateTime.Now.ToString("HH:mm:ss.fff  ") + m);
        }

        public static void LogErr(string where, Exception ex)
        {
            Log("FAILED  " + where + ": " + ex.GetType().Name + " - " + ex.Message);
        }

        public static string LogPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TharaThemeExplorer", "last-run.log");
        }

        public static void SaveLog()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath()));
                File.WriteAllLines(LogPath(), RunLog.ToArray());
            }
            catch { }
        }

        public static string WinVersionLabel()
        {
            string ver = string.Empty;
            try
            {
                string cvKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion";
                object obj = Registry.GetValue(cvKey, "DisplayVersion", null) ?? Registry.GetValue(cvKey, "ReleaseId", null);
                if (obj != null) ver = obj.ToString();
            }
            catch { }
            return string.Format("{0}{1} detected (Build {2})",
                IsWin11 ? "Windows 11" : "Windows 10",
                ver.Length > 0 ? " " + ver : string.Empty,
                WinBuild);
        }

        public static int ReadDword(string key, string name, int fallback)
        {
            try
            {
                object val = Registry.GetValue(key, name, null);
                return (val is int) ? (int)val : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        public static void WriteDword(string key, string name, int value)
        {
            try
            {
                Registry.SetValue(key, name, value, RegistryValueKind.DWord);
            }
            catch { }
        }

        public static void DeleteDword(string key, string name)
        {
            try
            {
                if (key.StartsWith(HKCUP, StringComparison.OrdinalIgnoreCase))
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(key.Substring(HKCUP.Length), true))
                    {
                        if (rk != null) rk.DeleteValue(name, false);
                    }
                }
            }
            catch { }
        }

        public static void ReadStyleState(out bool win10Explorer, out bool win10ContextMenu)
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(E10A))
            {
                win10Explorer = (rk != null);
            }
            using (RegistryKey rk2 = Registry.CurrentUser.OpenSubKey(CCMB))
            {
                win10ContextMenu = (rk2 != null);
            }
        }

        public static bool RibbonOverrideActive()
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(RIBBONCLSID + @"\InProcServer32"))
                {
                    return rk != null && (rk.GetValue(string.Empty) as string) == string.Empty;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void ApplyRibbonOverride(bool block)
        {
            try
            {
                if (block)
                {
                    Registry.SetValue(HKCUP + RIBBONCLSID + @"\InProcServer32", string.Empty, string.Empty, RegistryValueKind.String);
                }
                else if (RibbonOverrideActive())
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RIBBONCLSID, false);
                }
            }
            catch { }
        }

        public static bool TakeOwnershipOn()
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(TOKEYS[0]))
                {
                    return rk != null && (rk.GetValue(string.Empty) as string) == TOLABEL;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetTakeOwnership(bool on)
        {
            if (on)
            {
                WriteTakeOwnership(TOKEYS[0], "cmd.exe /c takeown /f \"%1\" && icacls \"%1\" /grant *S-1-5-32-544:F");
                WriteTakeOwnership(TOKEYS[1], "cmd.exe /c takeown /f \"%1\" /r /d y && icacls \"%1\" /grant *S-1-5-32-544:F /t");
            }
            else
            {
                foreach (string sub in TOKEYS)
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(sub))
                    {
                        if (rk == null || (rk.GetValue(string.Empty) as string) != TOLABEL)
                            continue;
                    }
                    Registry.CurrentUser.DeleteSubKeyTree(sub, false);
                }
            }
        }

        private static void WriteTakeOwnership(string sub, string cmd)
        {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(sub))
            {
                if (rk != null)
                {
                    rk.SetValue(string.Empty, TOLABEL);
                    rk.SetValue("HasLUAShield", string.Empty);
                    rk.SetValue("NoWorkingDirectory", string.Empty);
                    using (RegistryKey rk2 = rk.CreateSubKey("command"))
                    {
                        if (rk2 != null) rk2.SetValue(string.Empty, cmd);
                    }
                }
            }
        }

        public static bool IsAdmin()
        {
            try
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static bool ThisPcFoldersShown()
        {
            try
            {
                foreach (string guid in PCGUIDS)
                {
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(PCKEY + guid))
                    {
                        if (rk == null) return false;
                        object val = rk.GetValue("HiddenByDefault");
                        if ((val is int && (int)val == 1) || rk.GetValue("HideIfEnabled") != null)
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetThisPcFolders(bool on)
        {
            string sortKey = HKCUP + @"Software\Classes\CLSID\" + PCGUIDS[0] + @"\ShellFolder";
            if (on)
            {
                WriteDword(sortKey, "SortOrderIndex", 0);
            }
            else
            {
                DeleteDword(sortKey, "SortOrderIndex");
            }

            using (Process proc = Process.Start(new ProcessStartInfo(MyExe, "/thispc " + (on ? "on" : "off"))
            {
                Verb = "runas",
                UseShellExecute = true
            }))
            {
                if (proc == null) throw new Exception("Administrator helper could not be started.");
                proc.WaitForExit(30000);
                if (proc.ExitCode != 0) throw new Exception("Administrator helper failed with code " + proc.ExitCode);
            }
        }

        public static int RunThisPcHelper(string mode)
        {
            if (mode != "on" && mode != "off") return 4;
            if (!IsAdmin()) return 3;
            try
            {
                foreach (string guid in PCGUIDS)
                {
                    using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(PCKEY + guid))
                    {
                        if (rk != null)
                        {
                            if (mode == "on")
                            {
                                rk.DeleteValue("HideIfEnabled", false);
                                rk.SetValue("HiddenByDefault", 0, RegistryValueKind.DWord);
                            }
                            else
                            {
                                rk.SetValue("HideIfEnabled", 36354489, RegistryValueKind.DWord);
                                rk.SetValue("HiddenByDefault", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 2;
            }
        }

        public static bool IsNavHidden(string clsid)
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\" + clsid))
                {
                    if (rk == null) return false;
                    object val = rk.GetValue("System.IsPinnedToNameSpaceTree");
                    return (val is int && (int)val == 0);
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetNavHidden(string clsid, bool hide)
        {
            string path = @"Software\Classes\CLSID\" + clsid;
            try
            {
                if (hide)
                {
                    using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(path))
                    {
                        if (rk != null) rk.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                    }
                }
                else
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(path, true))
                    {
                        if (rk != null) rk.DeleteValue("System.IsPinnedToNameSpaceTree", false);
                    }
                }
            }
            catch { }
        }

        public static ChoiceOpt PosOpt(string label, string clsid, int bottomVal)
        {
            string key = HKCUP + @"Software\Classes\CLSID\" + clsid;
            return new ChoiceOpt
            {
                Text = label,
                Items = new string[] { "Top", "Bottom" },
                NeedsRefresh = true,
                ReadFn = () => ReadDword(key, "SortOrderIndex", -1) < bottomVal ? 0 : 1,
                WriteFn = v =>
                {
                    if (v == 1) WriteDword(key, "SortOrderIndex", bottomVal);
                    else DeleteDword(key, "SortOrderIndex");
                }
            };
        }

        public static RegOpt ControlPanelOpt()
        {
            string key = HKCUP + @"Software\Classes\CLSID\{26EE0668-A00A-44D7-9371-BEB064C98683}";
            string name = "System.IsPinnedToNameSpaceTree";
            return RegOpt.Custom("Show Control Panel", () =>
            {
                try
                {
                    object v = Registry.GetValue(key, name, null);
                    return (v is int) && ((int)v == 1);
                }
                catch { return false; }
            },
            on =>
            {
                try
                {
                    if (on)
                    {
                        Registry.SetValue(key, "", "Control Panel");
                        Registry.SetValue(key, name, 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        Registry.SetValue(key, name, 0, RegistryValueKind.DWord);
                    }

                    // Clean up legacy incorrect GUID if present from earlier versions
                    try
                    {
                        string oldSub = @"Software\Classes\CLSID\{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}";
                        using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(oldSub, true))
                        {
                            if (rk != null) Registry.CurrentUser.DeleteSubKeyTree(oldSub, false);
                        }
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    LogErr("Write ControlPanelOpt", ex);
                }
            }, true);
        }

        public static RegOpt[] NavAddOpts()
        {
            string prefix = HKCUP + @"Software\Classes\CLSID\";
            string name = "System.IsPinnedToNameSpaceTree";
            return new RegOpt[]
            {
                new RegOpt("Show Recycle Bin", prefix + "{645FF040-5081-101B-9F08-00AA002F954E}", name, 1, -1, 0, true),
                ControlPanelOpt(),
                new RegOpt("Show User folder", prefix + "{59031a47-3f72-44a7-89c5-5595fe6b30ee}", name, 1, -1, 0, true),
                new RegOpt("Show Network", prefix + "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", name, 1, 0, 1, true)
            };
        }

        public static RegOpt[] TweakOpts()
        {
            RegOpt r1 = new RegOpt("Hide caption text in Explorer windows", MyKey, "HideCaptionText", 1, 0, 0, false) { OffNeedsRefresh = true };
            RegOpt r2 = new RegOpt("Hide caption icon in Explorer windows", MyKey, "HideCaptionIcon", 1, 0, 0, false) { OffNeedsRefresh = true };
            return new RegOpt[] { r1, r2 };
        }

        public static RegOpt StatusBarOpt()
        {
            return new RegOpt("Show status bar", ADVF, "ShowStatusBar", 1, 0, 1, true);
        }

        public static RegOpt MicaOpt()
        {
            return new RegOpt("Disable Mica effect in Explorer windows", MyKey, "DisableMica", 1, 0, 0, false) { OffNeedsRefresh = true };
        }

        public static RegOpt ThisPcOpt()
        {
            RegOpt opt = RegOpt.Custom("Show the Folders group", ThisPcFoldersShown, SetThisPcFolders, true);
            opt.DefaultOn = !IsWin11;
            return opt;
        }

        public static RegOpt TakeOwnershipOpt()
        {
            return RegOpt.Custom("Add \"Take Ownership\"", TakeOwnershipOn, SetTakeOwnership, false);
        }

        public static void SaveSettings(bool e10, bool e11, bool c10, bool c11, bool restart, bool ok2Reload)
        {
            bool win10Exp, win10Ctx;
            ReadStyleState(out win10Exp, out win10Ctx);

            Log(string.Format("SaveSettings: e10={0} e11={1} c10={2} c11={3} restart={4} | current: Win10Exp={5} Win10Ctx={6}",
                e10, e11, c10, c11, restart, win10Exp, win10Ctx));

            if (c10 && !win10Ctx)
            {
                try
                {
                    Registry.SetValue(HKCUP + CCMB, string.Empty, string.Empty, RegistryValueKind.String);
                    Log("Context menu: set to Windows 10");
                }
                catch (Exception ex) { LogErr("Context menu -> Windows 10", ex); }
            }
            if (c11 && win10Ctx)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(CCMA, false);
                    Log("Context menu: set to Windows 11");
                }
                catch (Exception ex) { LogErr("Context menu -> Windows 11", ex); }
            }

            if (e10 && !win10Exp)
            {
                try
                {
                    Registry.SetValue(HKCUP + E10A + @"\InProcServer32", string.Empty, string.Empty, RegistryValueKind.String);
                    Registry.SetValue(HKCUP + E10B + @"\InProcServer32", string.Empty, string.Empty, RegistryValueKind.String);
                    byte[] itbar = new byte[]
                    {
                        19, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 32, 0, 0, 0, 16, 0, 1, 0,
                        0, 0, 0, 0, 1, 0, 0, 0, 1, 7,
                        0, 0, 94, 1, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0
                    };
                    string shellBrowserKey = HKCUP + @"Software\Microsoft\Internet Explorer\Toolbar\ShellBrowser";
                    Registry.SetValue(shellBrowserKey, "ITBar7Layout", itbar, RegistryValueKind.Binary);
                    Log("Explorer style: set to Windows 10 / 7");
                }
                catch (Exception ex) { LogErr("Explorer style -> Windows 10 / 7", ex); }
            }

            if (e11 && win10Exp)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(E10A, false);
                    Registry.CurrentUser.DeleteSubKeyTree(E10B, false);
                    Log("Explorer style: set to Windows 11");
                }
                catch (Exception ex) { LogErr("Explorer style -> Windows 11", ex); }
            }

            if (e10)
            {
                try
                {
                    Registry.SetValue(HKCUP + ADVK, "AlwaysShowMenus", 0, RegistryValueKind.DWord);
                    Registry.SetValue(HKCUP + TBAR, "Locked", 1, RegistryValueKind.DWord);
                }
                catch (Exception ex) { LogErr("Menu bar / toolbar lock", ex); }
            }

            if (ok2Reload || restart)
            {
                RefreshExplorer(restart);
            }
        }

        public static List<string> CaptureExplorerFolders()
        {
            List<string> list = new List<string>();
            try
            {
                Type shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    object windows = shellType.InvokeMember("Windows", BindingFlags.InvokeMethod, null, shell, null);
                    int count = Convert.ToInt32(windows.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, windows, null));
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            object item = windows.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, windows, new object[] { i });
                            if (item != null)
                            {
                                long hwnd = Convert.ToInt64(item.GetType().InvokeMember("HWND", BindingFlags.GetProperty, null, item, null));
                                if (IsExplorerFrame((IntPtr)hwnd))
                                {
                                    string url = Convert.ToString(item.GetType().InvokeMember("LocationURL", BindingFlags.GetProperty, null, item, null));
                                    string localPath = string.Empty;
                                    if (!string.IsNullOrEmpty(url) && url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        localPath = new Uri(url).LocalPath;
                                    }
                                    list.Add(localPath);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogErr("Reading open Explorer windows", ex);
            }
            return list;
        }

        public static bool IsExplorerFrame(IntPtr h)
        {
            StringBuilder sb = new StringBuilder(64);
            return NativeMethods.GetClassName(h, sb, 64) > 0 && sb.ToString() == "CabinetWClass";
        }

        public static void CloseExplorerWindows()
        {
            IntPtr h = NativeMethods.FindWindow("CabinetWClass", null);
            while (h != IntPtr.Zero)
            {
                try
                {
                    if (NativeMethods.IsWindowVisible(h))
                    {
                        NativeMethods.PostMessage(h, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }
                    h = NativeMethods.FindWindowEx(IntPtr.Zero, h, "CabinetWClass", null);
                }
                catch { break; }
            }
        }

        public static void RestartExplorer()
        {
            int sessionId = Process.GetCurrentProcess().SessionId;
            int count = 0;
            foreach (Process p in Process.GetProcessesByName("explorer"))
            {
                try
                {
                    if (p.SessionId == sessionId)
                    {
                        p.Kill();
                        p.WaitForExit(5000);
                        count++;
                    }
                }
                catch (Exception ex) { LogErr("Kill explorer pid " + p.Id, ex); }
            }
            Log("Explorer processes killed: " + count);
            if (count == 0)
            {
                try
                {
                    using (Process tk = Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "taskkill.exe"), "/f /im explorer.exe")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }))
                    {
                        if (tk != null) tk.WaitForExit(5000);
                    }
                }
                catch (Exception ex) { LogErr("taskkill", ex); }
            }
            Thread.Sleep(2000);
            try
            {
                if (Process.GetProcessesByName("explorer").Length == 0)
                {
                    Process.Start(new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"))
                    {
                        UseShellExecute = true
                    });
                    Log("Explorer restarted explicitly");
                }
            }
            catch (Exception ex) { LogErr("Start explorer fallback", ex); }
        }

        public static void OpenExplorerAt(string path)
        {
            try
            {
                string expPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                string args = string.Empty;
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.Length > 3 && path.EndsWith("\\"))
                    {
                        path = path.TrimEnd('\\');
                    }
                    args = (path.Length != 3) ? ("\"" + path + "\"") : path;
                }
                Process.Start(new ProcessStartInfo(expPath, args) { UseShellExecute = false });
            }
            catch (Exception ex)
            {
                LogErr("Opening explorer window", ex);
            }
        }

        public static void RefreshExplorer(bool restartShell)
        {
            List<string> openFolders = CaptureExplorerFolders();
            Log("Open explorer windows: " + openFolders.Count);
            CloseExplorerWindows();
            Thread.Sleep(600);
            if (restartShell)
            {
                Log("Restarting Explorer");
                RestartExplorer();
            }
            foreach (string path in openFolders)
            {
                OpenExplorerAt(path);
                Thread.Sleep(150);
            }
            Log("Explorer windows reopened");
        }
    }
}
