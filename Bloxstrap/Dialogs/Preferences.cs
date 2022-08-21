using System.IO;
using System.Diagnostics;

using Microsoft.Win32;

using Bloxstrap.Dialogs.BootstrapperStyles;
using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.Integrations;

namespace Bloxstrap.Dialogs
{
    public partial class Preferences : Form
    {
        private static readonly IReadOnlyDictionary<string, BootstrapperStyle> SelectableStyles = new Dictionary<string, BootstrapperStyle>()
        {
            { "Vista (2009 - 2011)", BootstrapperStyle.VistaDialog },
            { "Legacy (2009 - 2011)", BootstrapperStyle.LegacyDialog2009 },
            { "Legacy (2011 - 2014)", BootstrapperStyle.LegacyDialog2011 },
            { "Progress (~2014)", BootstrapperStyle.ProgressDialog },
            { "Progress (Dark)", BootstrapperStyle.ProgressDialogDark },
        };

        private static readonly IReadOnlyDictionary<string, BootstrapperIcon> SelectableIcons = new Dictionary<string, BootstrapperIcon>()
        {
            { "Bloxstrap", BootstrapperIcon.IconBloxstrap },
            { "2009", BootstrapperIcon.Icon2009 },
            { "2011", BootstrapperIcon.Icon2011 },
            { "Early 2015", BootstrapperIcon.IconEarly2015 },
            { "Late 2015", BootstrapperIcon.IconLate2015 },
            { "2017", BootstrapperIcon.Icon2017 },
            { "2019", BootstrapperIcon.Icon2019 },
        };

        private BootstrapperStyle? _selectedStyle;
        private BootstrapperIcon? _selectedIcon;

        private BootstrapperStyle SelectedStyle
        {
            get => (BootstrapperStyle)_selectedStyle;

            set
            {
                if (_selectedStyle == value)
                    return;

                _selectedStyle = value;

                int index = SelectableStyles.Values.ToList().IndexOf(value);
                this.StyleSelection.SetSelected(index, true);
            }
        }

        private BootstrapperIcon SelectedIcon
        {
            get => (BootstrapperIcon)_selectedIcon;

            set
            {
                if (_selectedIcon == value)
                    return;

                _selectedIcon = value;

                int index = SelectableIcons.Values.ToList().IndexOf(value);
                this.IconSelection.SetSelected(index, true);
                this.IconPreview.BackgroundImage = IconManager.GetBitmapResource(value);
            }
        }

        public Preferences()
        {
            InitializeComponent();

            Program.SettingsManager.ShouldSave = false;

            this.Icon = Properties.Resources.IconBloxstrap_ico;
            this.Text = Program.ProjectName;

            if (Program.IsFirstRun)
            {
                this.SaveButton.Text = "Install";
                this.InstallLocation.Text = Path.Combine(Program.LocalAppData, Program.ProjectName);
                this.ButtonOpenModFolder.Visible = false;
                this.LabelModFolderInstall.Visible = true;
            }
            else
            {
                this.InstallLocation.Text = Program.BaseDirectory;
            }

            foreach (var style in SelectableStyles)
            {
                this.StyleSelection.Items.Add(style.Key);
            }

            foreach (var icon in SelectableIcons)
            {
                this.IconSelection.Items.Add(icon.Key);
            }

            if (!Environment.Is64BitOperatingSystem)
                this.ToggleRFUEnabled.Enabled = false;

            SelectedStyle = Program.Settings.BootstrapperStyle;
            SelectedIcon = Program.Settings.BootstrapperIcon;

            this.ToggleCheckForUpdates.Checked = Program.Settings.CheckForUpdates;
            
            this.ToggleDiscordRichPresence.Checked = Program.Settings.UseDiscordRichPresence;
            this.ToggleRPCButtons.Checked = Program.Settings.HideRPCButtons;

            this.ToggleRFUEnabled.Checked = Program.Settings.RFUEnabled;
            this.ToggleRFUAutoclose.Checked = Program.Settings.RFUAutoclose;

            this.ToggleDeathSound.Checked = Program.Settings.UseOldDeathSound;
            this.ToggleMouseCursor.Checked = Program.Settings.UseOldMouseCursor;
        }

        private void InstallLocationBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = this.InstallLocationBrowseDialog.ShowDialog();

