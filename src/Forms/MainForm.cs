using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using TharaThemeExplorer.Core;
using TharaThemeExplorer.UI;

namespace TharaThemeExplorer
{
    public partial class Program
    {
        public class OptionsDialog : Form
        {
            private static readonly int[] TypeSizes = new int[] { 8, 9, 10, 11, 12 };

            private HeaderPanel header;
            private ToolbarPanel toolbar;
            private Panel footer;

            private BodyPanel pageW10;
            private BodyPanel pageW11;
            private BodyPanel pageType;
            private UnavailableTabOverlay unavailableOverlay;

            private TabStrip tabs;
            private ThumbnailSelector thumbStyle;
            private ThumbnailSelector thumbMenu;
            private Segmented sizeSeg;
            private ToggleSwitch w7on10;
            private Card classicCard;

            private PillButton btnOK;
            private PillButton btnApply;
            private PillButton btnCancel;
            private PillButton btnRestart;
            private PillButton btnReload;

            private StatusDot statusDot;
            private Label statusText;
            private StatusBadge micaBadge;
            private RegOpt micaOption;
            private Card micaCard;
            private System.Windows.Forms.Timer uiStatusTimer;
            private FontPicker fontPick;
            private Label previewLbl;
            private ToggleSwitch[] typeToggles;

            private string initFace;
            private int initPt;
            private bool[] initTypeToggles;
            private int initStyle;
            private int initMenu;
            private bool initW7on10;

            private readonly List<RegOpt> opts10 = new List<RegOpt>();
            private readonly List<RegOpt> opts11 = new List<RegOpt>();
            private readonly List<NavOpt> nav10 = new List<NavOpt>();
            private readonly List<NavOpt> nav11 = new List<NavOpt>();
            private readonly List<ChoiceOpt> ch11 = new List<ChoiceOpt>();
            private readonly ToolTip tip = new ToolTip();
            private readonly List<CustomUiItem> customItems = new List<CustomUiItem>();
            private ColumnPanel customCol10;
            private ColumnPanel customCol11;
            private bool syncingCustom;

            private sealed class CustomUiItem
            {
                public CustomNavigation.Item Item;
                public bool IsNew;
                public bool Deleted;
                public ToggleSwitch Box10;
                public ToggleSwitch Box11;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= NativeMethods.WS_EX_COMPOSITED;
                    return cp;
                }
            }

            public OptionsDialog()
            {
                InitializeComponent();
            }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ThemeColors.ApplyWindowTheme(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (uiStatusTimer != null && !IsDisposed && !Disposing &&
                (m.Msg == (int)NativeMethods.WM_SETTINGCHANGE || m.Msg == (int)NativeMethods.WM_THEMECHANGED))
                RefreshSystemTheme();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (uiStatusTimer != null && !IsDisposed && !Disposing) RefreshSystemTheme();
        }

        private static void RecolorControls(Control control, Color[] oldColors, Color[] newColors, Color oldBlue, Color oldAccent)
        {
            if (control.IsDisposed) return;
            Color back = ThemeColors.RemapControlColor(control.BackColor, oldColors, newColors);
            Color fore = ThemeColors.RemapControlColor(control.ForeColor, oldColors, newColors);
            if (back.ToArgb() != control.BackColor.ToArgb()) control.BackColor = back;
            if (fore.ToArgb() != control.ForeColor.ToArgb()) control.ForeColor = fore;
            StatusDot dot = control as StatusDot;
            if (dot != null && dot.DotColor.ToArgb() == oldBlue.ToArgb()) dot.DotColor = ThemeColors.CBlue;
            StatusBadge badge = control as StatusBadge;
            if (badge != null && badge.BadgeColor.ToArgb() == oldAccent.ToArgb()) badge.BadgeColor = ThemeColors.Accent;
            foreach (Control child in control.Controls) RecolorControls(child, oldColors, newColors, oldBlue, oldAccent);
            control.Invalidate();
        }

        private void RefreshSystemTheme()
        {
            Color[] oldColors = ThemeColors.ThemeControlColors();
            Color oldBlue = ThemeColors.CBlue;
            Color oldAccent = ThemeColors.Accent;
            if (!ThemeColors.RefreshIfChanged()) return;

            Color[] newColors = ThemeColors.ThemeControlColors();
            List<Form> forms = new List<Form>();
            foreach (Form form in Application.OpenForms) forms.Add(form);
            foreach (Form form in forms)
            {
                if (form.IsDisposed) continue;
                RecolorControls(form, oldColors, newColors, oldBlue, oldAccent);
                if (form.IsHandleCreated) ThemeColors.ApplyWindowTheme(form.Handle);
                form.Invalidate(true);
            }
            if (unavailableOverlay != null && unavailableOverlay.Visible)
                unavailableOverlay.CapturePage(tabs.SelectedIndex == 0 ? pageW10 : pageW11);
        }

