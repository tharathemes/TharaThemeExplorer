using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace TharaThemeExplorer.Core
{
    internal static class ThemeColors
    {
        public static float ScaleFactor { get; private set; }
        public static bool Dark { get; private set; }
        public static string IconFamily { get; private set; }

        public static Color CBlue { get; private set; }
        public static Color CIndigo { get; private set; }
        public static Color CPurple { get; private set; }
        public static Color CPink { get; private set; }
        public static Color CPinkDark { get; private set; }
        public static Color Accent { get; private set; }
        public static Color CGreen { get; private set; }
        public static Color CRed { get; private set; }
        public static Color CCard { get; private set; }
        public static Color CCardBorder { get; private set; }
        public static Color CBorder { get; private set; }
        public static Color CTile { get; private set; }
        public static Color CTileBorder { get; private set; }
        public static Color CText { get; private set; }
        public static Color CSub { get; private set; }
        public static Color CTrack { get; private set; }
        public static Color CKnobOff { get; private set; }
        public static Color CBar { get; private set; }
        public static Color[] CBody { get; private set; }
        public static Color[] CHead { get; private set; }

        private static readonly Dictionary<string, Font> fontCache = new Dictionary<string, Font>();
        private static Image cachedLogo;

        static ThemeColors()
        {
            RefreshTheme();
        }

        public static void RefreshTheme()
        {
            ScaleFactor = CalculateScale();
            Dark = DetectDarkTheme();
            IconFamily = DetectIconFamily();

            CBlue = !Dark ? Color.FromArgb(0, 102, 254) : Color.FromArgb(96, 165, 250);
            CIndigo = !Dark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(129, 140, 248);
            CPurple = !Dark ? Color.FromArgb(135, 85, 246) : Color.FromArgb(167, 139, 250);
            CPink = !Dark ? Color.FromArgb(236, 72, 153) : Color.FromArgb(244, 114, 182);
            CPinkDark = !Dark ? Color.FromArgb(219, 39, 119) : Color.FromArgb(236, 72, 153);
            Accent = !Dark ? CPurple : CPink;
            CGreen = Color.FromArgb(52, 199, 89);
            CRed = Color.FromArgb(255, 95, 87);
            CCard = !Dark ? Color.FromArgb(253, 253, 255) : Color.FromArgb(28, 28, 38);
            CCardBorder = !Dark ? Color.FromArgb(218, 216, 240) : Color.FromArgb(70, 58, 106);
            CBorder = !Dark ? Color.FromArgb(221, 225, 238) : Color.FromArgb(48, 49, 62);
            CTile = !Dark ? Color.FromArgb(246, 246, 252) : Color.FromArgb(21, 21, 29);
            CTileBorder = !Dark ? Color.FromArgb(225, 227, 240) : Color.FromArgb(52, 52, 68);
            CText = !Dark ? Color.FromArgb(24, 32, 51) : Color.FromArgb(241, 242, 245);
            CSub = !Dark ? Color.FromArgb(109, 113, 131) : Color.FromArgb(154, 158, 173);
            CTrack = !Dark ? Color.FromArgb(228, 231, 242) : Color.FromArgb(52, 52, 63);
            CKnobOff = !Dark ? Color.White : Color.FromArgb(235, 235, 240);
            CBar = !Dark ? Color.FromArgb(250, 250, 254) : Color.FromArgb(22, 22, 29);

            if (Dark)
            {
                CBody = new Color[]
                {
                    Color.FromArgb(22, 22, 27),
                    Color.FromArgb(27, 26, 36),
                    Color.FromArgb(34, 26, 35),
                    Color.FromArgb(27, 26, 36)
                };
                CHead = new Color[]
                {
                    Color.FromArgb(30, 32, 46),
                    Color.FromArgb(34, 28, 50),
                    Color.FromArgb(42, 28, 42),
                    Color.FromArgb(34, 28, 50)
                };
            }
            else
            {
                CBody = new Color[]
                {
                    Color.FromArgb(237, 242, 252),
                    Color.FromArgb(243, 239, 252),
                    Color.FromArgb(252, 240, 246),
                    Color.FromArgb(243, 239, 252)
                };
                CHead = new Color[]
                {
                    Color.FromArgb(248, 251, 255),
                    Color.FromArgb(247, 244, 255),
                    Color.FromArgb(255, 246, 250),
                    Color.FromArgb(247, 244, 255)
                };
            }
        }

        public static int S(double v)
        {
            return (int)Math.Round(v * ScaleFactor);
        }

        private static float CalculateScale()
        {
            try
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    return g.DpiX / 96f;
                }
            }
            catch
            {
                return 1f;
            }
        }

        private static bool DetectDarkTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val is int)
                        {
                            return (int)val == 0;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static string DetectIconFamily()
        {
            try
            {
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    foreach (FontFamily ff in installed.Families)
                    {
                        if (ff.Name == "Segoe Fluent Icons")
                        {
                            return ff.Name;
                        }
                    }
                }
            }
            catch { }
            return "Segoe MDL2 Assets";
        }

        public static Font F(string family, float size, FontStyle style = FontStyle.Regular)
        {
            string key = family + "|" + size + "|" + (int)style;
            Font font;
            if (!fontCache.TryGetValue(key, out font))
            {
                try
                {
                    font = new Font(family, size, style);
                }
                catch
                {
                    font = new Font("Segoe UI", size, style);
                }
                fontCache[key] = font;
            }
            return font;
        }

        public static Font IconFont(float pt)
        {
            return F(IconFamily, pt);
        }

        public static string Glyph(int code)
        {
            return char.ConvertFromUtf32(code);
        }

        public static Color Tint(Color c, int alpha)
        {
            return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), c);
        }

        public static Color Dim(Color c, bool enabled)
        {
            return !enabled ? Color.FromArgb(Math.Max(40, (int)(c.A * 45 / 100)), c) : c;
        }

        public static GraphicsPath RoundRect(Rectangle r, int rad)
        {
            int d = Math.Max(2, rad * 2);
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180f, 90f);
            path.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
            path.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillSoftGradient(Graphics g, Rectangle r, Color[] colors, float angle)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (LinearGradientBrush brush = new LinearGradientBrush(r, colors[0], colors[3], angle))
            {
                brush.InterpolationColors = new ColorBlend(4)
                {
                    Colors = colors,
                    Positions = new float[] { 0f, 0.34f, 0.67f, 1f }
                };
                g.FillRectangle(brush, r);
            }
        }

        public static void DrawGradientLine(Graphics g, Rectangle line)
        {
            if (line.Width <= 0 || line.Height <= 0) return;
            using (LinearGradientBrush brush = new LinearGradientBrush(line, Color.Transparent, Color.Transparent, 0f))
            {
                Color cBlue = Color.FromArgb(96, 165, 250);
                Color cPurple = Color.FromArgb(167, 139, 250);
                Color cPink = Color.FromArgb(244, 114, 182);

                brush.InterpolationColors = new ColorBlend(6)
                {
                    Colors = new Color[]
                    {
                        Color.FromArgb(0, cBlue),
                        cBlue,
                        cPurple,
                        cPurple,
                        cPink,
                        Color.FromArgb(0, cPink)
                    },
                    Positions = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f }
                };
                g.FillRectangle(brush, line);
            }
        }

        private static readonly Dictionary<string, Image> imageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image LoadImage(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return null;
            Image img;
            if (imageCache.TryGetValue(resourceName, out img)) return img;

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string[] resNames = asm.GetManifestResourceNames();
                foreach (string name in resNames)
                {
                    if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream s = asm.GetManifestResourceStream(name))
                        {
                            if (s != null)
                            {
                                using (Bitmap bmp = new Bitmap(s))
                                {
                                    img = new Bitmap(bmp);
                                    imageCache[resourceName] = img;
                                    return img;
                                }
                            }
                        }
                    }
                }

                // Fallback: load from Resources on disk
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Resources", resourceName);
                if (File.Exists(path))
                {
                    using (Bitmap bmp = new Bitmap(path))
                    {
                        img = new Bitmap(bmp);
                        imageCache[resourceName] = img;
                        return img;
                    }
                }
            }
            catch { }

            return null;
        }

        public static Image LoadLogo()
        {
            if (cachedLogo != null) return cachedLogo;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string[] resNames = asm.GetManifestResourceNames();
                foreach (string name in resNames)
                {
                    if (name.EndsWith("logo.png", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream s = asm.GetManifestResourceStream(name))
                        {
                            if (s != null)
                            {
                                using (Bitmap bmp = new Bitmap(s))
                                {
                                    cachedLogo = new Bitmap(bmp);
                                    return cachedLogo;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
