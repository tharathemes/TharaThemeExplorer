using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TharaThemeExplorer.Core
{
    internal static class Watcher
    {
        private const string WatcherMutex = "Local\\TharaThemeExplorer.Watcher";
        private static bool fMica;
        private static bool fText;
        private static bool fIcon;
        private static bool micaSupported;
        private static bool micaSeen;
        private static bool themeSeen;
        private static int micaResult;
        private static int themeResult;
        private static readonly Dictionary<IntPtr, string> capState = new Dictionary<IntPtr, string>();
        private static readonly HashSet<IntPtr> micaState = new HashSet<IntPtr>();

        private static readonly NativeMethods.EnumWindowsProc watchProc = WatchEnum;
        private static readonly NativeMethods.WinEventProc watchEventProc = WatchEvent;

        public static bool WatcherRunEnabled()
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(ShellHelper.RUNKEY))
                {
                    return rk != null && rk.GetValue(ShellHelper.WATCHVAL) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetWatcherRun(bool on)
        {
            try
            {
                using (RegistryKey rk = on ? Registry.CurrentUser.CreateSubKey(ShellHelper.RUNKEY) : Registry.CurrentUser.OpenSubKey(ShellHelper.RUNKEY, true))
                {
                    if (rk != null)
                    {
                        rk.DeleteValue("TharaExplorerMica", false);
                        rk.DeleteValue("TharaExplorerWatcher", false);
                        rk.DeleteValue("TharaThemeUIWatcher", false);
                        if (on)
                        {
                            rk.SetValue(ShellHelper.WATCHVAL, "\"" + ShellHelper.MyExe + "\" /watch", RegistryValueKind.String);
                        }
                        else
                        {
                            rk.DeleteValue(ShellHelper.WATCHVAL, false);
                        }
                    }
                }
            }
            catch (Exception ex) { ShellHelper.LogErr("Updating background watcher", ex); }
        }

        private static bool WatcherRunning()
        {
            try
            {
                using (Mutex existing = Mutex.OpenExisting(WatcherMutex)) { }
                return true;
            }
            catch (WaitHandleCannotBeOpenedException) { return false; }
            catch (UnauthorizedAccessException) { return true; }
        }

        public static void EnsureCurrentWatcher()
        {
            if (!WatcherRunEnabled()) return;
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string active = Registry.GetValue(ShellHelper.MyKey, "WatcherVersion", null) as string;
                if (WatcherRunning() && active == version) return;
                if (WatcherRunning())
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ShellHelper.RUNKEY, true))
                    {
                        if (key == null) return;
                        object previous = key.GetValue(ShellHelper.WATCHVAL);
                        key.DeleteValue(ShellHelper.WATCHVAL, false);
                        for (int i = 0; i < 30 && WatcherRunning(); i++) Thread.Sleep(100);
                        if (WatcherRunning())
                        {
                            if (previous != null) key.SetValue(ShellHelper.WATCHVAL, previous, RegistryValueKind.String);
                            return;
                        }
                    }
                }
                SetWatcherRun(true);
                StartWatcher();
            }
            catch (Exception ex) { ShellHelper.LogErr("Updating background watcher", ex); }
        }

        public static void StartWatcher()
        {
            try
            {
                // Avoid starting a second process just to discover the mutex is held.
                if (WatcherRunning()) return;
                Process.Start(new ProcessStartInfo(ShellHelper.MyExe, "/watch")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex) { ShellHelper.LogErr("Starting background watcher", ex); }
        }

        private static void RefreshFlags()
        {
            fMica = micaSupported && ShellHelper.ReadDword(ShellHelper.MyKey, "DisableMica", 0) == 1;
            fText = ShellHelper.ReadDword(ShellHelper.MyKey, "HideCaptionText", 0) == 1;
            fIcon = ShellHelper.ReadDword(ShellHelper.MyKey, "HideCaptionIcon", 0) == 1;
        }

        private static void ApplyToFrame(IntPtr h)
        {
            if (fMica)
            {
                int val1 = 1;
                int r1 = NativeMethods.DwmSetWindowAttribute(h, 38, ref val1, 4);
                if (r1 == 0) micaState.Add(h);
                micaSeen = true;
                if (r1 != 0) micaResult = r1;
            }
            else if (micaState.Contains(h))
            {
                // Restore the system-selected backdrop when the option is disabled.
                int automatic = 0;
                int result = NativeMethods.DwmSetWindowAttribute(h, 38, ref automatic, 4);
                micaSeen = true;
                if (result != 0) micaResult = result;
                else micaState.Remove(h);
            }
            string state;
            bool hasState = capState.TryGetValue(h, out state);
            if (fText || fIcon || hasState)
            {
                string targetState = (fText ? "t" : "-") + (fIcon ? "i" : "-");
                if (!hasState || state != targetState)
                {
                    NativeMethods.WTA_OPTIONS opts = new NativeMethods.WTA_OPTIONS
                    {
                        dwFlags = (fText ? 1u : 0u) | (fIcon ? 2u : 0u),
                        dwMask = 3u
                    };
                    int r = NativeMethods.SetWindowThemeAttribute(h, 1, ref opts, 8u);
                    themeSeen = true;
                    if (r != 0) themeResult = r;
                    capState[h] = targetState;
                }
            }
        }

        private static bool WatchEnum(IntPtr h, IntPtr l)
        {
            if (NativeMethods.IsWindowVisible(h) && ShellHelper.IsExplorerFrame(h))
            {
                ApplyToFrame(h);
            }
            return true;
        }

        private static void WatchEvent(IntPtr hHook, uint ev, IntPtr hwnd, int idObject, int idChild, uint thread, uint time)
        {
            try
            {
                if (idObject == 0 && idChild == 0 && hwnd != IntPtr.Zero && ShellHelper.IsExplorerFrame(hwnd))
                {
                    ApplyToFrame(hwnd);
                }
            }
            catch { }
        }

        public static void RunWatcher()
        {
            bool created;
            using (new Mutex(true, WatcherMutex, out created))
            {
                if (!created) return;

                try { Registry.SetValue(ShellHelper.MyKey, "WatcherVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString(), RegistryValueKind.String); }
                catch { }
                micaSupported = ShellHelper.WinBuild >= 22621;
                RefreshFlags();

                IntPtr h1 = NativeMethods.SetWinEventHook(0x8000, 0x8000, IntPtr.Zero, watchEventProc, 0, 0, 0);
                IntPtr h2 = NativeMethods.SetWinEventHook(0x8002, 0x8002, IntPtr.Zero, watchEventProc, 0, 0, 0);
                IntPtr h3 = NativeMethods.SetWinEventHook(3, 3, IntPtr.Zero, watchEventProc, 0, 0, 0);

                ApplicationContext ctx = new ApplicationContext();
                int wMica = int.MinValue;
                int wTheme = int.MinValue;

                // Event hooks handle new windows immediately; the timer is a slower
                // fallback and also notices changed settings.
                global::System.Windows.Forms.Timer timer = new global::System.Windows.Forms.Timer { Interval = 1200 };
                timer.Tick += (s, e) =>
                {
                    if (!WatcherRunEnabled())
                    {
                        // The UI can turn all watcher features off at once. Undo the
                        // changes in existing windows before stopping the watcher.
                        fMica = fText = fIcon = false;
                        HashSet<IntPtr> affected = new HashSet<IntPtr>(capState.Keys);
                        affected.UnionWith(micaState);
                        foreach (IntPtr hwnd in affected)
                            if (NativeMethods.IsWindow(hwnd))
                                try { ApplyToFrame(hwnd); } catch { }
                        ctx.ExitThread();
                        return;
                    }
                    RefreshFlags();
                    List<IntPtr> dead = new List<IntPtr>();
                    foreach (IntPtr hwnd in capState.Keys)
                    {
                        if (!NativeMethods.IsWindow(hwnd)) dead.Add(hwnd);
                    }
                    foreach (IntPtr hwnd in dead) capState.Remove(hwnd);
                    micaState.RemoveWhere(hwnd => !NativeMethods.IsWindow(hwnd));

                    micaSeen = false;
                    micaResult = 0;
                    themeSeen = false;
                    themeResult = 0;

                    try
                    {
                        NativeMethods.EnumWindows(watchProc, IntPtr.Zero);
                    }
                    catch { }

                    bool micaResultMissing = false;
                    if (micaSeen && micaResult == wMica)
                    {
                        try { micaResultMissing = !(Registry.GetValue(ShellHelper.MyKey, "MicaLastResult", null) is int); }
                        catch { }
                    }
                    if (micaSeen && (micaResult != wMica || micaResultMissing))
                    {
                        ShellHelper.WriteDword(ShellHelper.MyKey, "MicaLastResult", micaResult);
                        wMica = micaResult;
                    }
                    if (themeSeen && themeResult != wTheme)
                    {
                        ShellHelper.WriteDword(ShellHelper.MyKey, "ThemeLastResult", themeResult);
                        wTheme = themeResult;
                    }
                };

                try { NativeMethods.EnumWindows(watchProc, IntPtr.Zero); } catch { }
                timer.Start();
                Application.Run(ctx);
                timer.Stop();

                if (h1 != IntPtr.Zero) NativeMethods.UnhookWinEvent(h1);
                if (h2 != IntPtr.Zero) NativeMethods.UnhookWinEvent(h2);
                if (h3 != IntPtr.Zero) NativeMethods.UnhookWinEvent(h3);
            }
        }
    }
}
