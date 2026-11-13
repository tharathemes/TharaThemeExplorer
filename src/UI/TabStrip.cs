using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class TabStrip : Control
    {
        private readonly string[] texts;
        private readonly string[] glyphs;
        private readonly Rectangle[] rects;
        private int selectedIndex;
        private int hoverIndex = -1;
        private readonly Font font = ThemeColors.F("Segoe UI Semibold", 8.75f);

        public event EventHandler SelectedChanged;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                int clamped = Math.Max(0, Math.Min(texts.Length - 1, value));
                if (selectedIndex != clamped)
                {
                    selectedIndex = clamped;
                    Invalidate();
                    if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        public TabStrip(string[] texts, string[] glyphs)
        {
            this.texts = texts ?? new string[0];
            this.glyphs = glyphs ?? new string[0];
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            rects = new Rectangle[this.texts.Length];

            int tabH = ThemeColors.S(28);
            int totalW = 0;
            for (int i = 0; i < this.texts.Length; i++)
            {
                int textW = TextRenderer.MeasureText(this.texts[i], font).Width;
                int itemW = ThemeColors.S(10) + ThemeColors.S(18) + textW + ThemeColors.S(12);
                rects[i] = new Rectangle(totalW, 0, itemW, tabH);
                totalW += itemW + ThemeColors.S(6);
            }
            Size = new Size(totalW, tabH);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int newHover = -1;
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(e.Location))
                {
                    newHover = i;
                    break;
                }
            }
            if (hoverIndex != newHover)
            {
                hoverIndex = newHover;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoverIndex != -1)
            {
                hoverIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(e.Location))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < rects.Length; i++)
            {
                Rectangle r = rects[i];
                bool isSel = (i == selectedIndex);
                bool isHov = (i == hoverIndex && !isSel);

                if (isSel)
                {
                    using (GraphicsPath path = ThemeColors.RoundRect(r, r.Height / 2))
                    {
                        using (SolidBrush brush = new SolidBrush(ThemeColors.Accent))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                }
                else if (isHov)
                {
                    using (GraphicsPath path = ThemeColors.RoundRect(r, r.Height / 2))
                    {
                        using (SolidBrush brush = new SolidBrush(ThemeColors.Tint(ThemeColors.CText, !ThemeColors.Dark ? 10 : 18)))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                }

                Color c = isSel ? Color.White : (isHov ? ThemeColors.CText : ThemeColors.CSub);
                Font iconF = ThemeColors.IconFont(9f);

                int glyphX = r.X + ThemeColors.S(10);
                int glyphY = r.Y + (r.Height - ThemeColors.S(14)) / 2;
                TextRenderer.DrawText(g, glyphs[i], iconF, new Point(glyphX, glyphY), c, TextFormatFlags.NoPadding);

                int textX = glyphX + ThemeColors.S(18);
                int textY = r.Y + (r.Height - ThemeColors.S(16)) / 2;
                TextRenderer.DrawText(g, texts[i], font, new Point(textX, textY), c, TextFormatFlags.NoPadding);
            }
        }
    }
}
