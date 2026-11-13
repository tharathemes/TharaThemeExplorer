using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class CardPanel : Panel
    {
        private class CardRowInfo
        {
            public string Title;
            public string Desc;
            public Rectangle TitleRect;
            public Rectangle DescRect;
            public int DividerY;
        }

        private class NoteInfo
        {
            public string Text;
            public Rectangle Rect;
        }

        private readonly string glyph;
        private readonly string title;
        private readonly string subtitle;
        private readonly Color iconColor;
        private int currentY;
        private int rowCount;

        private readonly List<CardRowInfo> rows = new List<CardRowInfo>();
        private readonly List<NoteInfo> notes = new List<NoteInfo>();
        private readonly List<int> dividers = new List<int>();

        private static readonly Font titleFont = ThemeColors.F("Segoe UI Semibold", 11f);
        private static readonly Font subtitleFont = ThemeColors.F("Segoe UI", 8.25f);
        private static readonly Font rowTitleFont = ThemeColors.F("Segoe UI Semibold", 9.25f);
        private static readonly Font descFont = ThemeColors.F("Segoe UI", 8.25f);

        public CardPanel(string glyph, string title, string subtitle, Color iconColor, int widthPx)
        {
            this.glyph = glyph;
            this.title = title;
            this.subtitle = subtitle;
            this.iconColor = iconColor;

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = ThemeColors.CCard;
            Width = ThemeColors.S(widthPx);
            currentY = ThemeColors.S(68);
            Height = currentY + ThemeColors.S(10);
        }

        private int MeasureDescHeight(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return TextRenderer.MeasureText(text, descFont,
                new Size(Math.Max(10, width), 4000),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + ThemeColors.S(3);
        }

        public void Row(string text, string desc, Control right, Control below)
        {
            bool hasDesc = !string.IsNullOrEmpty(desc);
            int textW = Width - ThemeColors.S(32) - right.Width - ThemeColors.S(16);
            int descH = hasDesc ? MeasureDescHeight(desc, textW - ThemeColors.S(2)) : 0;

            int rowH = Math.Max(ThemeColors.S(48),
                ThemeColors.S(10) + ThemeColors.S(20) +
                (hasDesc ? (ThemeColors.S(2) + descH) : 0) +
                (below != null ? (ThemeColors.S(8) + below.Height) : 0) +
                ThemeColors.S(12));

            int divY = currentY;
            if (rowCount > 0)
            {
                dividers.Add(divY);
            }
            rowCount++;

            int titleY = (!hasDesc && below == null)
                ? (currentY + (rowH - ThemeColors.S(20)) / 2)
                : (currentY + ThemeColors.S(10));

            CardRowInfo rowInfo = new CardRowInfo
            {
                Title = text,
                Desc = desc,
                TitleRect = new Rectangle(ThemeColors.S(16), titleY, textW, ThemeColors.S(20)),
                DescRect = hasDesc ? new Rectangle(ThemeColors.S(16), titleY + ThemeColors.S(22), textW, descH) : Rectangle.Empty,
                DividerY = divY
            };
            rows.Add(rowInfo);

            if (below != null)
            {
                below.BackColor = ThemeColors.CCard;
                below.Left = ThemeColors.S(16);
                below.Top = titleY + ThemeColors.S(22) + (hasDesc ? (descH + ThemeColors.S(6)) : 0);
                Controls.Add(below);
            }

            right.BackColor = ThemeColors.CCard;
            right.Left = Width - ThemeColors.S(16) - right.Width;
            if (hasDesc || below != null)
            {
                right.Top = titleY + (ThemeColors.S(20) - right.Height) / 2;
            }
            else
            {
                right.Top = currentY + (rowH - right.Height) / 2;
            }
            Controls.Add(right);

            currentY += rowH;
            Height = currentY + ThemeColors.S(8);
        }

        public void Note(string text)
        {
            int noteH = MeasureDescHeight(text, Width - ThemeColors.S(34));
            NoteInfo n = new NoteInfo
            {
                Text = text,
                Rect = new Rectangle(ThemeColors.S(16), currentY - ThemeColors.S(4), Width - ThemeColors.S(32), noteH)
            };
            notes.Add(n);
            currentY += noteH + ThemeColors.S(6);
            Height = currentY + ThemeColors.S(8);
        }

        public void Section(string sectionTitle, Control control, int heightPx)
        {
            int divY = currentY;
            if (rowCount > 0)
            {
                dividers.Add(divY);
            }
            rowCount++;

            int titleH = ThemeColors.S(22);
            if (!string.IsNullOrEmpty(sectionTitle))
            {
                CardRowInfo info = new CardRowInfo
                {
                    Title = sectionTitle,
                    Desc = null,
                    TitleRect = new Rectangle(ThemeColors.S(16), currentY + ThemeColors.S(8), Width - ThemeColors.S(32), titleH),
                    DescRect = Rectangle.Empty,
                    DividerY = divY
                };
                rows.Add(info);
                currentY += titleH + ThemeColors.S(10);
            }

            control.BackColor = ThemeColors.CCard;
            control.Left = ThemeColors.S(16);
            control.Top = currentY;
            control.Width = Width - ThemeColors.S(32);
            control.Height = heightPx;
            Controls.Add(control);

            currentY += heightPx + ThemeColors.S(8);
            Height = currentY + ThemeColors.S(8);
        }

        public void Free(Control c, int heightPx)
        {
            if (rowCount > 0)
            {
                dividers.Add(currentY);
            }
            rowCount++;

            c.BackColor = ThemeColors.CCard;
            c.Left = ThemeColors.S(16);
            c.Top = currentY + ThemeColors.S(10);
            c.Width = Width - ThemeColors.S(32);
            c.Height = heightPx;
            Controls.Add(c);

            currentY += heightPx + ThemeColors.S(20);
            Height = currentY + ThemeColors.S(8);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Seamless parent background matching for card corners
            if (Parent != null && ThemeColors.CBody != null && ThemeColors.CBody.Length >= 4)
            {
                Rectangle parentRectInChild = new Rectangle(-Left, -Top, Parent.ClientSize.Width, Parent.ClientSize.Height);
                ThemeColors.FillSoftGradient(g, parentRectInChild, ThemeColors.CBody, 35f);
            }
            else
            {
                g.Clear(ThemeColors.CBody != null && ThemeColors.CBody.Length > 0 ? ThemeColors.CBody[0] : BackColor);
            }

            // Rounded Card Background & Border
            Rectangle cardRect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath cardPath = ThemeColors.RoundRect(cardRect, ThemeColors.S(14)))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeColors.CCard))
                {
                    g.FillPath(bgBrush, cardPath);
                }
                using (Pen borderPen = new Pen(ThemeColors.CCardBorder))
                {
                    g.DrawPath(borderPen, cardPath);
                }
            }

            // Top Icon Badge
            Rectangle iconCircle = new Rectangle(ThemeColors.S(16), ThemeColors.S(15), ThemeColors.S(38), ThemeColors.S(38));
            using (SolidBrush iconBg = new SolidBrush(ThemeColors.Tint(iconColor, !ThemeColors.Dark ? 30 : 38)))
            {
                g.FillEllipse(iconBg, iconCircle);
            }
            using (Pen iconBorder = new Pen(ThemeColors.Tint(iconColor, 70)))
            {
                g.DrawEllipse(iconBorder, iconCircle);
            }
            Font glyphF = ThemeColors.IconFont(11.5f);
            TextRenderer.DrawText(g, glyph, glyphF, iconCircle, iconColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Card Header Text
            TextRenderer.DrawText(g, title, titleFont,
                new Rectangle(ThemeColors.S(66), ThemeColors.S(14), Width - ThemeColors.S(82), ThemeColors.S(22)),
                ThemeColors.CText, TextFormatFlags.NoPadding);

            if (!string.IsNullOrEmpty(subtitle))
            {
                TextRenderer.DrawText(g, subtitle, subtitleFont,
                    new Rectangle(ThemeColors.S(66), ThemeColors.S(37), Width - ThemeColors.S(82), ThemeColors.S(18)),
                    ThemeColors.CSub, TextFormatFlags.NoPadding);
            }

            // Divider lines
            using (Pen divPen = new Pen(ThemeColors.Tint(ThemeColors.CBorder, 200)))
            {
                g.DrawLine(divPen, ThemeColors.S(16), ThemeColors.S(67), Width - ThemeColors.S(16), ThemeColors.S(67));
                foreach (int dy in dividers)
                {
                    g.DrawLine(divPen, ThemeColors.S(16), dy, Width - ThemeColors.S(16), dy);
                }
            }

            // Render Row Labels
            foreach (CardRowInfo r in rows)
            {
                TextRenderer.DrawText(g, r.Title, rowTitleFont, r.TitleRect, ThemeColors.CText,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                if (!string.IsNullOrEmpty(r.Desc))
                {
                    TextRenderer.DrawText(g, r.Desc, descFont, r.DescRect, ThemeColors.CSub,
                        TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            // Render Notes
            foreach (NoteInfo n in notes)
            {
                TextRenderer.DrawText(g, n.Text, descFont, n.Rect, ThemeColors.CSub,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }

    public class Card : CardPanel
    {
        public Card(string glyph, string title, string subtitle, Color iconColor, int widthPx)
            : base(glyph, title, subtitle, iconColor, widthPx)
        {
        }
    }
}
