using System;
using System.Drawing;
using System.Windows.Forms;
using TharaThemeExplorer.Core;
using TharaThemeExplorer.UI;

namespace TharaThemeExplorer
{
    internal sealed class CustomPathDialog : Form
    {
        private readonly TextBox pathBox;
        private readonly Label message;
        public CustomNavigation.Item SelectedItem { get; private set; }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ThemeColors.ApplyWindowTheme(Handle);
        }

        public CustomPathDialog()
        {
            Text = "Add navigation item";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;
            ClientSize = new Size(ThemeColors.S(520), ThemeColors.S(190));
            BackColor = ThemeColors.CCard;
            ForeColor = ThemeColors.CText;
            Font = ThemeColors.F("Segoe UI", 9f);

            Label prompt = new Label { Text = "Enter the full path of a file or folder:", Left = ThemeColors.S(16), Top = ThemeColors.S(14), Width = ThemeColors.S(488), Height = ThemeColors.S(22), ForeColor = ThemeColors.CText };
            pathBox = new TextBox { Left = ThemeColors.S(16), Top = ThemeColors.S(39), Width = ThemeColors.S(488), Height = ThemeColors.S(28), BackColor = ThemeColors.CTile, ForeColor = ThemeColors.CText, BorderStyle = BorderStyle.FixedSingle };
            Label note = new Label { Text = "Files appear as a folder containing a shortcut. Click Apply to update Explorer.", Left = ThemeColors.S(16), Top = ThemeColors.S(72), Width = ThemeColors.S(488), Height = ThemeColors.S(29), ForeColor = ThemeColors.CSub };
            message = new Label { Left = ThemeColors.S(16), Top = ThemeColors.S(101), Width = ThemeColors.S(488), Height = ThemeColors.S(32), ForeColor = ThemeColors.CRed };
            Controls.Add(prompt); Controls.Add(pathBox); Controls.Add(note); Controls.Add(message);

            PillButton file = new PillButton("Browse file", false, 98) { Left = ThemeColors.S(16), Top = ThemeColors.S(142) };
            PillButton folder = new PillButton("Browse folder", false, 108) { Left = ThemeColors.S(121), Top = ThemeColors.S(142) };
            PillButton add = new PillButton("Add", true, 78) { Left = ThemeColors.S(342), Top = ThemeColors.S(142) };
            PillButton cancel = new PillButton("Cancel", false, 78) { Left = ThemeColors.S(426), Top = ThemeColors.S(142) };
            Controls.Add(file); Controls.Add(folder); Controls.Add(add); Controls.Add(cancel);
            file.Click += (s, e) => BrowseFile();
            folder.Click += (s, e) => BrowseFolder();
            add.Click += (s, e) => AcceptPath();
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { AcceptPath(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); e.Handled = true; }
            };
            Shown += (s, e) => pathBox.Focus();
        }

        private void BrowseFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Title = "Choose a file", CheckFileExists = true, Filter = "All files (*.*)|*.*" })
                if (dialog.ShowDialog(this) == DialogResult.OK) { pathBox.Text = dialog.FileName; message.Text = string.Empty; }
        }

        private void BrowseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Choose a folder", ShowNewFolderButton = false })
                if (dialog.ShowDialog(this) == DialogResult.OK) { pathBox.Text = dialog.SelectedPath; message.Text = string.Empty; }
        }

        private void AcceptPath()
        {
            try
            {
                SelectedItem = CustomNavigation.FromPath(pathBox.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
                pathBox.Focus();
            }
        }
    }
}
