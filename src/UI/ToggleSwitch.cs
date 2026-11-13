using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TharaThemeExplorer.Core;

namespace TharaThemeExplorer.UI
{
    public class ToggleSwitch : Control
    {
        private bool isOn;
        private bool isHovered;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return isOn; }
            set
            {
                if (isOn != value)
                {
                    isOn = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            Size = new Size(ThemeColors.S(40), ThemeColors.S(22));
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (Enabled) Checked = !isOn;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && Enabled)
            {
                Checked = !isOn;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ThemeColors.RoundRect(rect, rect.Height / 2))
            {
                if (isOn)
                {
                    Color c1 = ThemeColors.Dim(!ThemeColors.Dark ? ThemeColors.CPurple : ThemeColors.CPink, Enabled);
                    Color c2 = ThemeColors.Dim(!ThemeColors.Dark ? ThemeColors.CPink : ThemeColors.CPurple, Enabled);
                    using (LinearGradientBrush brush = new LinearGradientBrush(rect, c1, c2, 0f))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    Color track = ThemeColors.Dim(!isHovered ? ThemeColors.CTrack : ThemeColors.Tint(ThemeColors.CText, !ThemeColors.Dark ? 40 : 55), Enabled);
                    using (SolidBrush brush = new SolidBrush(track))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            int knobSize = Height - ThemeColors.S(8);
            int knobX = !isOn ? ThemeColors.S(4) : (Width - knobSize - ThemeColors.S(4) - 1);
            int knobY = (Height - knobSize) / 2;

            Color knobColor = ThemeColors.Dim(!isOn ? ThemeColors.CKnobOff : Color.White, Enabled);
            using (SolidBrush knobBrush = new SolidBrush(knobColor))
            {
                g.FillEllipse(knobBrush, knobX, knobY, knobSize, knobSize);
            }

            if (Focused && Enabled)
            {
                using (Pen focusPen = new Pen(ThemeColors.Tint(ThemeColors.CSub, 160)))
                {
                    focusPen.DashStyle = DashStyle.Dot;
                    g.DrawPath(focusPen, ThemeColors.RoundRect(new Rectangle(-1, -1, Width + 1, Height + 1), Height / 2));
                }
            }
        }
    }
}
