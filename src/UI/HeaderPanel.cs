using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class HeaderPanel : Panel
    {
        public Image Logo { get; set; }

        public HeaderPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            ThemeColors.FillSoftGradient(g, new Rectangle(0, 0, Width, Height), ThemeColors.CHead, 8f);

            int logoSize = ThemeColors.S(54);
            Rectangle logoRect = new Rectangle(ThemeColors.S(24), (Height - logoSize) / 2, logoSize, logoSize);
            Rectangle borderRect = Rectangle.Inflate(logoRect, ThemeColors.S(3), ThemeColors.S(3));

            using (GraphicsPath path = ThemeColors.RoundRect(borderRect, ThemeColors.S(16)))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(borderRect, ThemeColors.CPink, ThemeColors.CBlue, 45f))
                {
                    using (Pen pen = new Pen(brush, (float)ThemeColors.S(2)))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            if (Logo != null)
            {
                g.DrawImage(Logo, logoRect);
            }
        }
    }

    public class ToolbarPanel : Panel
    {
        public ToolbarPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(ThemeColors.CBar);
            ThemeColors.DrawGradientLine(g, new Rectangle(0, 0, Width, 1));
            ThemeColors.DrawGradientLine(g, new Rectangle(0, Height - 1, Width, 1));
        }
    }

    public class BodyPanel : Panel
    {
        public BodyPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            DoubleBuffered = true;
            AutoScrollMargin = new Size(0, ThemeColors.S(16));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle r = ClientRectangle;
            if (r.Width < 2 || r.Height < 2) return;
            ThemeColors.FillSoftGradient(e.Graphics, r, ThemeColors.CBody, 35f);
        }
    }
}
