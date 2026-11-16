using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly string[] TakeOwnershipCommands = new string[]
        {
            "cmd.exe /c takeown /f \"%1\" && icacls \"%1\" /grant *S-1-5-32-544:F",
            "cmd.exe /c takeown /f \"%1\" /r /d y && icacls \"%1\" /grant *S-1-5-32-544:F /t"
        };

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
        private const string FolderDescriptions = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FolderDescriptions\";
        private static readonly string[] Win10ThisPcFolders = new string[]
        {
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
            "{f42ee2d3-909f-4907-8871-4c22fc0bf756}", // Documents
            "{7d83ee9b-2244-4e70-b1f5-5393042af1e4}", // Downloads
            "{a0c69a99-21c8-4671-8703-7934162fcf1d}", // Music
            "{0ddd015d-b06c-45d5-8c4c-f59713854639}", // Pictures
            "{35286a68-3c57-41a1-bbb1-0eae73d76c95}", // Videos
            "{31c0dd25-9439-4f12-bf41-7ff4eda38722}"  // 3D Objects (when present)
        };

        private static RegistryView[] FolderRegistryViews()
        {
            return Environment.Is64BitOperatingSystem
                ? new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 }
                : new RegistryView[] { RegistryView.Registry32 };
        }

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

        private static void WriteDwordStrict(string key, string name, int value)
        {
            Registry.SetValue(key, name, value, RegistryValueKind.DWord);
        }

        private static void DeleteDwordStrict(string key, string name)
        {
            if (!key.StartsWith(HKCUP, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This setting is not in the current-user registry.");
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(key.Substring(HKCUP.Length), true))
                if (rk != null) rk.DeleteValue(name, false);
        }

        public static void ReadStyleState(out bool win10Explorer, out bool win10ContextMenu)
        {
            win10Explorer = EmptyInprocOverrideExists(E10A) || EmptyInprocOverrideExists(E10B);
            win10ContextMenu = EmptyInprocOverrideExists(CCMA);
        }

        private static bool EmptyInprocOverrideExists(string classPath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(classPath + @"\InProcServer32"))
            {
                return key != null && (key.GetValue(string.Empty) as string) == string.Empty;
            }
        }

        private static void EnsureInprocOverrideAvailable(string classPath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(classPath + @"\InProcServer32"))
            {
                object value = key == null ? null : key.GetValue(string.Empty);
                if (value != null && (!(value is string) || (string)value != string.Empty))
                    throw new InvalidOperationException("Another application has an Explorer override here. Its setting was preserved.");
            }
        }

        private static void RemoveEmptyInprocOverride(string classPath)
        {
            bool removeClass = false;
            using (RegistryKey cls = Registry.CurrentUser.OpenSubKey(classPath, true))
            {
                if (cls == null) return;
                bool removeServer = false;
                bool changed = false;
                using (RegistryKey server = cls.OpenSubKey("InProcServer32", true))
                {
                    if (server != null && (server.GetValue(string.Empty) as string) == string.Empty)
                    {
                        server.DeleteValue(string.Empty, false);
                        changed = true;
                        removeServer = server.ValueCount == 0 && server.SubKeyCount == 0;
                    }
                }
                if (removeServer) cls.DeleteSubKey("InProcServer32", false);
                removeClass = changed && cls.ValueCount == 0 && cls.SubKeyCount == 0;
            }
            if (removeClass) Registry.CurrentUser.DeleteSubKey(classPath, false);
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
                    EnsureInprocOverrideAvailable(RIBBONCLSID);
                    Registry.SetValue(HKCUP + RIBBONCLSID + @"\InProcServer32", string.Empty, string.Empty, RegistryValueKind.String);
                }
                else if (RibbonOverrideActive())
                {
                    RemoveEmptyInprocOverride(RIBBONCLSID);
                }
            }
            catch { throw; }
        }

        public static bool TakeOwnershipOn()
        {
            try
            {
                return IsTakeOwnershipEntry(TOKEYS[0], TakeOwnershipCommands[0]) &&
                       IsTakeOwnershipEntry(TOKEYS[1], TakeOwnershipCommands[1]);
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
                // Check both locations before writing either one.
                for (int i = 0; i < TOKEYS.Length; i++)
                {
                    using (RegistryKey existing = Registry.CurrentUser.OpenSubKey(TOKEYS[i]))
                        if (existing != null && (existing.ValueCount > 0 || existing.SubKeyCount > 0) &&
                            !IsTakeOwnershipEntry(TOKEYS[i], TakeOwnershipCommands[i]))
                            throw new InvalidOperationException("A different Take Ownership context-menu command already exists and was preserved.");
                }
                for (int i = 0; i < TOKEYS.Length; i++) WriteTakeOwnership(TOKEYS[i], TakeOwnershipCommands[i]);
            }
            else
            {
                for (int i = 0; i < TOKEYS.Length; i++) RemoveTakeOwnership(TOKEYS[i], TakeOwnershipCommands[i]);
            }
        }

        private static bool IsTakeOwnershipEntry(string sub, string expectedCommand)
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(sub))
            using (RegistryKey cmd = rk == null ? null : rk.OpenSubKey("command"))
                return rk != null && cmd != null && (rk.GetValue(string.Empty) as string) == TOLABEL &&
                       (cmd.GetValue(string.Empty) as string) == expectedCommand;
        }

        private static void RemoveTakeOwnership(string sub, string expectedCommand)
        {
            if (!IsTakeOwnershipEntry(sub, expectedCommand)) return;
            bool removeParent = false;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(sub, true))
            {
                if (rk == null) return;
                bool removeCommand = false;
                using (RegistryKey cmd = rk.OpenSubKey("command", true))
                {
                    if (cmd == null || (cmd.GetValue(string.Empty) as string) != expectedCommand) return;
                    cmd.DeleteValue(string.Empty, false);
                    removeCommand = cmd.ValueCount == 0 && cmd.SubKeyCount == 0;
                }
                if (removeCommand) rk.DeleteSubKey("command", false);
                rk.DeleteValue(string.Empty, false);
                if ((rk.GetValue("HasLUAShield") as string) == string.Empty) rk.DeleteValue("HasLUAShield", false);
                if ((rk.GetValue("NoWorkingDirectory") as string) == string.Empty) rk.DeleteValue("NoWorkingDirectory", false);
                removeParent = rk.ValueCount == 0 && rk.SubKeyCount == 0;
            }
            if (removeParent) Registry.CurrentUser.DeleteSubKey(sub, false);
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
                if (!IsWin11)
                {
                    // Explorer uses the native registry view. A partially hidden group is
                    // still visible, so leave the switch on until all managed folders
                    // are hidden; this also lets the user retry a partial change.
                    RegistryView view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                    using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    {
                        for (int i = 0; i < Win10ThisPcFolders.Length; i++)
                        {
                            if (i == 6)
                            {
                                // 3D Objects is absent on some systems, even when its
                                // folder description remains registered.
                                using (RegistryKey ns = root.OpenSubKey(PCKEY + "{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}"))
                                    if (ns == null) continue;
                            }
                            using (RegistryKey folder = root.OpenSubKey(FolderDescriptions + Win10ThisPcFolders[i]))
                            {
                                if (folder == null) continue;
                                using (RegistryKey bag = folder.OpenSubKey("PropertyBag"))
                                {
                                    string policy = bag == null ? null : bag.GetValue("ThisPCPolicy") as string;
                                    if (!string.Equals(policy, "Hide", StringComparison.OrdinalIgnoreCase)) return true;
                                }
                            }
                        }
                    }
                    return false;
                }
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
            if (!IsWin11)
            {
                RunThisPcElevated(on);
                return;
            }
            RunThisPcElevated(on);
            string sortKey = HKCUP + @"Software\Classes\CLSID\" + PCGUIDS[0] + @"\ShellFolder";
            if (on)
            {
                WriteDwordStrict(sortKey, "SortOrderIndex", 0);
            }
            else
            {
                DeleteDwordStrict(sortKey, "SortOrderIndex");
            }

        }

        private static void RunThisPcElevated(bool on)
        {
            Process started;
            try
            {
                started = Process.Start(new ProcessStartInfo(MyExe, "/thispc " + (on ? "on" : "off"))
                {
                    Verb = "runas",
                    UseShellExecute = true
                });
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223)
                    throw new OperationCanceledException("Administrator approval was cancelled.", ex);
                throw;
            }
            using (Process proc = started)
            {
                if (proc == null) throw new Exception("Administrator helper could not be started.");
                if (!proc.WaitForExit(30000))
                    throw new TimeoutException("The administrator helper did not finish within 30 seconds. It may still be running.");
                if (proc.ExitCode != 0)
                {
                    string detail = proc.ExitCode == 2 ? "Windows refused one or more registry changes." :
                        proc.ExitCode == 3 ? "Administrator permission is required." :
                        proc.ExitCode == 4 ? "The requested folder option was invalid." :
                        proc.ExitCode == 5 ? "The folder registry entries were not found." :
                        "Administrator helper failed with code " + proc.ExitCode + ".";
                    throw new InvalidOperationException(detail);
                }
            }
        }

        public static int RunThisPcHelper(string mode)
        {
            if (mode != "on" && mode != "off") return 4;
            if (!IsAdmin()) return 3;
            try
            {
                if (!IsWin11)
                {
                    int updated = 0;
                    foreach (RegistryView view in FolderRegistryViews())
                    using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    foreach (string guid in Win10ThisPcFolders)
                    using (RegistryKey folder = root.OpenSubKey(FolderDescriptions + guid, true))
                    {
                        if (folder == null) continue;
                        using (RegistryKey bag = folder.CreateSubKey("PropertyBag"))
                        {
                            if (bag == null) throw new InvalidOperationException("Could not open folder policy.");
                            bag.SetValue("ThisPCPolicy", mode == "on" ? "Show" : "Hide", RegistryValueKind.String);
                            updated++;
                        }
                    }
                    if (mode == "on")
                    {
                        // Restore the exact namespace flags written by older versions
                        // of this app on Windows 10, without changing other flags.
                        foreach (RegistryView view in FolderRegistryViews())
                        using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                        foreach (string guid in PCGUIDS)
                        using (RegistryKey key = root.OpenSubKey(PCKEY + guid, true))
                        {
                            if (key == null) continue;
                            object feature = key.GetValue("HideIfEnabled");
                            object hidden = key.GetValue("HiddenByDefault");
                            if (feature is int && (int)feature == 36354489 && hidden is int && (int)hidden == 1)
                            {
                                key.DeleteValue("HideIfEnabled", false);
                                key.SetValue("HiddenByDefault", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                    return updated == 0 ? 5 : 0;
                }
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
                        if (rk == null) throw new InvalidOperationException("Could not open the navigation setting.");
                        rk.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
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
            catch { throw; }
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
                    if (v == 1) WriteDwordStrict(key, "SortOrderIndex", bottomVal);
                    else DeleteDwordStrict(key, "SortOrderIndex");
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

                    // Do not remove the older CLSID: other software may own values there.
                }
                catch { throw; }
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

        internal static string MicaErrorStatus(bool selected, bool saved, int build, object result)
        {
            if (selected != saved) return null;
            if (saved && build < 22621) return "Status: Not supported on this build";
            if (result is int && (int)result != 0)
                return "Status: Last Mica operation failed (0x" + ((int)result).ToString("X8") + ")";
            return null;
        }

        public static RegOpt ThisPcOpt()
        {
            RegOpt opt = RegOpt.Custom("Show the Folders group", ThisPcFoldersShown, SetThisPcFolders, true);
            opt.DefaultOn = !IsWin11;
            opt.RequiresExplorerRestart = true;
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
                    EnsureInprocOverrideAvailable(CCMA);
                    Registry.SetValue(HKCUP + CCMB, string.Empty, string.Empty, RegistryValueKind.String);
                    Log("Context menu: set to Windows 10");
                }
                catch (Exception ex) { LogErr("Context menu -> Windows 10", ex); }
            }
            if (c11 && win10Ctx)
            {
                try
                {
                    RemoveEmptyInprocOverride(CCMA);
                    Log("Context menu: set to Windows 11");
                }
                catch (Exception ex) { LogErr("Context menu -> Windows 11", ex); }
            }

            if (e10 && !win10Exp)
            {
                try
                {
                    EnsureInprocOverrideAvailable(E10A);
                    EnsureInprocOverrideAvailable(E10B);
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
                    RemoveEmptyInprocOverride(E10A);
                    RemoveEmptyInprocOverride(E10B);
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

        internal static string ExplorerRestoreLocation(string shellPath, string locationUrl)
        {
            // Folder.Self.Path handles both ordinary paths and shell namespaces.
            // Ignore arbitrary URLs or malformed values when launching Explorer.
            if (!string.IsNullOrEmpty(shellPath) && shellPath.IndexOfAny(new char[] { '"', '\0', '\r', '\n' }) < 0)
            {
                if (shellPath.StartsWith(@"\\", StringComparison.Ordinal) ||
                    (shellPath.Length >= 3 && char.IsLetter(shellPath[0]) && shellPath[1] == ':' && shellPath[2] == '\\') ||
                    shellPath.StartsWith("::{", StringComparison.Ordinal) ||
                    shellPath.StartsWith("shell:", StringComparison.OrdinalIgnoreCase))
                    return shellPath;
            }
            if (!string.IsNullOrEmpty(locationUrl))
            {
                Uri uri;
                if (Uri.TryCreate(locationUrl, UriKind.Absolute, out uri) && uri.IsFile)
                {
                    string local = uri.LocalPath;
                    if (local.IndexOfAny(new char[] { '"', '\0', '\r', '\n' }) < 0)
                        return local;
                }
            }
            return null;
        }

        private static object ComProperty(object obj, string name)
        {
            return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, null);
        }

        internal sealed class ExplorerWindowSnapshot
        {
            public IntPtr Handle;
            public string Location;
        }

        internal static List<ExplorerWindowSnapshot> CaptureExplorerSnapshots(object windows, Func<IntPtr, bool> isExplorer)
        {
            List<ExplorerWindowSnapshot> list = new List<ExplorerWindowSnapshot>();
            int count = Convert.ToInt32(ComProperty(windows, "Count"));
            for (int i = 0; i < count; i++)
            {
                try
                {
                    object item = windows.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, windows, new object[] { i });
                    if (item == null) continue;
                    long hwnd = Convert.ToInt64(ComProperty(item, "HWND"));
                    if (!isExplorer((IntPtr)hwnd)) continue;
                    string path = null;
                    string url = null;
                    try
                    {
                        object document = ComProperty(item, "Document");
                        object folder = ComProperty(document, "Folder");
                        object self = ComProperty(folder, "Self");
                        path = Convert.ToString(ComProperty(self, "Path"));
                    }
                    catch { }
                    try { url = Convert.ToString(ComProperty(item, "LocationURL")); }
                    catch { }
                    string location = ExplorerRestoreLocation(path, url);
                    if (location == null) Log("Could not record one Explorer location; it will reopen at the default location.");
                    list.Add(new ExplorerWindowSnapshot { Handle = (IntPtr)hwnd, Location = location });
                }
                catch { /* A window may have closed during enumeration. */ }
            }
            return list;
        }

        internal static List<string> CaptureExplorerWindows(object windows, Func<IntPtr, bool> isExplorer)
        {
            List<string> locations = new List<string>();
            foreach (ExplorerWindowSnapshot window in CaptureExplorerSnapshots(windows, isExplorer))
                locations.Add(window.Location);
            return locations;
        }

        private static List<ExplorerWindowSnapshot> CaptureExplorerWindowsForRefresh()
        {
            List<ExplorerWindowSnapshot> list = new List<ExplorerWindowSnapshot>();
            try
            {
                Type shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType == null) throw new InvalidOperationException("Windows Shell is unavailable.");
                object shell = Activator.CreateInstance(shellType);
                object windows = shellType.InvokeMember("Windows", BindingFlags.InvokeMethod, null, shell, null);
                list = CaptureExplorerSnapshots(windows, IsExplorerFrame);
            }
            catch (Exception ex)
            {
                // Do not close open windows when their locations could not be saved.
                if (NativeMethods.FindWindow("CabinetWClass", null) != IntPtr.Zero)
                    throw new InvalidOperationException("Could not save open Explorer locations. Close Explorer windows and try again.", ex);
            }
            if (list.Count == 0 && NativeMethods.FindWindow("CabinetWClass", null) != IntPtr.Zero)
                throw new InvalidOperationException("Could not save open Explorer locations. Close Explorer windows and try again.");
            return list;
        }

        public static List<string> CaptureExplorerFolders()
        {
            List<string> locations = new List<string>();
            foreach (ExplorerWindowSnapshot window in CaptureExplorerWindowsForRefresh())
                locations.Add(window.Location);
            return locations;
        }

        internal static void CloseAndReopenWindows(List<ExplorerWindowSnapshot> windows,
            Func<IntPtr, bool> exists, Func<IntPtr, bool> close, Action wait,
            Action notify, Action<string> reopen)
        {
            HashSet<IntPtr> pending = new HashSet<IntPtr>();
            HashSet<IntPtr> closed = new HashSet<IntPtr>();
            HashSet<IntPtr> attempted = new HashSet<IntPtr>();
            bool closeFailed = false;

            foreach (ExplorerWindowSnapshot window in windows)
            {
                if (!attempted.Add(window.Handle) || !exists(window.Handle)) continue;
                try
                {
                    if (close(window.Handle)) pending.Add(window.Handle);
                    else closeFailed = true;
                }
                catch { closeFailed = true; }
            }

            // Wait briefly for asynchronous WM_CLOSE messages to be processed.
            for (int attempt = 0; attempt < 50 && pending.Count > 0; attempt++)
            {
                List<IntPtr> finished = new List<IntPtr>();
                foreach (IntPtr handle in pending)
                    if (!exists(handle)) finished.Add(handle);
                foreach (IntPtr handle in finished) { pending.Remove(handle); closed.Add(handle); }
                if (pending.Count > 0) wait();
            }
            foreach (IntPtr handle in pending)
            {
                if (!exists(handle)) closed.Add(handle);
                else closeFailed = true;
            }

            // A notification failure must not prevent already-closed windows reopening.
            try { notify(); }
            catch (Exception ex) { Log("Explorer notification failed: " + ex.Message); }
            foreach (ExplorerWindowSnapshot window in windows)
                if (closed.Contains(window.Handle)) reopen(window.Location);

            if (closeFailed)
                throw new InvalidOperationException("Some Explorer windows could not close. Their original windows were left open.");
        }

        public static void RefreshOpenExplorerWindows()
        {
            List<ExplorerWindowSnapshot> windows = CaptureExplorerWindowsForRefresh();
            HashSet<IntPtr> capturedHandles = new HashSet<IntPtr>();
            foreach (ExplorerWindowSnapshot window in windows) capturedHandles.Add(window.Handle);
            bool uncapturedWindow = false;
            NativeMethods.EnumWindows(delegate(IntPtr handle, IntPtr unused)
            {
                if (NativeMethods.IsWindowVisible(handle) && IsExplorerFrame(handle) && !capturedHandles.Contains(handle))
                    uncapturedWindow = true;
                return true;
            }, IntPtr.Zero);
            CloseAndReopenWindows(windows,
                handle => NativeMethods.IsWindow(handle),
                handle => NativeMethods.PostMessage(handle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero),
                () => Thread.Sleep(100),
                () =>
                {
                    NativeMethods.PostMessage((IntPtr)0xffff, NativeMethods.WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero);
                    NativeMethods.SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
                },
                path => { OpenExplorerAt(path); Thread.Sleep(150); });
            Log("Explorer windows closed and reopened: " + windows.Count);
            if (uncapturedWindow)
                throw new InvalidOperationException("An Explorer window could not be saved and was left open. Please reopen it manually.");
        }

        public static bool IsExplorerFrame(IntPtr h)
        {
            StringBuilder sb = new StringBuilder(64);
            return NativeMethods.GetClassName(h, sb, 64) > 0 && sb.ToString() == "CabinetWClass";
        }

        public static void RestartExplorer()
        {
            int sessionId = Process.GetCurrentProcess().SessionId;
            int count = 0;
            bool stopFailed = false;
            foreach (Process p in Process.GetProcessesByName("explorer"))
            {
                using (p)
                {
                    try
                    {
                        if (p.SessionId == sessionId)
                        {
                            p.Kill();
                            if (!p.WaitForExit(5000)) throw new TimeoutException("The process did not exit.");
                            count++;
                        }
                    }
                    catch (Exception ex) { stopFailed = true; LogErr("Kill explorer pid " + p.Id, ex); }
                }
            }
            Log("Explorer processes killed: " + count);
            Thread.Sleep(1200);
            try
            {
                if (!ExplorerRunningInSession(sessionId))
                {
                    Process.Start(new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"))
                    {
                        UseShellExecute = true
                    });
                    Log("Explorer restarted explicitly");
                }
                for (int attempt = 0; attempt < 20 && !ExplorerRunningInSession(sessionId); attempt++) Thread.Sleep(250);
                if (!ExplorerRunningInSession(sessionId)) throw new InvalidOperationException("Windows Explorer did not start in this session.");
            }
            catch (Exception ex) { throw new InvalidOperationException("Could not restart Windows Explorer.", ex); }
            if (stopFailed) throw new InvalidOperationException("Some Explorer processes could not be restarted.");
        }

        private static bool ExplorerRunningInSession(int sessionId)
        {
            bool found = false;
            foreach (Process process in Process.GetProcessesByName("explorer"))
            {
                using (process)
                {
                    try { if (process.SessionId == sessionId) found = true; }
                    catch { }
                }
            }
            return found;
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
            if (!restartShell)
            {
                int refreshLogStart = RunLog.Count;
                RefreshOpenExplorerWindows();
                for (int i = refreshLogStart; i < RunLog.Count; i++)
                    if (RunLog[i].Contains("FAILED")) throw new InvalidOperationException("Some Explorer windows could not be reopened. See the log for details.");
                return;
            }
            List<string> openFolders = CaptureExplorerFolders();
            int logStart = RunLog.Count;
            Log("Open explorer windows: " + openFolders.Count);
            Log("Restarting Explorer");
            Exception restartError = null;
            try { RestartExplorer(); }
            catch (Exception ex) { restartError = ex; }
            foreach (string path in openFolders)
            {
                OpenExplorerAt(path);
                Thread.Sleep(150);
            }
            Log("Explorer windows reopened");
            if (restartError != null) throw new InvalidOperationException("Explorer did not restart completely. Some windows may have reopened.", restartError);
            for (int i = logStart; i < RunLog.Count; i++)
                if (RunLog[i].Contains("FAILED")) throw new InvalidOperationException("Some Explorer windows could not be restored. See the log for details.");
        }
    }
}
