using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer
{
    public partial class Program
    {
        private static Mutex instanceMutex;

        public static bool IsWin11 = ShellHelper.IsWin11;

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 2 && args[0].Equals("/thispc", StringComparison.OrdinalIgnoreCase))
            {
                Environment.ExitCode = ShellHelper.RunThisPcHelper(args[1].ToLowerInvariant());
                return;
            }

            foreach (string arg in args)
            {
                if (arg.Equals("/watch", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/mica", StringComparison.OrdinalIgnoreCase))
                {
                    Watcher.RunWatcher();
                    return;
                }
            }

            bool isNewInstance;
            instanceMutex = new Mutex(true, "Local\\TharaThemeExplorer.SingleInstance", out isNewInstance);
            if (!isNewInstance)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show("An unexpected error occurred. No further changes were made.", ShellHelper.MyName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            if (ShellHelper.WinBuild < 17763)
            {
                MessageBox.Show("Windows 10 (version 1809) or Windows 11 is required.", ShellHelper.MyName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (args.Length == 0)
            {
                MigrateOldSettings();
                Application.Run(new OptionsDialog());
                return;
            }

            if (!ShellHelper.IsWin11) return;

            bool win10Exp, win10Ctx;
            ShellHelper.ReadStyleState(out win10Exp, out win10Ctx);

            bool e10 = win10Exp;
            bool e11 = !win10Exp;
            bool c10 = win10Ctx;
            bool c11 = !win10Ctx;
            bool ok2Reload = true;
            bool restart = false;

            for (int i = 0; i < args.Length; i++)
            {
                string opt = args[i].ToLowerInvariant();
                if (opt == "/e10") e10 = true;
                if (opt == "/e11") e11 = true;
                if (opt == "/c10") c10 = true;
                if (opt == "/c11") c11 = true;
                if (opt == "/x") ok2Reload = false;
                if (opt == "/r") restart = true;
            }

            try
            {
                ShellHelper.SaveSettings(e10, e11, c10, c11, restart, ok2Reload);
            }
            catch { }
        }

        private static void MigrateOldSettings()
        {
            try
            {
                if (Registry.GetValue(ShellHelper.MyKey, "MigratedFromOldVersion", null) == null)
                {
                    bool needWatcher = false;
                    string[] oldKeys = new string[]
                    {
                        @"HKEY_CURRENT_USER\Software\TharaThemeUI",
                        @"HKEY_CURRENT_USER\Software\TharaExplorer"
                    };
                    string[] optNames = new string[] { "DisableMica", "HideCaptionText", "HideCaptionIcon" };

                    foreach (string key in oldKeys)
                    {
                        foreach (string opt in optNames)
                        {
                            if (!(Registry.GetValue(ShellHelper.MyKey, opt, null) is int))
                            {
                                object val = Registry.GetValue(key, opt, null);
                                if (val is int)
                                {
                                    ShellHelper.WriteDword(ShellHelper.MyKey, opt, (int)val);
                                    if ((int)val == 1) needWatcher = true;
                                }
                            }
                        }
                    }

                    ShellHelper.WriteDword(ShellHelper.MyKey, "MigratedFromOldVersion", 1);
                    if (needWatcher)
                    {
                        Watcher.SetWatcherRun(true);
                        Watcher.StartWatcher();
                    }
                }
            }
            catch { }
        }
    }
}
