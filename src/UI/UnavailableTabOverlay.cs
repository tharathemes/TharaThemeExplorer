using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    internal sealed class UnavailableTabOverlay : Control
    {
        private Bitmap snapshot;
        private string title;
        private string explanation;

        public UnavailableTabOverlay()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            AccessibleRole = AccessibleRole.StaticText;
            TabStop = false;
            Visible = false;
        }

        public void ShowFor(Control page, string selectedWindows, string currentWindows)
        {
            Bounds = page.Bounds;
            title = selectedWindows + " settings are unavailable";
            explanation = "This PC is running " + currentWindows + ". Select the " + currentWindows +
                          " tab to change its Explorer settings.";
            AccessibleName = title + ". " + explanation;
            CapturePage(page);
            Visible = true;
            BringToFront();
            Invalidate();
        }

        public void CapturePage(Control page)
        {
            Bitmap next = null;
            try
            {
                if (page.Width > 0 && page.Height > 0)
                {
                    next = new Bitmap(page.Width, page.Height);
                    page.DrawToBitmap(next, new Rectangle(0, 0, next.Width, next.Height));
                }
            }
            catch
            {
                if (next != null) next.Dispose();
                next = null;
            }
            if (snapshot != null) snapshot.Dispose();
            snapshot = next;
            Invalidate();
        }

        public void HideOverlay()
        {
            Visible = false;
            if (snapshot != null) { snapshot.Dispose(); snapshot = null; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (snapshot != null) g.DrawImageUnscaled(snapshot, 0, 0);
            else ThemeColors.FillSoftGradient(g, ClientRectangle, ThemeColors.CBody, 35f);

            using (SolidBrush shade = new SolidBrush(Color.FromArgb(165, 0, 0, 0)))
                g.FillRectangle(shade, ClientRectangle);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            int width = Math.Min(ThemeColors.S(520), Math.Max(ThemeColors.S(180), Width - ThemeColors.S(48)));
            int height = ThemeColors.S(142);
            Rectangle card = new Rectangle((Width - width) / 2, (Height - height) / 2, width, height);
            using (GraphicsPath path = ThemeColors.RoundRect(card, ThemeColors.S(14)))
            {
                using (SolidBrush background = new SolidBrush(Color.FromArgb(245, 28, 29, 38))) g.FillPath(background, path);
                using (Pen border = new Pen(Color.FromArgb(110, 230, 230, 245))) g.DrawPath(border, path);
            }

            Rectangle titleRect = new Rectangle(card.X + ThemeColors.S(20), card.Y + ThemeColors.S(23),
                card.Width - ThemeColors.S(40), ThemeColors.S(28));
            Rectangle messageRect = new Rectangle(card.X + ThemeColors.S(28), card.Y + ThemeColors.S(59),
                card.Width - ThemeColors.S(56), ThemeColors.S(58));
            TextRenderer.DrawText(g, title, ThemeColors.F("Segoe UI Semibold", 12f), titleRect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, explanation, ThemeColors.F("Segoe UI", 9.5f), messageRect, Color.FromArgb(225, 226, 233),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && snapshot != null) { snapshot.Dispose(); snapshot = null; }
            base.Dispose(disposing);
        }
    }
}
