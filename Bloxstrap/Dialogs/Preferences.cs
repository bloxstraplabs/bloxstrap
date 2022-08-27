using System.IO;
using System.Diagnostics;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.Integrations;
using Bloxstrap.Models;

namespace Bloxstrap.Dialogs
{
    public partial class Preferences : Form
    {
        #region Properties
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
            { "2015", BootstrapperIcon.IconEarly2015 },
            { "2016", BootstrapperIcon.IconLate2015 },
            { "2017", BootstrapperIcon.Icon2017 },
            { "2019", BootstrapperIcon.Icon2019 },
            { "2022", BootstrapperIcon.Icon2022 }
        };

        private string ChannelInfo
        {
            set
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(() => this.LabelChannelInfo.Text = value);
                }
                else
                {
                    this.LabelChannelInfo.Text = value;
                }
            }
        }
        #endregion

        #region Core
        private async Task GetChannelInfo(string channel)
        {
            ChannelInfo = "Getting latest version, please wait...";

            VersionDeploy info = await DeployManager.GetLastDeploy(channel);

            if (info.FileVersion is null || info.Timestamp is null)
                return;

            string strTimestamp = info.Timestamp.Value.ToString("MM/dd/yyyy h:mm:ss tt", Program.CultureFormat);

            ChannelInfo = $"Latest version:\nv{info.FileVersion} @ {strTimestamp}";
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

            if (!Environment.Is64BitOperatingSystem)
                this.ToggleRFUEnabled.Enabled = false;

            // set data sources for list controls
            this.StyleSelection.DataSource = SelectableStyles.Keys.ToList();
            this.IconSelection.DataSource = SelectableIcons.Keys.ToList();

            if (DeployManager.ChannelsAbstracted.Contains(Program.Settings.Channel))
                this.SelectChannel.DataSource = DeployManager.ChannelsAbstracted;
            else
                this.ToggleShowAllChannels.Checked = true;

            // populate preferences
            this.StyleSelection.Text = SelectableStyles.FirstOrDefault(x => x.Value == Program.Settings.BootstrapperStyle).Key;
            this.IconSelection.Text = SelectableIcons.FirstOrDefault(x => x.Value == Program.Settings.BootstrapperIcon).Key;

            this.ToggleCheckForUpdates.Checked = Program.Settings.CheckForUpdates;
            
            this.ToggleDiscordRichPresence.Checked = Program.Settings.UseDiscordRichPresence;
            this.ToggleRPCButtons.Checked = Program.Settings.HideRPCButtons;

            this.ToggleRFUEnabled.Checked = Program.Settings.RFUEnabled;
            this.ToggleRFUAutoclose.Checked = Program.Settings.RFUAutoclose;

            this.ToggleDeathSound.Checked = Program.Settings.UseOldDeathSound;
            this.ToggleMouseCursor.Checked = Program.Settings.UseOldMouseCursor;

            this.SelectChannel.Text = Program.Settings.Channel;
        }
        #endregion

        #region Dialog Events
        private void ToggleShowAllChannels_CheckedChanged(object sender, EventArgs e)
        {
            if (this.ToggleShowAllChannels.Checked)
                this.SelectChannel.DataSource = DeployManager.ChannelsAll;
            else
                this.SelectChannel.DataSource = DeployManager.ChannelsAbstracted;

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string installLocation = this.InstallLocation.Text;

            if (String.IsNullOrEmpty(installLocation))
            {
                Program.ShowMessageBox("You must set an install location", MessageBoxIcon.Error);
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
                Program.ShowMessageBox($"{Program.ProjectName} does not have write access to the install location you selected. Please choose another install location.", MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                Program.ShowMessageBox(ex.Message, MessageBoxIcon.Error);
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
                    Program.ShowMessageBox($"{Program.ProjectName} will install to the new location you've set the next time it runs.", MessageBoxIcon.Information);

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

            this.Close();
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Program.Settings.BootstrapperStyle.Show();
            this.Visible = true;
        }

        private void Preferences_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
        #endregion

        #region Preference Events
        private void StyleSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;

            Program.Settings.BootstrapperStyle = SelectableStyles[this.StyleSelection.Text];
        }

        private void IconSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            BootstrapperIcon icon = SelectableIcons[this.IconSelection.Text];
            
            this.IconPreview.BackgroundImage = icon.GetBitmap();

            if (!this.Visible)
                return;

            Program.Settings.BootstrapperIcon = icon;
        }

        private void ToggleDiscordRichPresence_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseDiscordRichPresence = this.ToggleRPCButtons.Enabled = this.ToggleDiscordRichPresence.Checked;
        }

        private void ToggleRPCButtons_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.HideRPCButtons = this.ToggleRPCButtons.Checked;
        }

        private void RFUWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.OpenWebsite($"https://github.com/{RbxFpsUnlocker.ProjectRepository}");
        }

        private void ToggleRFUEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.RFUEnabled = this.ToggleRFUAutoclose.Enabled = this.ToggleRFUEnabled.Checked;
        }

        private void ToggleRFUAutoclose_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.RFUAutoclose = this.ToggleRFUAutoclose.Checked;
        }

        private void InstallLocationBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = this.InstallLocationBrowseDialog.ShowDialog();

            if (result == DialogResult.OK)
                this.InstallLocation.Text = this.InstallLocationBrowseDialog.SelectedPath;
        }

        private void ToggleDeathSound_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseOldDeathSound = this.ToggleDeathSound.Checked;
        }

        private void ToggleMouseCursor_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseOldMouseCursor = this.ToggleMouseCursor.Checked;
        }

        private void ButtonOpenModFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

        private void SelectChannel_SelectedValueChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                Program.Settings.Channel = this.SelectChannel.Text;
                
            Task.Run(() => GetChannelInfo(Program.Settings.Channel));
        }

        private void ToggleCheckForUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.CheckForUpdates = this.ToggleCheckForUpdates.Checked;
        }
        #endregion
    }
}
