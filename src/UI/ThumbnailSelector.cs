using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class ThumbnailOption
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageName { get; set; }
        public Image Image { get; set; }

        public ThumbnailOption(string title, string subtitle, string imageName)
        {
            Title = title;
            Subtitle = subtitle;
            ImageName = imageName;
            Image = ThemeColors.LoadImage(imageName);
        }
    }

    public class ThumbnailSelector : Control
    {
        private readonly List<ThumbnailOption> items = new List<ThumbnailOption>();
        private int selectedIndex = 0;
        private int hoverIndex = -1;

        public event EventHandler SelectedChanged;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                int clamped = Math.Max(0, Math.Min(items.Count - 1, value));
                if (selectedIndex != clamped)
                {
                    selectedIndex = clamped;
                    Invalidate();
                    if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        public int Count { get { return items.Count; } }

        public ThumbnailSelector()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = ThemeColors.CCard;
            TabStop = true;
            Cursor = Cursors.Hand;
        }

        public void AddItem(string title, string subtitle, string imageName)
        {
            items.Add(new ThumbnailOption(title, subtitle, imageName));
            Invalidate();
        }

        public Rectangle GetItemRect(int index)
        {
            if (index < 0 || index >= items.Count) return Rectangle.Empty;

            int gap = ThemeColors.S(10);
            // Size each thumbnail card using standard slot width (~148px)
            int itemW = (Width - gap * 2) / 3;

            int x = index * (itemW + gap);
            int w = (items.Count == 3 && index == 2) ? (Width - x) : itemW;
            return new Rectangle(x, 1, Math.Max(1, w), Math.Max(1, Height - 2));
        }

        private int HitTest(Point pt)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Rectangle r = GetItemRect(i);
                if (r.Contains(pt)) return i;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = HitTest(e.Location);
            if (idx != hoverIndex)
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                int idx = HitTest(e.Location);
                if (idx >= 0 && idx < items.Count)
                {
                    SelectedIndex = idx;
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (items.Count == 0) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                    SelectedIndex = Math.Max(0, selectedIndex - 1);
                    e.Handled = true;
                    break;
                case Keys.Right:
                case Keys.Down:
                    SelectedIndex = Math.Min(items.Count - 1, selectedIndex + 1);
                    e.Handled = true;
                    break;
                case Keys.Home:
                    SelectedIndex = 0;
                    e.Handled = true;
                    break;
                case Keys.End:
                    SelectedIndex = items.Count - 1;
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(ThemeColors.CCard);

            if (items.Count == 0) return;

            Font titleFont = ThemeColors.F("Segoe UI Semibold", 9.25f);
            Font subFont = ThemeColors.F("Segoe UI", 8.25f);
            Font badgeFont = ThemeColors.F("Segoe UI Semibold", 7.5f);

            int rad = ThemeColors.S(14);

            for (int i = 0; i < items.Count; i++)
            {
                Rectangle itemR = GetItemRect(i);
                bool isSelected = (i == selectedIndex);
                bool isHovered = (i == hoverIndex && !isSelected);

                int divY = itemR.Bottom - ThemeColors.S(48);
                Rectangle imgBox = new Rectangle(itemR.X + ThemeColors.S(8), itemR.Top + ThemeColors.S(8), itemR.Width - ThemeColors.S(16), divY - itemR.Top - ThemeColors.S(14));

                // 1. Card Background & Outline Border
                using (GraphicsPath cardPath = ThemeColors.RoundRect(itemR, rad))
                {
                    using (SolidBrush bgBrush = new SolidBrush(ThemeColors.CTile))
                    {
                        g.FillPath(bgBrush, cardPath);
                    }

                    // 2. Outline Border
                    if (isSelected)
                    {
                        Rectangle borderR = new Rectangle(itemR.X + 1, itemR.Y + 1, itemR.Width - 2, itemR.Height - 2);
                        using (GraphicsPath bPath = ThemeColors.RoundRect(borderR, Math.Max(2, rad - 1)))
                        using (Pen p = new Pen(ThemeColors.CPink, 2f))
                        {
                            g.DrawPath(p, bPath);
                        }
                    }
                    else if (isHovered)
                    {
                        using (Pen p = new Pen(ThemeColors.Tint(ThemeColors.Accent, 180), 1.25f))
                        {
                            g.DrawPath(p, cardPath);
                        }
                    }
                    else
                    {
                        using (Pen p = new Pen(ThemeColors.CTileBorder, 1f))
                        {
                            g.DrawPath(p, cardPath);
                        }
                    }

                    // 3. Horizontal Divider Line
                    using (Pen divPen = new Pen(isSelected ? ThemeColors.Tint(ThemeColors.CPink, 70) : ThemeColors.CTileBorder, 1f))
                    {
                        g.DrawLine(divPen, itemR.X + ThemeColors.S(8), divY, itemR.Right - ThemeColors.S(8), divY);
                    }

                    // 4. Thumbnail Illustration
                    if (items[i].Image != null)
                    {
                        Image img = items[i].Image;
                        float scale = Math.Min((float)imgBox.Width / img.Width, (float)imgBox.Height / img.Height);
                        int dw = Math.Max(1, (int)Math.Round(img.Width * scale));
                        int dh = Math.Max(1, (int)Math.Round(img.Height * scale));
                        int dx = imgBox.X + (imgBox.Width - dw) / 2;
                        int dy = imgBox.Y + (imgBox.Height - dh) / 2;

                        g.DrawImage(img, dx, dy, dw, dh);
                    }
                }

                // 5. "Active" Badge (Top-right pill badge, only on active card)
                if (isSelected)
                {
                    string badgeText = "Active";
                    Size bTextSz = TextRenderer.MeasureText(badgeText, badgeFont);
                    int badgeW = bTextSz.Width + ThemeColors.S(8);
                    int badgeH = ThemeColors.S(16);
                    Rectangle badgeR = new Rectangle(itemR.Right - badgeW - ThemeColors.S(6), itemR.Top + ThemeColors.S(6), badgeW, badgeH);

                    using (GraphicsPath badgePath = ThemeColors.RoundRect(badgeR, ThemeColors.S(8)))
                    using (SolidBrush badgeBrush = new SolidBrush(ThemeColors.CPink))
                    {
                        g.FillPath(badgeBrush, badgePath);
                    }

                    TextRenderer.DrawText(g, badgeText, badgeFont, badgeR, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }

                // 6. Title Text
                int titleY = divY + ThemeColors.S(5);
                Rectangle titleR = new Rectangle(itemR.X + ThemeColors.S(4), titleY, itemR.Width - ThemeColors.S(8), ThemeColors.S(18));
                TextRenderer.DrawText(g, items[i].Title, titleFont, titleR, ThemeColors.CText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                // 7. Subtitle Text (Pink if active, subtle slate if inactive)
                int subY = titleY + ThemeColors.S(16);
                Rectangle subR = new Rectangle(itemR.X + ThemeColors.S(4), subY, itemR.Width - ThemeColors.S(8), ThemeColors.S(16));
                Color subColor = isSelected ? ThemeColors.CPink : ThemeColors.CSub;
                TextRenderer.DrawText(g, items[i].Subtitle, subFont, subR, subColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }
}
