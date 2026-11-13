using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer
{
    public partial class Program
    {
        public class Segmented : Control
        {
            private readonly string[] items;
            private int selectedIndex;
            private int hoverIndex = -1;

        public event EventHandler SelectedChanged;

        public string[] Items { get { return items; } }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                int clamped = Math.Max(0, Math.Min(items.Length - 1, value));
                if (selectedIndex != clamped)
                {
                    selectedIndex = clamped;
                    Invalidate();
                    if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        public Segmented(string[] items, int itemWidthPx)
        {
            this.items = items ?? new string[0];
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            int inset = ThemeColors.S(3);
            int totalW = ThemeColors.S(itemWidthPx) * this.items.Length + inset * 2;
            Size = new Size(totalW, ThemeColors.S(28));
            Cursor = Cursors.Hand;
            Font = ThemeColors.F("Segoe UI Semibold", 8.75f);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!Enabled || items.Length == 0) return;

            int inset = ThemeColors.S(3);
            int availW = Width - inset * 2;
            int idx = Math.Max(0, Math.Min(items.Length - 1, (e.X - inset) * items.Length / Math.Max(1, availW)));
            if (hoverIndex != idx)
            {
                hoverIndex = idx;
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
            if (!Enabled || e.Button != MouseButtons.Left || items.Length == 0) return;

            int inset = ThemeColors.S(3);
            int availW = Width - inset * 2;
            SelectedIndex = Math.Max(0, Math.Min(items.Length - 1, (e.X - inset) * items.Length / Math.Max(1, availW)));
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Outer track capsule matching user reference
            Rectangle trackRect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath trackPath = ThemeColors.RoundRect(trackRect, trackRect.Height / 2))
            {
                Color bgTrack = ThemeColors.Dim(!ThemeColors.Dark ? Color.FromArgb(236, 237, 243) : Color.FromArgb(32, 32, 44), Enabled);
                using (SolidBrush brush = new SolidBrush(bgTrack))
                {
                    g.FillPath(brush, trackPath);
                }
                Color borderTrack = ThemeColors.Dim(!ThemeColors.Dark ? Color.FromArgb(222, 224, 236) : Color.FromArgb(52, 52, 68), Enabled);
                using (Pen pen = new Pen(borderTrack))
                {
                    g.DrawPath(pen, trackPath);
                }
            }

            if (items.Length == 0) return;
            int inset = ThemeColors.S(3);
            int availW = Width - inset * 2;

            for (int i = 0; i < items.Length; i++)
            {
                int x1 = inset + (i * availW) / items.Length;
                int x2 = inset + ((i + 1) * availW) / items.Length;
                Rectangle segRect = new Rectangle(x1, inset, x2 - x1, Height - inset * 2);
                bool isSelected = (i == selectedIndex);
                bool isHovered = (i == hoverIndex && !isSelected && Enabled);

                if (isSelected)
                {
                    // Pink in dark mode matching active tab, purple in light mode
                    Color activePillColor = ThemeColors.Dark ? ThemeColors.CPink : Color.FromArgb(133, 87, 244);
                    using (GraphicsPath pillPath = ThemeColors.RoundRect(segRect, segRect.Height / 2))
                    {
                        using (SolidBrush pillBrush = new SolidBrush(activePillColor))
                        {
                            g.FillPath(pillBrush, pillPath);
                        }
                    }
                }
                else if (isHovered)
                {
                    using (GraphicsPath hoverPath = ThemeColors.RoundRect(segRect, segRect.Height / 2))
                    {
                        using (SolidBrush hoverBrush = new SolidBrush(ThemeColors.Tint(ThemeColors.CText, !ThemeColors.Dark ? 10 : 16)))
                        {
                            g.FillPath(hoverBrush, hoverPath);
                        }
                    }
                }

                Font font = ThemeColors.F("Segoe UI Semibold", 8.75f);
                Color textColor;
                if (isSelected)
                {
                    textColor = Color.White;
                }
                else
                {
                    textColor = !ThemeColors.Dark ? Color.FromArgb(60, 65, 85) : Color.FromArgb(180, 184, 200);
                }

                TextRenderer.DrawText(g, items[i], font, segRect, textColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.NoPadding);
            }
        }
    }
}
}

namespace TharaThemeExplorer.UI
{
    public class SegmentedControl : Program.Segmented
    {
        public SegmentedControl(string[] items, int itemWidthPx) : base(items, itemWidthPx) { }
    }
}