        private void InitializeComponent()
        {
            Text = ShellHelper.MyName;
            Icon = Icon.ExtractAssociatedIcon(ShellHelper.MyExe);
            AutoScaleMode = AutoScaleMode.None;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            Font = ThemeColors.F("Segoe UI", 9.5f);
            BackColor = ThemeColors.CBar;
            ForeColor = ThemeColors.CText;

            Screen screen = Screen.FromPoint(Cursor.Position);
            int formW = ThemeColors.S(1060);
            int headerH = ThemeColors.S(94);
            int toolbarH = ThemeColors.S(42);
            int footerH = ThemeColors.S(68);
            int formH = Math.Min(ThemeColors.S(900), screen.WorkingArea.Height - ThemeColors.S(40));
            ClientSize = new Size(formW, formH);

            Image logo = ThemeColors.LoadLogo();

            // 1. Header Panel
            header = new HeaderPanel
            {
                Left = 0,
                Top = 0,
                Width = formW,
                Height = headerH,
                Logo = logo
            };

            Label titleLbl = new Label
            {
                Text = ShellHelper.MyName,
                Font = ThemeColors.F("Segoe UI Semibold", 15f),
                ForeColor = ThemeColors.CText,
                BackColor = Color.Transparent,
                Left = ThemeColors.S(96),
                Top = ThemeColors.S(17),
                Height = ThemeColors.S(28),
                AutoSize = true
            };

            btnReload = new PillButton(ThemeColors.Glyph(59255), false, 26)
            {
                IconOnly = true,
                IconSize = 7.0f,
                Height = ThemeColors.S(26)
            };
            btnReload.Left = formW - ThemeColors.S(26) - btnReload.Width;
            btnReload.Top = (headerH - btnReload.Height) / 2;
            tip.SetToolTip(btnReload, "Reset to Windows defaults (loads them into the window; press Apply to use them)");

            StatusBadge verBadge = new StatusBadge(ShellHelper.WinVersionLabel(), ThemeColors.CGreen)
            {
                Neutral = true
            };
            verBadge.Left = btnReload.Left - ThemeColors.S(12) - verBadge.Width;
            verBadge.Top = (headerH - verBadge.Height) / 2;

            int titleW = TextRenderer.MeasureText(titleLbl.Text, titleLbl.Font).Width;
            LinkChip linkChip = new LinkChip("tharathemes.com", "https://www.tharathemes.com/")
            {
                Left = titleLbl.Left + titleW + ThemeColors.S(12),
                Top = titleLbl.Top + ThemeColors.S(3)
            };
            tip.SetToolTip(linkChip, "Open https://www.tharathemes.com/");

            Label subLbl = new Label
            {
                Text = "Customize Windows File Explorer navigation, layout, context menus and visuals",
                Font = ThemeColors.F("Segoe UI", 9f),
                ForeColor = ThemeColors.CSub,
                BackColor = Color.Transparent,
                Left = ThemeColors.S(96),
                Top = ThemeColors.S(54),
                Height = ThemeColors.S(20),
                Width = Math.Max(ThemeColors.S(200), verBadge.Left - ThemeColors.S(110))
            };

            header.Controls.Add(titleLbl);
            header.Controls.Add(linkChip);
            header.Controls.Add(subLbl);
            header.Controls.Add(verBadge);
            header.Controls.Add(btnReload);
            Controls.Add(header);

            // 2. Toolbar Panel
            toolbar = new ToolbarPanel
            {
                Left = 0,
                Top = headerH,
                Width = formW,
                Height = toolbarH
            };

            int gridMarginX = (formW - ThemeColors.S(1008)) / 2;

            tabs = new TabStrip(
                new string[] { "Windows 10", "Windows 11", "Typography" },
                new string[] { ThemeColors.Glyph(59575), ThemeColors.Glyph(59575), ThemeColors.Glyph(59602) }
            );
            tabs.Left = gridMarginX;
            tabs.Top = (toolbarH - tabs.Height) / 2;
            tabs.SelectedChanged += (o, e) =>
            {
                pageW10.Visible = (tabs.SelectedIndex == 0);
                pageW11.Visible = (tabs.SelectedIndex == 1);
                pageType.Visible = (tabs.SelectedIndex == 2);
                UpdateUnavailableOverlay();
            };
            toolbar.Controls.Add(tabs);
            Controls.Add(toolbar);

            // 3. Body Panels
            int bodyTop = headerH + toolbarH;
            int bodyH = formH - bodyTop - footerH;

            pageW10 = new BodyPanel { Left = 0, Top = bodyTop, Width = formW, Height = bodyH, AutoScroll = true };
            pageW11 = new BodyPanel { Left = 0, Top = bodyTop, Width = formW, Height = bodyH, AutoScroll = true };
            pageType = new BodyPanel { Left = 0, Top = bodyTop, Width = formW, Height = bodyH, AutoScroll = true, Visible = false };

            Controls.Add(pageW10);
            Controls.Add(pageW11);
            Controls.Add(pageType);
            unavailableOverlay = new UnavailableTabOverlay();
            Controls.Add(unavailableOverlay);

            // 4. Footer Panel
            footer = new Panel
            {
                Left = 0,
                Top = formH - footerH,
                Width = formW,
                Height = footerH,
                BackColor = ThemeColors.CBar
            };
            footer.Paint += (o, e) =>
            {
                using (Pen pen = new Pen(ThemeColors.CBorder))
                {
                    e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
                }
            };

            statusDot = new StatusDot
            {
                Left = gridMarginX,
                Top = (footerH - ThemeColors.S(14)) / 2,
                BackColor = ThemeColors.CBar
            };
            statusText = new Label
            {
                Text = "Ready",
                Font = ThemeColors.F("Segoe UI Semibold", 9f),
                ForeColor = ThemeColors.CText,
                BackColor = ThemeColors.CBar,
                Left = gridMarginX + ThemeColors.S(22),
                Top = (footerH - ThemeColors.S(20)) / 2,
                Width = ThemeColors.S(440),
                Height = ThemeColors.S(22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            footer.Controls.Add(statusDot);
            footer.Controls.Add(statusText);

            btnCancel = new PillButton("Cancel", false, 84) { TextOnly = true };
            btnApply = new PillButton("Apply", false, 88);
            btnOK = new PillButton("OK", true, 84);
            btnRestart = new PillButton("Restart Explorer", false, 160) { IconGlyph = ThemeColors.Glyph(59180) };

            int curBtnX = formW - gridMarginX;
            btnCancel.Left = curBtnX - btnCancel.Width;
            curBtnX = btnCancel.Left - ThemeColors.S(10);
            btnApply.Left = curBtnX - btnApply.Width;
            curBtnX = btnApply.Left - ThemeColors.S(10);
            btnOK.Left = curBtnX - btnOK.Width;
            curBtnX = btnOK.Left - ThemeColors.S(10);
            btnRestart.Left = curBtnX - btnRestart.Width;

            foreach (PillButton btn in new PillButton[] { btnRestart, btnOK, btnApply, btnCancel })
            {
                btn.Height = ThemeColors.S(38);
                btn.Top = (footerH - btn.Height) / 2;
                footer.Controls.Add(btn);
            }
            Controls.Add(footer);

            btnOK.Click += (o, e) => ApplyChanges(true);
            btnApply.Click += (o, e) => ApplyChanges(false);
            btnCancel.Click += (o, e) => Close();
            btnRestart.Click += (o, e) => RestartClicked();
            btnReload.Click += (o, e) => ResetToDefaults();

            try
            {
                foreach (CustomNavigation.Item item in CustomNavigation.Load())
                    customItems.Add(new CustomUiItem { Item = item });
            }
            catch (Exception ex) { SetStatus("Could not load custom navigation items: " + ex.Message, ThemeColors.CRed); }

            BuildWin11Page();
            BuildWin10Page();
            BuildTypePage();

            if (!ShellHelper.IsWin11) pageW11.Enabled = false;
            else pageW10.Enabled = false;

            tabs.SelectedIndex = ShellHelper.IsWin11 ? 1 : 0;
            pageW10.Visible = !ShellHelper.IsWin11;
            pageW11.Visible = ShellHelper.IsWin11;
            pageType.Visible = false;

            uiStatusTimer = new System.Windows.Forms.Timer { Interval = 1500 };
            uiStatusTimer.Tick += (s, e) =>
            {
                RefreshSystemTheme();
                if (ShellHelper.IsWin11) UpdateMicaStatus();
            };
            uiStatusTimer.Start();
            FormClosed += (s, e) => { uiStatusTimer.Stop(); uiStatusTimer.Dispose(); };
            UpdateDirty();
        }

        private void UpdateUnavailableOverlay()
        {
            if (unavailableOverlay == null || tabs == null) return;
            if (tabs.SelectedIndex == 0 && ShellHelper.IsWin11)
                unavailableOverlay.ShowFor(pageW10, "Windows 10", "Windows 11");
            else if (tabs.SelectedIndex == 1 && !ShellHelper.IsWin11)
                unavailableOverlay.ShowFor(pageW11, "Windows 11", "Windows 10");
            else unavailableOverlay.HideOverlay();
        }

        private ToggleSwitch CreateToggle(bool val)
        {
            ToggleSwitch sw = new ToggleSwitch { Checked = val };
            sw.CheckedChanged += (o, e) => UpdateDirty();
            return sw;
        }

        private void OptRow(Card card, List<RegOpt> list, RegOpt opt, string desc, Control below)
        {
            opt.Initial = opt.Read();
            opt.Box = CreateToggle(opt.Initial);
            list.Add(opt);
            card.Row(opt.Text, desc, opt.Box, below);
        }

        private void OptTile(ColumnPanel col, List<RegOpt> list, RegOpt opt)
        {
            opt.Initial = opt.Read();
            opt.Box = CreateToggle(opt.Initial);
            list.Add(opt);
            col.Tile(opt.Text, opt.Box);
        }

        private void NavTile(ColumnPanel col, List<NavOpt> list, int idx)
        {
            NavOpt opt = new NavOpt
            {
                Clsid = ShellHelper.NavItems[idx, 1],
                Initial = ShellHelper.IsNavHidden(ShellHelper.NavItems[idx, 1])
            };
            opt.Box = CreateToggle(opt.Initial);
            list.Add(opt);
            col.Tile("Hide " + ShellHelper.NavItems[idx, 0], opt.Box);
        }

        private void AddCustomHeader(ColumnPanel col)
        {
            PillButton add = new PillButton("Add more", true, 78)
            {
                Height = ThemeColors.S(26),
                KeepAccentOnHover = true,
                BackgroundInset = 2,
                Font = ThemeColors.F("Segoe UI Semibold", 8.5f),
                AccessibleName = "Add a custom file or folder"
            };
            tip.SetToolTip(add, "Add a custom file or folder");
            add.Click += (s, e) => AddCustomItem();
            col.SetHeaderButton(add);
        }

        private void AddCustomRow(ColumnPanel col, CustomUiItem entry, bool windows11)
        {
            bool value = entry.IsNew || entry.Item.InitialEnabled;
            ToggleSwitch toggle = CreateToggle(value);
            PillButton remove = new PillButton(ThemeColors.Glyph(0xE74D), false, 28)
            {
                Height = ThemeColors.S(28),
                IconOnly = true,
                IconSize = 9f,
                AccessibleName = "Remove " + entry.Item.Name
            };
            if (windows11) entry.Box11 = toggle; else entry.Box10 = toggle;
            toggle.CheckedChanged += (s, e) =>
            {
                if (syncingCustom) return;
                syncingCustom = true;
                try
                {
                    ToggleSwitch other = windows11 ? entry.Box10 : entry.Box11;
                    if (other != null && other.Checked != toggle.Checked) other.Checked = toggle.Checked;
                }
                finally { syncingCustom = false; }
                UpdateDirty();
            };
            remove.Click += (s, e) => RemoveCustomItem(entry);
            tip.SetToolTip(toggle, entry.Item.Path);
            tip.SetToolTip(remove, "Remove this entry after Apply. The original file or folder is not deleted.");
            string label = "Show " + (entry.Item.IsFile ? "file: " : "") + entry.Item.Name;
            col.Tile(label, toggle, remove);
        }

        private void AddCustomItem()
        {
            using (CustomPathDialog dialog = new CustomPathDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                CustomNavigation.Item item = dialog.SelectedItem;
                foreach (CustomUiItem existing in customItems)
                {
                    if (!existing.Deleted && string.Equals(existing.Item.Path.TrimEnd('\\', '/'), item.Path.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(this, "This file or folder is already in the Add Items list.", ShellHelper.MyName,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                CustomUiItem added = new CustomUiItem { Item = item, IsNew = true };
                customItems.Add(added);
                AddCustomRow(customCol10, added, false);
                AddCustomRow(customCol11, added, true);
                UpdateDirty();
            }
        }

        private void RemoveCustomItem(CustomUiItem entry)
        {
            if (entry.IsNew) customItems.Remove(entry);
            else entry.Deleted = true;
            if (entry.Box10 != null) { customCol10.RemoveTile(entry.Box10); entry.Box10 = null; }
            if (entry.Box11 != null) { customCol11.RemoveTile(entry.Box11); entry.Box11 = null; }
            UpdateDirty();
        }

        private bool CustomItemsChanged()
        {
            foreach (CustomUiItem entry in customItems)
            {
                if (entry.IsNew || entry.Deleted) return true;
                ToggleSwitch box = ShellHelper.IsWin11 ? entry.Box11 : entry.Box10;
                if (box != null && box.Checked != entry.Item.InitialEnabled) return true;
            }
            return false;
        }

        private void ApplyCustomItems()
        {
            List<CustomUiItem> removed = new List<CustomUiItem>();
            foreach (CustomUiItem entry in customItems)
            {
                try
                {
                    if (entry.Deleted)
                    {
                        if (!entry.IsNew) CustomNavigation.Delete(entry.Item);
                        removed.Add(entry);
                        continue;
                    }
                    ToggleSwitch box = ShellHelper.IsWin11 ? entry.Box11 : entry.Box10;
                    bool enabled = box != null && box.Checked;
                    if (entry.IsNew)
                    {
                        CustomNavigation.Add(entry.Item, enabled);
                        entry.IsNew = false;
                        entry.Item.InitialEnabled = enabled;
                    }
                    else if (enabled != entry.Item.InitialEnabled)
                    {
                        CustomNavigation.SetEnabled(entry.Item, enabled);
                        entry.Item.InitialEnabled = enabled;
                    }
                }
                catch (Exception ex) { ShellHelper.LogErr("Custom item " + entry.Item.Name, ex); }
            }
            foreach (CustomUiItem entry in removed) customItems.Remove(entry);
        }

        private void ChoiceTile(ColumnPanel col, ChoiceOpt opt, string tooltip)
        {
            opt.Initial = opt.ReadFn();
            opt.Seg = new Segmented(opt.Items, 54);
            opt.Seg.SelectedIndex = opt.Initial;
            opt.Seg.SelectedChanged += (s, e) => UpdateDirty();
            ch11.Add(opt);
            if (tooltip != null) tip.SetToolTip(opt.Seg, tooltip);
            col.Tile(opt.Text, opt.Seg);
        }

        private void PlaceRow(Panel page, ref int y, params Card[] cards)
        {
            int maxH = 0;
            foreach (Card c in cards)
            {
                if (c.Height > maxH) maxH = c.Height;
            }
            int totalCardsW = ThemeColors.S(496) * cards.Length + ThemeColors.S(16) * Math.Max(0, cards.Length - 1);
            int marginX = (page.Width - totalCardsW) / 2;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Height = maxH;
                cards[i].Left = marginX + i * (ThemeColors.S(496) + ThemeColors.S(16));
                cards[i].Top = y;
                page.Controls.Add(cards[i]);
            }
            y += maxH + ThemeColors.S(16);
        }

        private void PlaceWide(Panel page, ref int y, Card w)
        {
            int marginX = (page.Width - w.Width) / 2;
            w.Left = marginX;
            w.Top = y;
            page.Controls.Add(w);
            int previousHeight = w.Height;
            w.SizeChanged += (s, e) =>
            {
                int delta = w.Height - previousHeight;
                if (delta != 0)
                {
                    int previousBottom = w.Top + previousHeight;
                    foreach (Control sibling in page.Controls)
                        if (sibling != w && sibling.Top >= previousBottom) sibling.Top += delta;
                    previousHeight = w.Height;
                }
            };
            y += w.Height + ThemeColors.S(16);
        }

        private void BindResizableRow(Panel page, params Card[] cards)
        {
            int previousHeight = cards[0].Height;
            EventHandler update = (sender, e) =>
            {
                int nextHeight = 0;
                foreach (Card card in cards) nextHeight = Math.Max(nextHeight, card.ContentHeight);
                int delta = nextHeight - previousHeight;
                int previousBottom = cards[0].Top + previousHeight;
                foreach (Card card in cards) card.Height = nextHeight;
                if (delta != 0)
                {
                    foreach (Control sibling in page.Controls)
                    {
                        bool inRow = false;
                        foreach (Card card in cards) if (sibling == card) { inRow = true; break; }
                        if (!inRow && sibling.Top >= previousBottom) sibling.Top += delta;
                    }
                }
                previousHeight = nextHeight;
            };
            foreach (Card card in cards) card.ContentHeightChanged += update;
        }

        private StatusBadge MakeChip(string name, string okText, bool showWhenOk)
        {
            object val = Registry.GetValue(ShellHelper.MyKey, name, null);
            if (!(val is int)) return null;
            if ((int)val == 0)
            {
                return showWhenOk ? new StatusBadge("Status: " + okText, ThemeColors.Accent) : null;
            }
            return new StatusBadge("Status: Windows refused the change (0x" + ((int)val).ToString("X8") + ")", ThemeColors.CRed);
        }

        private void UpdateMicaStatus()
        {
            if (micaBadge == null || micaOption == null || micaOption.Box == null || micaCard == null) return;
            object result = null;
            try { result = Registry.GetValue(ShellHelper.MyKey, "MicaLastResult", null); }
            catch { }
            string text = ShellHelper.MicaErrorStatus(micaOption.Box.Checked, micaOption.Initial, ShellHelper.WinBuild, result);
            if (text != null)
            {
                micaBadge.Text = text;
                micaBadge.BadgeColor = ThemeColors.CRed;
                micaBadge.Width = TextRenderer.MeasureText(text, ThemeColors.F("Segoe UI Semibold", 8f)).Width + ThemeColors.S(32);
                micaBadge.Invalidate();
            }
            micaCard.SetOptionalStatusVisible(text != null);
        }

        private Card CreateThisPcCard(List<RegOpt> list)
        {
            Card card = new Card(ThemeColors.Glyph(59248), "This PC", "Special user directories & virtual namespaces", ThemeColors.Accent, 496);
            OptRow(card, list, ShellHelper.ThisPcOpt(), "Desktop, Documents, Downloads, Music, Pictures and Videos. Applies to all users and asks for administrator permission.", null);
            card.Free(new FolderBox(), ThemeColors.S(96));
            return card;
        }

        private void BuildWin11Page()
        {
            int curY = ThemeColors.S(16);

            // Explorer Style Card
            Card card1 = new Card(ThemeColors.Glyph(59575), "File Explorer style", "Ribbon bar and window chrome configuration", ThemeColors.Accent, 496);
            bool win10Exp, win10Ctx;
            ShellHelper.ReadStyleState(out win10Exp, out win10Ctx);
            bool ribbonOverride = win10Exp && ShellHelper.RibbonOverrideActive();
            initStyle = !ribbonOverride ? (!win10Exp ? 2 : 1) : 0;

            thumbStyle = new ThumbnailSelector();
            thumbStyle.AddItem("Windows 7", "Command bar", "opt_w7_exp.png");
            thumbStyle.AddItem("Windows 10", "Ribbon", "opt_w10_exp.png");
            thumbStyle.AddItem("Windows 11", "Modern bar", "opt_w11_exp.png");
            thumbStyle.SelectedIndex = initStyle;
            thumbStyle.SelectedChanged += (o, e) =>
            {
                UpdateDirty();
                UpdateClassic();
            };
            card1.Section("Explorer layout", thumbStyle, ThemeColors.S(186));
            card1.Note("Windows 7 uses classic command bar; Windows 11 uses modern lightweight bar.");
            micaOption = ShellHelper.MicaOpt();
            micaBadge = new StatusBadge("Status: Last Mica operation failed", ThemeColors.CRed);
            micaCard = card1;
            OptRow(card1, opts11, micaOption, "Increases window contrast and snappiness.", null);
            card1.AddOptionalStatus(micaBadge);
            micaOption.Box.CheckedChanged += (o, e) => UpdateMicaStatus();
            UpdateMicaStatus();

            // Context Menu Card
            Card card2 = new Card(ThemeColors.Glyph(59645), "Right-click menu", "Context menu style and shell extensions", ThemeColors.Accent, 496);
            initMenu = !win10Ctx ? 1 : 0;
            thumbMenu = new ThumbnailSelector();
            thumbMenu.AddItem("Windows 10", "Classic full menu", "opt_w10_menu.png");
            thumbMenu.AddItem("Windows 11", "Modern Fluent acrylic", "opt_w11_menu.png");
            thumbMenu.SelectedIndex = initMenu;
            thumbMenu.SelectedChanged += (o, e) => UpdateDirty();
            card2.Section("Menu style", thumbMenu, ThemeColors.S(186));
            card2.Note("Windows 10 shows full context menu instantly without delay.");
            OptRow(card2, opts11, ShellHelper.TakeOwnershipOpt(), "Adds quick Take Ownership entry for files & folders.", null);

            PlaceRow(pageW11, ref curY, card1, card2);
            BindResizableRow(pageW11, card1, card2);

            // Navigation Pane Columns Card
            ColumnsCard navCard = new ColumnsCard(ThemeColors.Glyph(59136), "Navigation pane", "Control side tree view visibility, pinned special folders, and order", ThemeColors.Accent, 1008);
            int colW = 314;

            ColumnPanel colHide = new ColumnPanel(ThemeColors.Glyph(60698), "Hide Items", colW);
            NavTile(colHide, nav11, 0);
            NavTile(colHide, nav11, 1);
            NavTile(colHide, nav11, 2);
            NavTile(colHide, nav11, 3);

            ColumnPanel colAdd = new ColumnPanel(ThemeColors.Glyph(59152), "Add Items", colW);
            customCol11 = colAdd;
            AddCustomHeader(colAdd);
            foreach (RegOpt opt in ShellHelper.NavAddOpts()) OptTile(colAdd, opts11, opt);
            foreach (CustomUiItem item in customItems) AddCustomRow(colAdd, item, true);

            ColumnPanel colOrder = new ColumnPanel(ThemeColors.Glyph(59595), "Order", colW);
            bool showAll = ShellHelper.ReadDword(ShellHelper.ADVF, "NavPaneShowAllFolders", 0) == 1;
            string orderTip = showAll ? "No effect while \"Show all folders\" is on in Folder Options." : null;
            ChoiceTile(colOrder, ShellHelper.PosOpt("Home position", "{f874310e-b6b7-47dc-bc84-b9e6b38f5903}", 72), orderTip);
            ChoiceTile(colOrder, ShellHelper.PosOpt("Gallery position", "{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", 73), orderTip);

            navCard.AddColumn(colHide);
            navCard.AddColumn(colAdd);
            navCard.AddColumn(colOrder);
            PlaceWide(pageW11, ref curY, navCard);

            // Classic Explorer & This PC
            classicCard = new Card(ThemeColors.Glyph(59332), "Windows 7 / 10 style Explorer only", "Settings valid when Classic layout is activated", ThemeColors.Accent, 496);
            OptRow(classicCard, opts11, ShellHelper.StatusBarOpt(), "Display item count & size indicators in bottom footer.", null);
            RegOpt[] tweaks = ShellHelper.TweakOpts();
            OptRow(classicCard, opts11, tweaks[0], "Removes path/title text in header titlebar.", MakeChip("ThemeLastResult", "caption options applied", false));
            OptRow(classicCard, opts11, tweaks[1], "Removes folder icon from the window titlebar.", null);

            PlaceRow(pageW11, ref curY, classicCard, CreateThisPcCard(opts11));
            UpdateClassic();
        }

        private void BuildWin10Page()
        {
            int curY = ThemeColors.S(16);

            Card card1 = new Card(ThemeColors.Glyph(59575), "File Explorer style", "Ribbon bar and window chrome configuration", ThemeColors.Accent, 496);
            initW7on10 = ShellHelper.RibbonOverrideActive();
            w7on10 = CreateToggle(initW7on10);
            card1.Row("Windows 7 style", "Replaces the Windows 10 ribbon with the older command bar.", w7on10, null);

            Card card2 = new Card(ThemeColors.Glyph(59645), "Right-click menu", "Context menu style and shell extensions", ThemeColors.Accent, 496);
            OptRow(card2, opts10, ShellHelper.TakeOwnershipOpt(), "Adds quick Take Ownership entry for files & folders.", null);

            PlaceRow(pageW10, ref curY, card1, card2);

            ColumnsCard navCard = new ColumnsCard(ThemeColors.Glyph(59136), "Navigation pane", "Control side tree view visibility and pinned special folders", ThemeColors.Accent, 1008);
            int colW = 480;
            ColumnPanel colHide = new ColumnPanel(ThemeColors.Glyph(60698), "Hide Items", colW);
            NavTile(colHide, nav10, 2);
            NavTile(colHide, nav10, 3);

            ColumnPanel colAdd = new ColumnPanel(ThemeColors.Glyph(59152), "Add Items", colW);
            customCol10 = colAdd;
            AddCustomHeader(colAdd);
            foreach (RegOpt opt in ShellHelper.NavAddOpts()) OptTile(colAdd, opts10, opt);
            foreach (CustomUiItem item in customItems) AddCustomRow(colAdd, item, false);

            navCard.AddColumn(colHide);
            navCard.AddColumn(colAdd);
            PlaceWide(pageW10, ref curY, navCard);

            Card card3 = new Card(ThemeColors.Glyph(59332), "Explorer windows", "Title bar text and icon of Explorer windows", ThemeColors.Accent, 496);
            RegOpt[] tweaks = ShellHelper.TweakOpts();
            OptRow(card3, opts10, tweaks[0], "Removes path/title text in header titlebar.", MakeChip("ThemeLastResult", "caption options applied", false));
            OptRow(card3, opts10, tweaks[1], "Removes folder icon from the window titlebar.", null);

            PlaceRow(pageW10, ref curY, card3, CreateThisPcCard(opts10));
        }

        private int CurrentSizePt()
        {
            return TypeSizes[sizeSeg.SelectedIndex];
        }

        private static int IndexOfPt(int pt)
        {
            return Math.Max(0, Math.Min(TypeSizes.Length - 1, pt - TypeSizes[0]));
        }

        private bool TypoChanged()
        {
            if (fontPick == null || sizeSeg == null) return false;
            if (fontPick.Value != initFace || sizeSeg.SelectedIndex != IndexOfPt(initPt)) return true;
            if (initTypeToggles != null && typeToggles != null)
                for (int i = 0; i < typeToggles.Length; i++)
                    if (typeToggles[i].Checked != initTypeToggles[i]) return true;
            return false;
        }

        private void UpdatePreview()
        {
            if (previewLbl == null) return;
            try
            {
                previewLbl.Font = ThemeColors.F(fontPick.Value, CurrentSizePt());
            }
            catch
            {
                previewLbl.Font = ThemeColors.F("Segoe UI", CurrentSizePt());
            }
            previewLbl.Text = "Aa Bb Cc   The quick brown fox jumps over the lazy dog.\n0123456789   ( ) { } [ ]   @ # & %";
            previewLbl.Invalidate();
        }

        private void BuildTypePage()
        {
            int curY = ThemeColors.S(16);
            string face;
            int pt;
            TypographyHelper.GetCurrentFont(out face, out pt);
            initFace = face;
            initPt = pt;

            Card card1 = new Card(ThemeColors.Glyph(59602), "Font", "Choose any installed font and size", ThemeColors.Accent, 496);
            fontPick = new FontPicker { Value = face };
            fontPick.ValueChanged += (o, e) =>
            {
                UpdatePreview();
                UpdateDirty();
            };
            card1.Row("Font", null, fontPick, null);

            sizeSeg = new Segmented(new string[] { "8", "9", "10", "11", "12" }, 42);
            sizeSeg.SelectedIndex = IndexOfPt(pt);
            sizeSeg.SelectedChanged += (o, e) =>
            {
                UpdatePreview();
                UpdateDirty();
            };
            card1.Row("Size (pt)", null, sizeSeg, null);

            previewLbl = new Label
            {
                ForeColor = ThemeColors.CText,
                BackColor = ThemeColors.CCard,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card1.Free(previewLbl, ThemeColors.S(64));

            Label hintLbl = new Label
            {
                Text = "Windows 11's newer parts (Start menu, taskbar, Settings) keep their own font. To go back, use Reset to defaults, then Apply.",
                Font = ThemeColors.F("Segoe UI", 8.25f),
                ForeColor = ThemeColors.CSub,
                BackColor = ThemeColors.CCard
            };
            card1.Free(hintLbl, ThemeColors.S(34));

            Card card2 = new Card(ThemeColors.Glyph(59249), "Use the font for", "Choose where the font is used", ThemeColors.Accent, 496);
            string[] names = new string[] { "Icons and file names", "Menus", "Message boxes", "Window titles", "Status bars" };
            string[] descs = new string[] { "File names in Explorer and labels on the desktop.", null, null, null, null };
            typeToggles = new ToggleSwitch[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                typeToggles[i] = new ToggleSwitch { Checked = true };
                typeToggles[i].CheckedChanged += (o, e) => UpdateDirty();
                card2.Row(names[i], descs[i], typeToggles[i], null);
            }
            initTypeToggles = new bool[names.Length];
            for (int i = 0; i < names.Length; i++) initTypeToggles[i] = typeToggles[i].Checked;

            PlaceRow(pageType, ref curY, card1, card2);
            UpdatePreview();
        }

        private void UpdateClassic()
        {
            if (classicCard != null && thumbStyle != null)
            {
                classicCard.Enabled = (thumbStyle.SelectedIndex != 2);
            }
        }

        private List<RegOpt> ActiveOpts { get { return ShellHelper.IsWin11 ? opts11 : opts10; } }
        private List<NavOpt> ActiveNav { get { return ShellHelper.IsWin11 ? nav11 : nav10; } }

        private bool HasChanges()
        {
            if (TypoChanged()) return true;
            if (CustomItemsChanged()) return true;

            if (ShellHelper.IsWin11)
            {
                if (thumbStyle.SelectedIndex != initStyle || thumbMenu.SelectedIndex != initMenu) return true;
                foreach (ChoiceOpt c in ch11)
                {
                    if (c.Seg.SelectedIndex != c.Initial) return true;
                }
            }
            else
            {
                if (w7on10.Checked != initW7on10) return true;
            }

            foreach (RegOpt r in ActiveOpts)
            {
                if (r.Box.Checked != r.Initial) return true;
            }
            foreach (NavOpt n in ActiveNav)
            {
                if (n.Box.Checked != n.Initial) return true;
            }

            return false;
        }

        private void UpdateDirty()
        {
            if (btnApply != null)
            {
                btnApply.Enabled = HasChanges();
                btnApply.Invalidate();
            }
        }

        private void SetStatus(string msg, Color color)
        {
            statusDot.DotColor = color;
            statusDot.Invalidate();
            statusText.Text = msg;
        }

        private void ResetToDefaults()
        {
            if (ShellHelper.IsWin11)
            {
                thumbStyle.SelectedIndex = 2;
                thumbMenu.SelectedIndex = 1;
                foreach (ChoiceOpt c in ch11) c.Seg.SelectedIndex = 0;
            }
            else
            {
                w7on10.Checked = false;
            }

            foreach (RegOpt r in ActiveOpts) r.Box.Checked = r.DefaultOn;
            foreach (NavOpt n in ActiveNav) n.Box.Checked = false;

            fontPick.Value = "Segoe UI";
            sizeSeg.SelectedIndex = IndexOfPt(9);
            foreach (ToggleSwitch sw in typeToggles) sw.Checked = true;

            UpdateDirty();
            if (HasChanges())
            {
                SetStatus("Windows default settings loaded. Press Apply to use them.", ThemeColors.CBlue);
            }
            else
            {
                SetStatus("Everything already matches the Windows default settings.", ThemeColors.CBlue);
            }
        }

        private void RestartClicked()
        {
            DialogResult dr = MessageBox.Show(this,
                "Restart Windows Explorer now?\n\nThe taskbar and desktop reload for a moment. Known Explorer locations are reopened where possible; tabs may not be restored.",
                ShellHelper.MyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                ShellHelper.RunLog.Clear();
                ShellHelper.RefreshExplorer(true);
                ShellHelper.SaveLog();
                SetStatus("Explorer was restarted.", Color.FromArgb(52, 199, 89));
            }
            catch (Exception ex)
            {
                SetStatus("Could not restart Explorer: " + ex.Message, ThemeColors.CRed);
            }
            Cursor = Cursors.Default;
        }

        private void ApplyChanges(bool close)
        {
            if (!HasChanges())
            {
                if (close) Close();
                return;
            }

            ShellHelper.RunLog.Clear();
            bool needRestart = false;
            bool refreshWindows = false;
            bool styleChanged = false;
            bool styleFailed = false;
            bool typographyFailed = false;
            HashSet<RegOpt> failedOptions = new HashSet<RegOpt>();
            HashSet<NavOpt> failedNav = new HashSet<NavOpt>();
            HashSet<ChoiceOpt> failedChoices = new HashSet<ChoiceOpt>();
            int styleIdx = ShellHelper.IsWin11 ? thumbStyle.SelectedIndex : -1;

            if (ShellHelper.IsWin11)
            {
                bool win10Exp, win10Ctx;
                ShellHelper.ReadStyleState(out win10Exp, out win10Ctx);
                bool ribbonOverride = win10Exp && ShellHelper.RibbonOverrideActive();
                bool wantWin10Exp = (styleIdx <= 1);
                bool wantRibbon = (styleIdx == 0);
                bool wantWin10Ctx = (thumbMenu.SelectedIndex == 0);

                if (wantWin10Exp != win10Exp || wantRibbon != ribbonOverride || wantWin10Ctx != win10Ctx)
                {
                    needRestart = styleChanged = true;
                }
            }
            else
            {
                if (w7on10.Checked != ShellHelper.RibbonOverrideActive()) needRestart = styleChanged = true;
            }

            foreach (RegOpt r in ActiveOpts)
            {
                if (r.Box.Checked != r.Initial && r.RequiresExplorerRestart)
                    needRestart = true;
                if (r.Box.Checked != r.Initial && (r.NeedsRefresh || (!r.Box.Checked && r.OffNeedsRefresh)))
                {
                    refreshWindows = true;
                }
            }
            foreach (NavOpt n in ActiveNav)
            {
                if (n.Box.Checked != n.Initial) refreshWindows = true;
            }
            if (ShellHelper.IsWin11)
            {
                foreach (ChoiceOpt c in ch11)
                {
                    if (c.Seg.SelectedIndex != c.Initial && c.NeedsRefresh) refreshWindows = true;
                }
            }
            if (TypoChanged()) refreshWindows = true;
            if (CustomItemsChanged()) refreshWindows = true;

            bool restartNow = false;
            if (needRestart)
            {
                DialogResult dr = MessageBox.Show(this,
                    "Some changes may need Windows Explorer to restart. Known locations reopen where possible; tabs may not be restored.\n\nYes: apply and restart Explorer now\nNo: apply and reopen Explorer windows only\nCancel: go back",
                    ShellHelper.MyName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel) return;
                restartNow = (dr == DialogResult.Yes);
            }

            Cursor = Cursors.WaitCursor;
            ShellHelper.Log(string.Format("Apply pressed: needRestart={0}, restartNow={1}, refresh={2}, win11={3}",
                needRestart, restartNow, refreshWindows, ShellHelper.IsWin11));

            if (styleChanged)
            {
                int beforeStyle = ShellHelper.RunLog.Count;
                try
                {
                    if (ShellHelper.IsWin11)
                    {
                        bool e10 = (styleIdx <= 1);
                        bool e11 = (styleIdx == 2);
                        bool c10 = (thumbMenu.SelectedIndex == 0);
                        bool c11 = (thumbMenu.SelectedIndex == 1);
                        ShellHelper.ApplyRibbonOverride(styleIdx == 0);
                        ShellHelper.SaveSettings(e10, e11, c10, c11, false, false);
                    }
                    else
                    {
                        ShellHelper.ApplyRibbonOverride(w7on10.Checked);
                        ShellHelper.SaveSettings(false, false, false, false, false, false);
                    }
                }
                catch (Exception ex) { styleFailed = true; ShellHelper.LogErr("Explorer style", ex); }
                for (int i = beforeStyle; i < ShellHelper.RunLog.Count; i++)
                    if (ShellHelper.RunLog[i].Contains("FAILED")) styleFailed = true;
            }

            foreach (RegOpt r in ActiveOpts)
            {
                if (r.Box.Checked != r.Initial)
                {
                    try
                    {
                        if (r == micaOption) ShellHelper.DeleteDword(ShellHelper.MyKey, "MicaLastResult");
                        r.Write(r.Box.Checked);
                    }
                    catch (Exception ex) { failedOptions.Add(r); ShellHelper.LogErr(r.Text, ex); }
                }
            }
            ApplyCustomItems();
            foreach (NavOpt n in ActiveNav)
            {
                if (n.Box.Checked != n.Initial)
                {
                    try { ShellHelper.SetNavHidden(n.Clsid, n.Box.Checked); }
                    catch (Exception ex) { failedNav.Add(n); ShellHelper.LogErr("Navigation pane", ex); }
                }
            }
            if (ShellHelper.IsWin11)
            {
                foreach (ChoiceOpt c in ch11)
                {
                    if (c.Seg.SelectedIndex != c.Initial)
                    {
                        try { c.WriteFn(c.Seg.SelectedIndex); }
                        catch (Exception ex) { failedChoices.Add(c); ShellHelper.LogErr(c.Text, ex); }
                    }
                }
            }

            if (TypoChanged())
            {
                bool anyChecked = false;
                foreach (ToggleSwitch sw in typeToggles)
                {
                    if (sw.Checked) anyChecked = true;
                }
                if (!anyChecked)
                {
                    typographyFailed = true;
                    ShellHelper.Log("FAILED  Typography: choose at least one place to use the font.");
                }
                else
                {
                    try
                    {
                        TypographyHelper.ApplyTypography(fontPick.Value, CurrentSizePt(),
                            typeToggles[0].Checked, typeToggles[1].Checked, typeToggles[2].Checked,
                            typeToggles[3].Checked, typeToggles[4].Checked);
                        ShellHelper.Log(string.Format("Font '{0}' {1} pt applied", fontPick.Value, CurrentSizePt()));
                    }
                    catch (Exception ex)
                    {
                        typographyFailed = true;
                        ShellHelper.LogErr("Typography", ex);
                    }
                }
            }

            bool needWatcher = false;
            foreach (RegOpt r in ActiveOpts)
            {
                if (r.Key == ShellHelper.MyKey && r.Read()) needWatcher = true;
            }
            Watcher.SetWatcherRun(needWatcher);
            if (needWatcher) Watcher.StartWatcher();

            // Apply every setting before refreshing or restarting Explorer.
            if (refreshWindows || restartNow)
            {
                try { ShellHelper.RefreshExplorer(restartNow); }
                catch (Exception ex) { ShellHelper.LogErr("Refreshing Explorer", ex); }
            }

            ShellHelper.SaveLog();

            if (!typographyFailed)
            {
                initFace = fontPick.Value;
                initPt = CurrentSizePt();
                for (int i = 0; i < typeToggles.Length; i++) initTypeToggles[i] = typeToggles[i].Checked;
            }
            foreach (RegOpt r in ActiveOpts)
                if (!failedOptions.Contains(r)) r.Initial = r.Box.Checked;
            UpdateMicaStatus();
            foreach (NavOpt n in ActiveNav)
                if (!failedNav.Contains(n)) n.Initial = n.Box.Checked;
            if (ShellHelper.IsWin11)
            {
                foreach (ChoiceOpt c in ch11)
                    if (!failedChoices.Contains(c)) c.Initial = c.Seg.SelectedIndex;
                if (!styleFailed)
                {
                    initStyle = thumbStyle.SelectedIndex;
                    initMenu = thumbMenu.SelectedIndex;
                }
            }
            else
            {
                if (!styleFailed) initW7on10 = w7on10.Checked;
            }

            Cursor = Cursors.Default;
            UpdateDirty();

            string errors = string.Empty;
            foreach (string entry in ShellHelper.RunLog)
            {
                if (entry.Contains("FAILED")) errors += entry.Substring(14) + "\n";
            }

            if (errors.Length > 0)
            {
                SetStatus("Some changes could not be applied. See details.", Color.FromArgb(196, 43, 28));
                MessageBox.Show(this, "Some changes could not be applied:\n\n" + errors + "\nDetails: " + ShellHelper.LogPath(), ShellHelper.MyName);
            }
            else if (needRestart && !restartNow)
            {
                SetStatus("Applied. Restart Explorer to finish applying the changes.", Color.FromArgb(157, 93, 0));
            }
            else if (restartNow)
            {
                SetStatus("Applied. Explorer was restarted.", Color.FromArgb(16, 137, 62));
            }
            else if (refreshWindows)
            {
                SetStatus("Applied. Explorer windows were closed and reopened.", Color.FromArgb(16, 137, 62));
            }
            else
            {
                SetStatus("Applied.", Color.FromArgb(16, 137, 62));
            }

            if (close && errors.Length == 0) Close();
        }
    }
}
}
