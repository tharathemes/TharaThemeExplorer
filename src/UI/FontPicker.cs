using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class FontPopup : Form
    {
        private readonly ListBox lb;
        private readonly Dictionary<string, Font> cache = new Dictionary<string, Font>();

        public event Action<string> Picked;

        public FontPopup(string[] names, string current, int widthPx)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = ThemeColors.CBorder;
            Padding = new Padding(1);
            Size = new Size(widthPx, ThemeColors.S(28) * 9 + 2);

            lb = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = ThemeColors.S(28),
                BackColor = ThemeColors.CCard,
                ForeColor = ThemeColors.CText,
                IntegralHeight = false
            };

            foreach (string name in names) lb.Items.Add(name);

            int idx = lb.Items.IndexOf(current);
            if (idx >= 0)
            {
                lb.SelectedIndex = idx;
                lb.TopIndex = Math.Max(0, idx - 3);
            }

            lb.DrawItem += DrawFontItem;
            lb.MouseUp += (o, e) =>
            {
                int itemIdx = lb.IndexFromPoint(e.Location);
                if (itemIdx >= 0 && itemIdx < lb.Items.Count)
                {
                    Pick((string)lb.Items[itemIdx]);
                }
            };
            lb.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Return && lb.SelectedIndex >= 0)
                {
                    Pick((string)lb.SelectedItem);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };

            Controls.Add(lb);
            Deactivate += (o, e) => Close();
        }

        private void Pick(string name)
        {
            if (Picked != null) Picked(name);
            Close();
        }

        private Font FontFor(string name)
        {
            Font f;
            if (!cache.TryGetValue(name, out f))
            {
                try { f = new Font(name, 10.5f); }
                catch { f = new Font("Segoe UI", 10.5f); }
                cache[name] = f;
            }
            return f;
        }

        private void DrawFontItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool selected = (e.State & DrawItemState.Selected) != DrawItemState.None;
            Color bg = !selected ? ThemeColors.CCard : ThemeColors.Tint(ThemeColors.Accent, !ThemeColors.Dark ? 50 : 70);

            using (SolidBrush brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = (string)lb.Items[e.Index];
            Rectangle rect = new Rectangle(e.Bounds.X + ThemeColors.S(10), e.Bounds.Y, e.Bounds.Width - ThemeColors.S(14), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, FontFor(text), rect, ThemeColors.CText,
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (Font f in cache.Values) f.Dispose();
            cache.Clear();
            base.OnFormClosed(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            lb.Focus();
        }
    }

    public class FontPicker : Control
    {
        private string val = "Segoe UI";
        private static string[] cachedNames;

        public event EventHandler ValueChanged;

        public string Value
        {
            get { return val; }
            set
            {
                if (val != value)
                {
                    val = value;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public FontPicker()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            Size = new Size(ThemeColors.S(240), ThemeColors.S(32));
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        private static string[] GetFontNames()
        {
            if (cachedNames == null)
            {
                List<string> list = new List<string>();
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    foreach (FontFamily ff in installed.Families) list.Add(ff.Name);
                }
                list.Sort(StringComparer.CurrentCultureIgnoreCase);
                cachedNames = list.ToArray();
            }
            return cachedNames;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (!Enabled || Parent == null) return;

            FontPopup popup = new FontPopup(GetFontNames(), val, Width);
            Point pt = Parent.PointToScreen(new Point(Left, Bottom + ThemeColors.S(4)));
            Rectangle work = Screen.FromControl(this).WorkingArea;
            if (pt.Y + popup.Height > work.Bottom)
            {
                pt = Parent.PointToScreen(new Point(Left, Top - popup.Height - ThemeColors.S(4)));
            }
            popup.Location = pt;
            popup.Picked += v => Value = v;
            popup.Show(FindForm());
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
            using (GraphicsPath path = ThemeColors.RoundRect(rect, ThemeColors.S(8)))
            {
                using (SolidBrush brush = new SolidBrush(ThemeColors.Dim(ThemeColors.CTrack, Enabled)))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(ThemeColors.CBorder))
                {
                    g.DrawPath(pen, path);
                }
            }

            Font font = ThemeColors.F(val, 9.5f);
            TextRenderer.DrawText(g, val, font, new Rectangle(ThemeColors.S(12), 0, Width - ThemeColors.S(40), Height),
                ThemeColors.Dim(ThemeColors.CText, Enabled),
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, ThemeColors.Glyph(59149), ThemeColors.IconFont(8f),
                new Rectangle(Width - ThemeColors.S(28), 0, ThemeColors.S(20), Height),
                ThemeColors.Dim(ThemeColors.CSub, Enabled),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
