using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class ColumnPanel : Panel
    {
        private class TileInfo
        {
            public string Text;
            public Rectangle Rect;
        }

        private readonly string glyph;
        private readonly string heading;
        private int currentY;
        private readonly List<TileInfo> tiles = new List<TileInfo>();

        private static readonly Font glyphFont = ThemeColors.IconFont(9.5f);
        private static readonly Font headingFont = ThemeColors.F("Segoe UI Semibold", 9.25f);
        private static readonly Font labelFont = ThemeColors.F("Segoe UI Semibold", 9.25f);

        public ColumnPanel(string glyph, string heading, int widthPx)
        {
            this.glyph = glyph;
            this.heading = heading;

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = ThemeColors.CCard;
            Width = ThemeColors.S(widthPx);
            currentY = ThemeColors.S(34);
            Height = currentY;
        }

        public void Tile(string text, Control right)
        {
            Rectangle rect = new Rectangle(0, currentY, Width - 1, ThemeColors.S(46));
            tiles.Add(new TileInfo { Text = text, Rect = rect });

            right.BackColor = ThemeColors.CTile;
            right.Left = rect.Right - ThemeColors.S(14) - right.Width;
            right.Top = rect.Y + (rect.Height - right.Height) / 2;
            Controls.Add(right);

            currentY += rect.Height + ThemeColors.S(10);
            Height = currentY;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(ThemeColors.CCard);

            // Heading Icon and Text
            int headerH = ThemeColors.S(24);
            int glyphYOffset = (heading.IndexOf("add", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                heading.IndexOf("order", StringComparison.OrdinalIgnoreCase) >= 0)
                                ? ThemeColors.S(2) : 0;
            Rectangle glyphRect = new Rectangle(ThemeColors.S(2), ThemeColors.S(5) + glyphYOffset, ThemeColors.S(16), headerH);
            Rectangle headTextRect = new Rectangle(ThemeColors.S(22), ThemeColors.S(5), Width - ThemeColors.S(24), headerH);

            TextRenderer.DrawText(g, glyph, glyphFont, glyphRect, ThemeColors.CSub,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, heading, headingFont, headTextRect, ThemeColors.CText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Tiles
            foreach (TileInfo t in tiles)
            {
                using (GraphicsPath path = ThemeColors.RoundRect(t.Rect, ThemeColors.S(10)))
                {
                    using (SolidBrush brush = new SolidBrush(ThemeColors.CTile))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(ThemeColors.CTileBorder))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Tile Text
                Rectangle textRect = new Rectangle(t.Rect.X + ThemeColors.S(14), t.Rect.Y + (t.Rect.Height - ThemeColors.S(20)) / 2, t.Rect.Width - ThemeColors.S(100), ThemeColors.S(20));
                TextRenderer.DrawText(g, t.Text, labelFont, textRect, ThemeColors.CText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }

    public class ColumnsCard : Card
    {
        private readonly List<ColumnPanel> columns = new List<ColumnPanel>();

        public ColumnsCard(string glyph, string title, string subtitle, Color iconColor, int widthPx)
            : base(glyph, title, subtitle, iconColor, widthPx)
        {
        }

        public void AddColumn(ColumnPanel col)
        {
            col.Left = ThemeColors.S(16) + columns.Count * (col.Width + ThemeColors.S(16));
            col.Top = ThemeColors.S(68) + ThemeColors.S(18);
            columns.Add(col);
            Controls.Add(col);

            int maxH = 0;
            foreach (ColumnPanel c in columns)
            {
                if (c.Height > maxH) maxH = c.Height;
            }
            Height = ThemeColors.S(68) + ThemeColors.S(18) + maxH + ThemeColors.S(8);
        }
    }
}
