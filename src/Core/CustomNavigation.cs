using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Win32;

namespace TharaThemeExplorer.Core
{
    internal static class CustomNavigation
    {
        private const string Owner = "TharaThemeExplorer.CustomNavigation.v1";
        private const string OwnerValue = "TharaThemeExplorerOwner";
        private const string MetadataRoot = @"Software\TharaThemeExplorer\CustomNavigation";
        private const string ClassRoot = @"Software\Classes\CLSID\";
        private const string ClassRoot32 = @"Software\Classes\Wow6432Node\CLSID\";
        private const string DesktopRoot = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\";
        private const string DesktopHidden = @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel";
        private const string FolderShortcutClsid = "{0E5AAE11-A475-4C5B-AB00-C66DE400274E}";
        private const string ProxyShortcut = "Open file.lnk";

        internal sealed class Item
        {
            public string Id;
            public string Path;
            public string Name;
            public bool IsFile;
            public bool InitialEnabled;
        }

        private static string CanonicalId(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid)) throw new InvalidOperationException("Invalid custom item identifier.");
            return guid.ToString("B").ToUpperInvariant();
        }

        public static Item FromPath(string value)
        {
            string input = (value ?? string.Empty).Trim().Trim('"');
            if (input.Length == 0) throw new ArgumentException("Enter a file or folder path.");
            input = Environment.ExpandEnvironmentVariables(input);
            if (!System.IO.Path.IsPathRooted(input)) throw new ArgumentException("Enter an absolute file or folder path.");
            string path = System.IO.Path.GetFullPath(input);
            bool folder = Directory.Exists(path);
            bool file = !folder && File.Exists(path);
            if (!folder && !file) throw new ArgumentException("The file or folder does not exist or cannot be accessed.");
            string trimmed = path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            string name = System.IO.Path.GetFileName(trimmed);
            if (string.IsNullOrEmpty(name)) name = path;
            return new Item { Id = Guid.NewGuid().ToString("B").ToUpperInvariant(), Path = path, Name = name, IsFile = file };
        }

        public static List<Item> Load()
        {
            List<Item> items = new List<Item>();
            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(MetadataRoot))
            {
                if (root == null) return items;
                foreach (string id in root.GetSubKeyNames())
                {
                    try
                    {
                        string canonical = CanonicalId(id);
                        using (RegistryKey key = root.OpenSubKey(id))
                        {
                            if (!Owned(key)) continue;
                            string path = key.GetValue("Path") as string;
                            string name = key.GetValue("Name") as string;
                            object kind = key.GetValue("IsFile");
                            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name) || !(kind is int)) continue;
                            Item item = new Item { Id = canonical, Path = path, Name = name, IsFile = (int)kind == 1 };
                            item.InitialEnabled = IsEnabled(item);
                            items.Add(item);
                        }
                    }
                    catch (ArgumentException) { }
                    catch (InvalidOperationException) { }
                }
            }
            items.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
            return items;
        }

        private static bool Owned(RegistryKey key)
        {
            return key != null && string.Equals(key.GetValue(OwnerValue) as string, Owner, StringComparison.Ordinal);
        }

        private static IEnumerable<string> ClassPaths(string id)
        {
            yield return ClassRoot + id;
            if (Environment.Is64BitOperatingSystem) yield return ClassRoot32 + id;
        }

        private static void AssertSlotAvailable(string path)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
                if (key != null && !Owned(key))
                    throw new InvalidOperationException("This navigation identifier is already used by another item.");
        }

        private static bool IsEnabled(Item item)
        {
            string id = CanonicalId(item.Id);
            using (RegistryKey ns = Registry.CurrentUser.OpenSubKey(DesktopRoot + id))
            using (RegistryKey cls = Registry.CurrentUser.OpenSubKey(ClassRoot + id))
            {
                object pinned = cls == null ? null : cls.GetValue("System.IsPinnedToNameSpaceTree");
                return Owned(ns) && Owned(cls) && pinned is int && (int)pinned == 1;
            }
        }

        private static string ProxyDirectory(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid)) throw new InvalidOperationException("Invalid custom item identifier.");
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TharaThemeExplorer", "NavigationItems", guid.ToString("N"));
        }

        internal static string MakeProxy(Item item, string proxyDirectory, Action<string, string> createShortcut)
        {
            Directory.CreateDirectory(proxyDirectory);
            string shortcut = System.IO.Path.Combine(proxyDirectory, ProxyShortcut);
            createShortcut(item.Path, shortcut);
            return proxyDirectory;
        }

        private static void CheckTarget(Item item)
        {
            bool exists = item.IsFile ? File.Exists(item.Path) : Directory.Exists(item.Path);
            if (!exists) throw new InvalidOperationException("The selected file or folder is missing or cannot be accessed: " + item.Path);
        }

        private static void Register(Item item, Action<string, string> createShortcut)
        {
            string id = CanonicalId(item.Id);
            CheckTarget(item);
            foreach (string path in ClassPaths(id)) AssertSlotAvailable(path);
            AssertSlotAvailable(DesktopRoot + id);
            bool registryStarted = false;
            try
            {
                string target = item.IsFile ? MakeProxy(item, ProxyDirectory(id), createShortcut) : item.Path;
                registryStarted = true;
                foreach (string path in ClassPaths(id))
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(path))
                    {
                        if (key == null) throw new InvalidOperationException("Cannot create the navigation item.");
                        key.SetValue(OwnerValue, Owner, RegistryValueKind.String);
                        key.SetValue(string.Empty, item.Name, RegistryValueKind.String);
                        key.SetValue("System.IsPinnedToNameSpaceTree", 1, RegistryValueKind.DWord);
                        key.SetValue("SortOrderIndex", 0x42, RegistryValueKind.DWord);
                        using (RegistryKey icon = key.CreateSubKey("DefaultIcon"))
                            icon.SetValue(string.Empty, item.IsFile ? @"%SystemRoot%\System32\shell32.dll,-1" : @"%SystemRoot%\System32\imageres.dll,-3", RegistryValueKind.ExpandString);
                        using (RegistryKey server = key.CreateSubKey("InProcServer32"))
                        {
                            server.SetValue(string.Empty, @"%SystemRoot%\System32\shell32.dll", RegistryValueKind.ExpandString);
                            server.SetValue("ThreadingModel", "Both", RegistryValueKind.String);
                        }
                        using (RegistryKey instance = key.CreateSubKey("Instance"))
                        {
                            instance.SetValue("CLSID", FolderShortcutClsid, RegistryValueKind.String);
                            using (RegistryKey bag = instance.CreateSubKey("InitPropertyBag"))
                            {
                                bag.SetValue("Attributes", 17, RegistryValueKind.DWord);
                                bag.SetValue("TargetFolderPath", target, RegistryValueKind.String);
                            }
                        }
                        using (RegistryKey shell = key.CreateSubKey("ShellFolder"))
                        {
                            shell.SetValue("Attributes", unchecked((int)0xF080004D), RegistryValueKind.DWord);
                            shell.SetValue("FolderValueFlags", 0x28, RegistryValueKind.DWord);
                        }
                    }
                }
                using (RegistryKey ns = Registry.CurrentUser.CreateSubKey(DesktopRoot + id))
                {
                    ns.SetValue(OwnerValue, Owner, RegistryValueKind.String);
                    ns.SetValue(string.Empty, item.Name, RegistryValueKind.String);
                }
                using (RegistryKey hidden = Registry.CurrentUser.CreateSubKey(DesktopHidden))
                    hidden.SetValue(id, 1, RegistryValueKind.DWord);
            }
            catch
            {
                if (registryStarted) { try { Unregister(item); } catch { } }
                RemoveProxy(item);
                throw;
            }
        }

        private static void RemoveOwnedKey(string path)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
            {
                if (key == null) return;
                if (!Owned(key)) throw new InvalidOperationException("A navigation entry is owned by another application and was not removed.");
            }
            Registry.CurrentUser.DeleteSubKeyTree(path, false);
        }

        private static void Unregister(Item item)
        {
            string id = CanonicalId(item.Id);
            // Check every existing key before removing any of them.
            foreach (string path in ClassPaths(id)) AssertSlotAvailable(path);
            AssertSlotAvailable(DesktopRoot + id);
            RemoveOwnedKey(DesktopRoot + id);
            foreach (string path in ClassPaths(id)) RemoveOwnedKey(path);
            using (RegistryKey hidden = Registry.CurrentUser.OpenSubKey(DesktopHidden, true))
                if (hidden != null) hidden.DeleteValue(id, false);
        }

        private static void RemoveProxy(Item item)
        {
            if (!item.IsFile) return;
            string directory = ProxyDirectory(item.Id);
            string shortcut = System.IO.Path.Combine(directory, ProxyShortcut);
            try
            {
                if (File.Exists(shortcut)) File.Delete(shortcut);
                if (Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0)
                    Directory.Delete(directory, false);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static void Add(Item item, bool enabled)
        {
            AddWithShortcut(item, enabled, CreateShortcut);
        }

        internal static void AddWithShortcut(Item item, bool enabled, Action<string, string> createShortcut)
        {
            string id = CanonicalId(item.Id);
            CheckTarget(item);
            using (RegistryKey existing = Registry.CurrentUser.OpenSubKey(MetadataRoot + @"\" + id))
                if (existing != null) throw new InvalidOperationException("This custom navigation item already exists.");
            bool registered = false;
            try
            {
                if (enabled) { Register(item, createShortcut); registered = true; }
                using (RegistryKey meta = Registry.CurrentUser.CreateSubKey(MetadataRoot + @"\" + id))
                {
                    if (meta == null) throw new InvalidOperationException("Cannot save the navigation item.");
                    meta.SetValue(OwnerValue, Owner, RegistryValueKind.String);
                    meta.SetValue("Path", item.Path, RegistryValueKind.String);
                    meta.SetValue("Name", item.Name, RegistryValueKind.String);
                    meta.SetValue("IsFile", item.IsFile ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch
            {
                if (registered) { try { Unregister(item); } catch { } }
                try { RemoveOwnedKey(MetadataRoot + @"\" + id); } catch { }
                if (registered) RemoveProxy(item);
                throw;
            }
        }

        public static void SetEnabled(Item item, bool enabled)
        {
            SetEnabledWithShortcut(item, enabled, CreateShortcut);
        }

        internal static void SetEnabledWithShortcut(Item item, bool enabled, Action<string, string> createShortcut)
        {
            string id = CanonicalId(item.Id);
            using (RegistryKey meta = Registry.CurrentUser.OpenSubKey(MetadataRoot + @"\" + id))
                if (!Owned(meta)) throw new InvalidOperationException("The custom navigation item could not be found.");
            if (enabled) Register(item, createShortcut);
            else Unregister(item);
        }

        public static void Delete(Item item)
        {
            string id = CanonicalId(item.Id);
            using (RegistryKey meta = Registry.CurrentUser.OpenSubKey(MetadataRoot + @"\" + id))
                if (!Owned(meta)) throw new InvalidOperationException("The custom navigation item could not be found.");
            Unregister(item);
            RemoveOwnedKey(MetadataRoot + @"\" + id);
            RemoveProxy(item);
        }

        [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLinkObject { }

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int count, IntPtr data, uint flags);
            void GetIDList(out IntPtr list);
            void SetIDList(IntPtr list);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder description, int count);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string description);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory, int count);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder args, int count);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string args);
            void GetHotkey(out short hotkey);
            void SetHotkey(short hotkey);
            void GetShowCmd(out int command);
            void SetShowCmd(int command);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int count, out int index);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string path, int index);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
            void Resolve(IntPtr hwnd, uint flags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
        }

        private static void CreateShortcut(string target, string shortcut)
        {
            object link = new ShellLinkObject();
            try
            {
                IShellLinkW shellLink = (IShellLinkW)link;
                shellLink.SetPath(target);
                string parent = System.IO.Path.GetDirectoryName(target);
                if (!string.IsNullOrEmpty(parent)) shellLink.SetWorkingDirectory(parent);
                shellLink.SetDescription("Open " + System.IO.Path.GetFileName(target));
                ((IPersistFile)link).Save(shortcut, true);
            }
            finally { Marshal.FinalReleaseComObject(link); }
        }
    }
}
