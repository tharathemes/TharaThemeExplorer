using System;
using Microsoft.Win32;
using TharaThemeExplorer.UI;

namespace TharaThemeExplorer.Core
{
    internal class RegOpt
    {
        public string Text { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public int OnVal { get; set; }
        public int OffVal { get; set; }
        public int Def { get; set; }
        public bool NeedsRefresh { get; set; }
        public bool OffNeedsRefresh { get; set; }
        public bool DefaultOn { get; set; }
        public bool Initial { get; set; }
        public ToggleSwitch Box { get; set; }

        public Func<bool> ReadFn { get; set; }
        public Action<bool> WriteFn { get; set; }

        public RegOpt(string text, string key, string name, int onVal, int offVal, int def, bool restartNeeded)
        {
            Text = text;
            Key = key;
            Name = name;
            OnVal = onVal;
            OffVal = offVal;
            Def = def;
            NeedsRefresh = restartNeeded;
            DefaultOn = (def == onVal);
        }

        public static RegOpt Custom(string text, Func<bool> read, Action<bool> write, bool restartNeeded)
        {
            return new RegOpt(text, null, null, 1, 0, 0, restartNeeded)
            {
                ReadFn = read,
                WriteFn = write
            };
        }

        public bool Read()
        {
            if (ReadFn != null) return ReadFn();
            try
            {
                object val = Registry.GetValue(Key, Name, null);
                if (val is int)
                {
                    return (int)val == OnVal;
                }
                return DefaultOn;
            }
            catch
            {
                return DefaultOn;
            }
        }

        public void Write(bool on)
        {
            if (WriteFn != null)
            {
                WriteFn(on);
                return;
            }
            try
            {
                if (on)
                {
                    Registry.SetValue(Key, Name, OnVal, RegistryValueKind.DWord);
                }
                else
                {
                    if (OffVal == -1)
                    {
                        // Delete value
                        if (Key.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
                        {
                            string sub = Key.Substring("HKEY_CURRENT_USER\\".Length);
                            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(sub, true))
                            {
                                if (rk != null) rk.DeleteValue(Name, false);
                            }
                        }
                    }
                    else
                    {
                        Registry.SetValue(Key, Name, OffVal, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                ShellHelper.LogErr("Write RegOpt: " + Text, ex);
            }
        }
    }

    internal class ChoiceOpt
    {
        public string Text { get; set; }
        public string[] Items { get; set; }
        public Func<int> ReadFn { get; set; }
        public Action<int> WriteFn { get; set; }
        public bool NeedsRefresh { get; set; }
        public Program.Segmented Seg { get; set; }
        public int Initial { get; set; }
    }

    internal class NavOpt
    {
        public string Clsid { get; set; }
        public ToggleSwitch Box { get; set; }
        public bool Initial { get; set; }
    }
}
