using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TharaThemeExplorer.Core
{
    internal static class TypographyHelper
    {
        public static void GetCurrentFont(out string face, out int pt)
        {
            face = "Segoe UI";
            pt = 9;
            try
            {
                byte[] data = new byte[108];
                BitConverter.GetBytes(108).CopyTo(data, 0);
                if (NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETICONTITLELOGFONT, 108, data, 0))
                {
                    string f = Encoding.Unicode.GetString(data, 44, 64).Split('\0')[0];
                    if (f.Length > 0) face = f;
                    int height = Math.Abs(BitConverter.ToInt32(data, 16));
                    int dpi = (int)Math.Round(96f * ThemeColors.ScaleFactor);
                    if (height > 0)
                    {
                        pt = (int)Math.Round((double)height * 72.0 / dpi);
                    }
                }
            }
            catch { }
        }

        private static void SetLogFont(byte[] buf, int offset, string face, int height)
        {
            BitConverter.GetBytes(height).CopyTo(buf, offset);
            BitConverter.GetBytes(0).CopyTo(buf, offset + 4);
            byte[] nameBytes = new byte[64];
            byte[] strBytes = Encoding.Unicode.GetBytes(face.Length <= 31 ? face : face.Substring(0, 31));
            Array.Copy(strBytes, nameBytes, strBytes.Length);
            Array.Copy(nameBytes, 0, buf, offset + 28, 64);
        }

        public static void ApplyTypography(string face, int pt, bool icons, bool menus, bool messages, bool titles, bool status)
        {
            int dpi = (int)Math.Round(96f * ThemeColors.ScaleFactor);
            int height = -(int)Math.Round((double)(pt * dpi) / 72.0);

            if (menus || messages || titles || status)
            {
                byte[] metrics = new byte[504];
                BitConverter.GetBytes(504).CopyTo(metrics, 0);
                if (!NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, 504, metrics, 0))
                {
                    throw new Exception("Could not read non-client metrics (Win32 error " + Marshal.GetLastWin32Error() + ")");
                }
                if (titles)
                {
                    SetLogFont(metrics, 24, face, height);
                    SetLogFont(metrics, 124, face, height);
                }
                if (menus)
                {
                    SetLogFont(metrics, 224, face, height);
                }
                if (status)
                {
                    SetLogFont(metrics, 316, face, height);
                }
                if (messages)
                {
                    SetLogFont(metrics, 408, face, height);
                }
                uint flags = NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE;
                if (!NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETNONCLIENTMETRICS, 504, metrics, flags))
                {
                    throw new Exception("Windows refused font metrics change (Win32 error " + Marshal.GetLastWin32Error() + ")");
                }
            }

            if (icons)
            {
                byte[] iconData = new byte[108];
                BitConverter.GetBytes(108).CopyTo(iconData, 0);
                if (!NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETICONTITLELOGFONT, 108, iconData, 0))
                {
                    throw new Exception("Could not read icon font (Win32 error " + Marshal.GetLastWin32Error() + ")");
                }
                SetLogFont(iconData, 16, face, height);
                uint flags = NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE;
                if (!NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETICONTITLELOGFONT, 108, iconData, flags))
                {
                    throw new Exception("Windows refused icon font change (Win32 error " + Marshal.GetLastWin32Error() + ")");
                }
            }
        }
    }
}
