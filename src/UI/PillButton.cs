using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class PillButton : Control
    {
        private bool hover;

        public bool Primary { get; set; }
        public bool IconOnly { get; set; }
        public bool TextOnly { get; set; }
        public float IconSize { get; set; }
        public string IconGlyph { get; set; }

        public PillButton(string text, bool primary, int widthPx)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            Text = text;
            Primary = primary;
            IconSize = 11f;
            Size = new Size(ThemeColors.S(widthPx), ThemeColors.S(34));
            Cursor = Cursors.Hand;
            Font = ThemeColors.F("Segoe UI Semibold", 9.25f);
            TabStop = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Return) && Enabled)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color bg, fg, border;

            if (Primary)
            {
                if (Enabled)
                {
                    bg = !hover ? ThemeColors.Accent : (!ThemeColors.Dark ? ThemeColors.CIndigo : ThemeColors.CPinkDark);
                    fg = Color.White;
                    border = bg;
                }
                else
                {
                    bg = ThemeColors.CTrack;
                    fg = ThemeColors.CSub;
                    border = ThemeColors.CTrack;
                }
            }
            else if (TextOnly)
            {
                bg = (!hover || !Enabled) ? Color.Transparent : ThemeColors.Tint(ThemeColors.CText, 16);
                border = Color.Transparent;
                fg = !Enabled ? ThemeColors.Tint(ThemeColors.CSub, 120) : (!hover ? ThemeColors.CSub : ThemeColors.CText);
            }
            else
            {
                bg = (!hover || !Enabled) ? ThemeColors.Tint(ThemeColors.CText, 6) : ThemeColors.Tint(ThemeColors.CText, 18);
                border = !Enabled ? ThemeColors.CBorder : ThemeColors.Tint(ThemeColors.CText, !ThemeColors.Dark ? 50 : 60);
                fg = !Enabled ? ThemeColors.CSub : ThemeColors.CText;
            }

            int radius = !IconOnly ? ThemeColors.S(10) : (rect.Height / 2);
            using (GraphicsPath path = ThemeColors.RoundRect(rect, radius))
            {
                if (bg.A > 0)
                {
                    using (SolidBrush brush = new SolidBrush(bg))
                    {
                        g.FillPath(brush, path);
                    }
                }
                if (border.A > 0)
                {
                    using (Pen pen = new Pen(border))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            if (IconOnly)
            {
                Font font = ThemeColors.IconFont(IconSize);
                TextRenderer.DrawText(g, Text, font, new Rectangle(0, 0, Width, Height), fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            else if (IconGlyph != null)
            {
                int textW = TextRenderer.MeasureText(Text, Font).Width;
                int startX = (Width - (ThemeColors.S(24) + textW)) / 2;
                Font iconF = ThemeColors.IconFont(10f);
                TextRenderer.DrawText(g, IconGlyph, iconF, new Point(startX, (Height - ThemeColors.S(16)) / 2), fg, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, Text, Font, new Point(startX + ThemeColors.S(24), (Height - ThemeColors.S(16)) / 2), fg, TextFormatFlags.NoPadding);
            }
            else
            {
                TextRenderer.DrawText(g, Text, Font, rect, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}
