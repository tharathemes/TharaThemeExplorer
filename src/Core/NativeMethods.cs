using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TharaThemeExplorer.Core
{
    internal static class NativeMethods
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate void WinEventProc(IntPtr hHook, uint ev, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

        [StructLayout(LayoutKind.Sequential)]
        public struct WTA_OPTIONS
        {
            public uint dwFlags;
            public uint dwMask;
        }

        public enum DWMWINDOWATTRIBUTE : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029,
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        }

        public const uint WM_CLOSE = 0x0010;
        public const uint WM_SETREDRAW = 0x000B;
        public const int WS_EX_COMPOSITED = 0x02000000;

        public const uint SPI_GETNONCLIENTMETRICS = 41;
        public const uint SPI_SETNONCLIENTMETRICS = 42;
        public const uint SPI_GETICONTITLELOGFONT = 45;
        public const uint SPI_SETICONTITLELOGFONT = 46;
        public const uint SPIF_UPDATEINIFILE = 0x01;
        public const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc p, IntPtr l);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr h, StringBuilder sb, int max);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr h);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref int pvAttribute, uint cbAttribute);

        [DllImport("uxtheme.dll")]
        public static extern int SetWindowThemeAttribute(IntPtr hwnd, int attr, ref WTA_OPTIONS opts, uint size);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eMin, uint eMax, IntPtr hmod, WinEventProc proc, uint pid, uint tid, uint flags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hHook);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        public static extern bool SystemParametersInfo(uint action, uint param, [In, Out] byte[] data, uint winIni);
    }
}
