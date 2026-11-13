using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class StatusDot : Control
    {
        private Color dotColor = ThemeColors.CBlue;

        public Color DotColor
        {
            get { return dotColor; }
            set
            {
                if (dotColor != value)
                {
                    dotColor = value;
                    Invalidate();
                }
            }
        }

        public StatusDot()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            Size = new Size(ThemeColors.S(14), ThemeColors.S(14));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush glowBrush = new SolidBrush(ThemeColors.Tint(dotColor, 70)))
            {
                g.FillEllipse(glowBrush, 0, 0, Width - 1, Height - 1);
            }
            int pad = ThemeColors.S(3);
            using (SolidBrush centerBrush = new SolidBrush(dotColor))
            {
                g.FillEllipse(centerBrush, pad, pad, Width - pad * 2 - 1, Height - pad * 2 - 1);
            }
        }
    }

    public class LinkChip : Control
    {
        private readonly string url;
        private bool hover;
        private readonly Font font = ThemeColors.F("Segoe UI Semibold", 8f);

        public LinkChip(string text, string url)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            Text = text;
            this.url = url;
            Cursor = Cursors.Hand;
            int textW = TextRenderer.MeasureText(text, font).Width;
            Size = new Size(textW + ThemeColors.S(38), ThemeColors.S(24));
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

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ThemeColors.RoundRect(rect, rect.Height / 2))
            {
                using (SolidBrush bg = new SolidBrush(ThemeColors.Tint(ThemeColors.CText, !hover ? 10 : 22)))
                {
                    g.FillPath(bg, path);
                }
                using (Pen pen = new Pen(!hover ? ThemeColors.Tint(ThemeColors.CText, 46) : ThemeColors.Tint(ThemeColors.Accent, 160)))
                {
                    g.DrawPath(pen, path);
                }
            }

            Font iconFont = ThemeColors.IconFont(8f);
            TextRenderer.DrawText(g, ThemeColors.Glyph(59559), iconFont, new Point(ThemeColors.S(10), (Height - ThemeColors.S(13)) / 2), ThemeColors.Accent, TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, Text, font, new Point(ThemeColors.S(27), (Height - ThemeColors.S(14)) / 2), ThemeColors.CText, TextFormatFlags.NoPadding);
        }
    }

    public class StatusBadge : Control
    {
        public Color BadgeColor { get; set; }
        public string GlyphText { get; set; }
        public bool Neutral { get; set; }

        private readonly Font smallFont = ThemeColors.F("Segoe UI Semibold", 8f);

        public StatusBadge(string text, Color color, string glyphText = null)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            Text = text;
            BadgeColor = color;
            GlyphText = glyphText;
            int textW = TextRenderer.MeasureText(text, smallFont).Width;
            int extraW = glyphText != null ? 38 : 32;
            Size = new Size(textW + ThemeColors.S(extraW), ThemeColors.S(26));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ThemeColors.RoundRect(rect, rect.Height / 2))
            {
                Color bg = !Neutral ? ThemeColors.Tint(BadgeColor, !ThemeColors.Dark ? 22 : 28) : ThemeColors.Tint(ThemeColors.CText, !ThemeColors.Dark ? 8 : 12);
                Color border = !Neutral ? ThemeColors.Tint(BadgeColor, 110) : ThemeColors.Tint(ThemeColors.CText, 40);

                using (SolidBrush brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(border))
                {
                    g.DrawPath(pen, path);
                }
            }

            int curX = ThemeColors.S(11);
            if (GlyphText != null)
            {
                Font iconF = ThemeColors.IconFont(9f);
                TextRenderer.DrawText(g, GlyphText, iconF, new Point(curX, (Height - ThemeColors.S(14)) / 2), BadgeColor, TextFormatFlags.NoPadding);
                curX += ThemeColors.S(18);
            }
            else
            {
                using (SolidBrush dotBrush = new SolidBrush(BadgeColor))
                {
                    g.FillEllipse(dotBrush, curX, (Height - ThemeColors.S(7)) / 2, ThemeColors.S(7), ThemeColors.S(7));
                }
                curX += ThemeColors.S(15);
            }

            Color textC = !Neutral ? BadgeColor : ThemeColors.CText;
            TextRenderer.DrawText(g, Text, smallFont, new Point(curX, (Height - ThemeColors.S(14)) / 2), textC, TextFormatFlags.NoPadding);
        }
    }
}
