using Microsoft.Win32;

using Bloxstrap.Helpers;
using Bloxstrap.Enums;
using Bloxstrap.Dialogs.BootstrapperStyles;

namespace Bloxstrap.Dialogs
{
    public partial class Preferences : Form
    {
        private static readonly IReadOnlyDictionary<string, BootstrapperStyle> SelectableStyles = new Dictionary<string, BootstrapperStyle>()
        {
            { "Legacy (2011 - 2014)", BootstrapperStyle.LegacyDialog },
            { "Progress (~2014)", BootstrapperStyle.ProgressDialog },
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
        private bool _useDiscordRichPresence = true;
        private bool _useOldDeathSound = true;

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

        private bool UseDiscordRichPresence
        {
            get => _useDiscordRichPresence;

            set
            {
                if (_useDiscordRichPresence == value)
                    return;

                _useDiscordRichPresence = value;

                this.ToggleDiscordRichPresence.Checked = value;
            }
        }

        private bool UseOldDeathSound
        {
            get => _useOldDeathSound;

            set
            {
                if (_useOldDeathSound == value)
                    return;

                _useOldDeathSound = value;

                this.ToggleDeathSound.Checked = value;
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

            this.InfoTooltip.SetToolTip(this.StyleSelection, "Choose how the bootstrapper dialog should look.");
            this.InfoTooltip.SetToolTip(this.IconSelection, "Choose what icon the bootstrapper should use.");
            this.InfoTooltip.SetToolTip(this.GroupBoxInstallLocation, "Choose where Bloxstrap should install to.\nThis is useful if you typically install all your games to a separate storage drive.");
            this.InfoTooltip.SetToolTip(this.ToggleDiscordRichPresence, "Choose whether to show what game you're playing on your Discord profile.\nThis will ONLY work when you launch a game from the website, and is not supported in the Beta App.");

            SelectedStyle = Program.Settings.BootstrapperStyle;
            SelectedIcon = Program.Settings.BootstrapperIcon;
            UseDiscordRichPresence = Program.Settings.UseDiscordRichPresence;
            UseOldDeathSound = Program.Settings.UseOldDeathSound;
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
            else if (Program.BaseDirectory != installLocation)
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

            if (!Program.IsFirstRun)
                Program.SettingsManager.ShouldSave = true;

            Program.Settings.BootstrapperStyle = SelectedStyle;
            Program.Settings.BootstrapperIcon = SelectedIcon;
            Program.Settings.UseDiscordRichPresence = UseDiscordRichPresence;
            Program.Settings.UseOldDeathSound = UseOldDeathSound;

            this.Close();
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            // small hack to get the icon to show in the preview without saving to settings
            BootstrapperIcon savedIcon = Program.Settings.BootstrapperIcon;
            Program.Settings.BootstrapperIcon = SelectedIcon;

            this.Visible = false;

            switch (SelectedStyle)
            {
                case BootstrapperStyle.LegacyDialog:
                    new LegacyDialog().ShowDialog();
                    break;

                case BootstrapperStyle.ProgressDialog:
                    new ProgressDialog().ShowDialog();
                    break;
            }

            Program.Settings.BootstrapperIcon = savedIcon;

            this.Visible = true;
        }

        private void ToggleDiscordRichPresence_CheckedChanged(object sender, EventArgs e)
        {
            UseDiscordRichPresence = this.ToggleDiscordRichPresence.Checked;
        }

        private void ToggleDeathSound_CheckedChanged(object sender, EventArgs e)
        {
            UseOldDeathSound = this.ToggleDeathSound.Checked;
        }
    }
}
