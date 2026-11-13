using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class FolderBox : Control
    {
        private readonly string[] names = new string[] { "Desktop", "Downloads", "Documents", "Pictures", "Music", "Videos" };

        public FolderBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = ThemeColors.CCard;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(ThemeColors.CCard);
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ThemeColors.RoundRect(rect, ThemeColors.S(10)))
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

            Font headerFont = ThemeColors.F("Segoe UI Semibold", 7.5f);
            TextRenderer.DrawText(g, "INCLUDED VIRTUAL SHELL FOLDERS", headerFont,
                new Point(ThemeColors.S(14), ThemeColors.S(11)), ThemeColors.CSub, TextFormatFlags.NoPadding);

            int colW = (Width - ThemeColors.S(28)) / 3;
            Font itemFont = ThemeColors.F("Segoe UI", 8.5f);

            for (int i = 0; i < names.Length; i++)
            {
                int itemX = ThemeColors.S(14) + (i % 3) * colW;
                int itemY = ThemeColors.S(38) + (i / 3) * ThemeColors.S(24);

                using (SolidBrush dotBrush = new SolidBrush(ThemeColors.Accent))
                {
                    g.FillEllipse(dotBrush, itemX, itemY + ThemeColors.S(5), ThemeColors.S(6), ThemeColors.S(6));
                }
                TextRenderer.DrawText(g, names[i], itemFont, new Point(itemX + ThemeColors.S(14), itemY), ThemeColors.CText, TextFormatFlags.NoPadding);
            }
        }
    }
}