            if (result == DialogResult.OK)
                this.InstallLocation.Text = this.InstallLocationBrowseDialog.SelectedPath;
        }

        private void StyleSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = this.StyleSelection.Text;
            SelectedStyle = SelectableStyles[selected];
        }

        private void IconSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = this.IconSelection.Text;
            SelectedIcon = SelectableIcons[selected];
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string installLocation = this.InstallLocation.Text;

            if (String.IsNullOrEmpty(installLocation))
            {
                Program.ShowMessageBox(MessageBoxIcon.Error, "You must set an install location");
                return;
            }

            try
            {
                // check if we can write to the directory (a bit hacky but eh)

                string testPath = installLocation;
                string testFile = Path.Combine(installLocation, "BloxstrapWriteTest.txt");
                bool testPathExists = Directory.Exists(testPath);

                if (!testPathExists)
                    Directory.CreateDirectory(testPath);

                File.WriteAllText(testFile, "hi");
                File.Delete(testFile);

                if (!testPathExists)
                    Directory.Delete(testPath);
            }
            catch (UnauthorizedAccessException)
            {
                Program.ShowMessageBox(MessageBoxIcon.Error, $"{Program.ProjectName} does not have write access to the install location you selected. Please choose another install location.");
                return;
            }
            catch (Exception ex)
            {
                Program.ShowMessageBox(MessageBoxIcon.Error, ex.Message);
                return;
            }

            if (Program.IsFirstRun)
            {
                // this will be set in the registry after first install
                Program.BaseDirectory = installLocation;
            }
            else
            {
                Program.SettingsManager.ShouldSave = true;

                if (Program.BaseDirectory is not null && Program.BaseDirectory != installLocation)
                {
                    Program.ShowMessageBox(MessageBoxIcon.Information, $"{Program.ProjectName} will install to the new location you've set the next time it runs.");

                    Program.Settings.VersionGuid = "";

                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\{Program.ProjectName}"))
                    {
                        registryKey.SetValue("InstallLocation", installLocation);
                        registryKey.SetValue("OldInstallLocation", Program.BaseDirectory);
                    }

                    // preserve settings
                    // we don't need to copy the bootstrapper over since the install process will do that automatically

                    Program.SettingsManager.Save();

                    File.Copy(Path.Combine(Program.BaseDirectory, "Settings.json"), Path.Combine(installLocation, "Settings.json"));
                }
            }                

            Program.Settings.BootstrapperStyle = SelectedStyle;
            Program.Settings.BootstrapperIcon = SelectedIcon;

            this.Close();
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            Program.Settings.BootstrapperIcon = SelectedIcon;

            this.Visible = false;

            switch (SelectedStyle)
            {
                case BootstrapperStyle.VistaDialog:
                    new VistaDialog().ShowDialog();
                    break;

                case BootstrapperStyle.LegacyDialog2009:
                    new LegacyDialog2009().ShowDialog();
                    break;

                case BootstrapperStyle.LegacyDialog2011:
                    new LegacyDialog2011().ShowDialog();
                    break;

                case BootstrapperStyle.ProgressDialog:
                    new ProgressDialog().ShowDialog();
                    break;

                case BootstrapperStyle.ProgressDialogDark:
                    new ProgressDialogDark().ShowDialog();
                    break;
            }

            this.Visible = true;
        }

        private void ToggleDiscordRichPresence_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseDiscordRichPresence = this.ToggleRPCButtons.Enabled = this.ToggleDiscordRichPresence.Checked;
        }

        private void ToggleRPCButtons_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.HideRPCButtons = this.ToggleRPCButtons.Checked;
        }

        private void ToggleRFUEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.RFUEnabled = this.ToggleRFUAutoclose.Enabled = this.ToggleRFUEnabled.Checked;
        }

        private void ToggleRFUAutoclose_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.RFUAutoclose = this.ToggleRFUAutoclose.Checked;
        }

        private void ToggleDeathSound_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseOldDeathSound = this.ToggleDeathSound.Checked;
        }

        private void ToggleMouseCursor_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseOldMouseCursor = this.ToggleMouseCursor.Checked;
        }

        private void ToggleCheckForUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.CheckForUpdates = this.ToggleCheckForUpdates.Checked;
        }

        private void RFUWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.OpenWebsite($"https://github.com/{RbxFpsUnlocker.ProjectRepository}");
        }

        private void ButtonOpenModFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

        private void Preferences_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}
