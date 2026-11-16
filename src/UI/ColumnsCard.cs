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
            public Control Right;
            public Control Extra;
        }

        private readonly string glyph;
        private readonly string heading;
        private int currentY;
        private Control headerButton;
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
            currentY = ThemeColors.S(40);
            Height = currentY;
        }

        public void SetHeaderButton(Control button)
        {
            headerButton = button;
            button.BackColor = ThemeColors.CCard;
            PositionHeaderButton();
            Controls.Add(button);
            Invalidate();
        }

        private void PositionHeaderButton()
        {
            if (headerButton == null) return;
            headerButton.Left = Width - headerButton.Width - ThemeColors.S(2);
            headerButton.Top = ThemeColors.S(3);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            PositionHeaderButton();
        }

        public void Tile(string text, Control right)
        {
            Tile(text, right, null);
        }

        public void Tile(string text, Control right, Control extra)
        {
            tiles.Add(new TileInfo { Text = text, Right = right, Extra = extra });

            right.BackColor = ThemeColors.CTile;
            Controls.Add(right);
            if (extra != null)
            {
                extra.BackColor = ThemeColors.CTile;
                Controls.Add(extra);
            }
            LayoutTiles();
        }

        public void RemoveTile(Control right)
        {
            TileInfo found = null;
            foreach (TileInfo tile in tiles)
                if (tile.Right == right) { found = tile; break; }
            if (found == null) return;
            tiles.Remove(found);
            Controls.Remove(found.Right);
            found.Right.Dispose();
            if (found.Extra != null) { Controls.Remove(found.Extra); found.Extra.Dispose(); }
            LayoutTiles();
        }

        private void LayoutTiles()
        {
            int tileHeight = ThemeColors.S(46);
            int verticalStep = tileHeight + ThemeColors.S(10);
            for (int i = 0; i < tiles.Count; i++)
            {
                TileInfo tile = tiles[i];
                int y = ThemeColors.S(40) + i * verticalStep;
                tile.Rect = new Rectangle(0, y, Width - 1, tileHeight);
                tile.Right.Left = tile.Rect.Right - ThemeColors.S(14) - tile.Right.Width;
                tile.Right.Top = tile.Rect.Y + (tile.Rect.Height - tile.Right.Height) / 2;
                if (tile.Extra != null)
                {
                    tile.Extra.Left = tile.Right.Left - ThemeColors.S(7) - tile.Extra.Width;
                    tile.Extra.Top = tile.Rect.Y + (tile.Rect.Height - tile.Extra.Height) / 2;
                }
            }
            currentY = ThemeColors.S(40) + tiles.Count * verticalStep;
            Height = currentY;
            Invalidate();
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
            int headerWidth = headerButton == null ? Width - ThemeColors.S(24) : Math.Max(1, headerButton.Left - ThemeColors.S(28));
            Rectangle headTextRect = new Rectangle(ThemeColors.S(22), ThemeColors.S(5), headerWidth, headerH);

            TextRenderer.DrawText(g, glyph, glyphFont, glyphRect, ThemeColors.CSub,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, heading, headingFont, headTextRect, ThemeColors.CText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

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
                int rightEdge = t.Extra != null ? t.Extra.Left : t.Right.Left;
                Rectangle textRect = new Rectangle(t.Rect.X + ThemeColors.S(14), t.Rect.Y + (t.Rect.Height - ThemeColors.S(20)) / 2, Math.Max(1, rightEdge - t.Rect.X - ThemeColors.S(24)), ThemeColors.S(20));
                TextRenderer.DrawText(g, t.Text, labelFont, textRect, ThemeColors.CText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
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
            col.SizeChanged += (s, e) => UpdateColumnHeight();
            UpdateColumnHeight();
        }

        private void UpdateColumnHeight()
        {
            int maxH = 0;
            foreach (ColumnPanel c in columns)
            {
                if (c.Height > maxH) maxH = c.Height;
            }
            Height = ThemeColors.S(68) + ThemeColors.S(18) + maxH + ThemeColors.S(8);
        }
    }
}
