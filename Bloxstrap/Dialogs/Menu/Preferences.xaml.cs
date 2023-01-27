using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Models;

namespace Bloxstrap.Dialogs.Menu
{
    /// <summary>
    /// Interaction logic for PreferencesWPF.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public readonly PreferencesViewModel ViewModel;

        public Preferences()
        {
            InitializeComponent();
            SetTheme();

            ViewModel = new(this);
            this.DataContext = ViewModel;

            App.SettingsManager.ShouldSave = false;

            this.Icon = Imaging.CreateBitmapSourceFromHIcon(
                Properties.Resources.IconBloxstrap_ico.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            this.Title = App.ProjectName;

            // just in case i guess?
            if (!Environment.Is64BitOperatingSystem)
                this.CheckBoxRFUEnabled.IsEnabled = false;
        }

        public void SetTheme()
        {
            string theme = "Light";

            if (App.Settings.Theme.GetFinal() == Theme.Dark)
                theme = "ColourfulDark";

            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"Dialogs/Menu/Themes/{theme}Theme.xaml", UriKind.Relative) };
        }

        private void ButtonOpenReShadeFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Directories.ReShade);
        }

        private void ButtonOpenReShadeHelp_Click(object sender, EventArgs e)
        {
            new ReShadeHelp().Show();
        }

        private void ButtonOpenModFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

        private void ButtonOpenModHelp_Click(object sender, EventArgs e)
        {
            new ModHelp().Show();
        }

        private void ButtonLocationBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ViewModel.InstallLocation = dialog.SelectedPath;
            }
        }

        private void ButtonPreview_Click(object sender, EventArgs e)
        {
            //this.Visible = false;
            App.Settings.BootstrapperStyle.Show();
            //this.Visible = true;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
            string installLocation = this.TextBoxInstallLocation.Text;

            if (String.IsNullOrEmpty(installLocation))
            {
                App.ShowMessageBox("You must set an install location", MessageBoxImage.Error);
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
                App.ShowMessageBox($"{App.ProjectName} does not have write access to the install location you selected. Please choose another install location.", MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                App.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                return;
            }

            if (App.IsFirstRun)
            {
                // this will be set in the registry after first install
                App.BaseDirectory = installLocation;
            }
            else
            {
                App.SettingsManager.ShouldSave = true;

                if (App.BaseDirectory is not null && App.BaseDirectory != installLocation)
                {
                    App.ShowMessageBox($"{App.ProjectName} will install to the new location you've set the next time it runs.", MessageBoxImage.Information);

                    App.Settings.VersionGuid = "";

                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\{App.ProjectName}"))
                    {
                        registryKey.SetValue("InstallLocation", installLocation);
                        registryKey.SetValue("OldInstallLocation", App.BaseDirectory);
                    }

                    // preserve settings
                    // we don't need to copy the bootstrapper over since the install process will do that automatically

                    App.SettingsManager.Save();

                    File.Copy(Path.Combine(App.BaseDirectory, "Settings.json"), Path.Combine(installLocation, "Settings.json"));
                }
            }

            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Utilities.OpenWebsite(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }

    public class PreferencesViewModel : INotifyPropertyChanged
    {
        private readonly Preferences _window;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string BloxstrapVersion { get; } = $"Version {App.Version}";

        #region Integrations
        public bool DRPEnabled 
        { 
            get => App.Settings.UseDiscordRichPresence;
            set
            {
                // if user wants discord rpc, auto-enable buttons by default
                _window.CheckBoxDRPButtons.IsChecked = value;
                App.Settings.UseDiscordRichPresence = value;
            }
        }

        public bool DRPButtons 
        { 
            get => !App.Settings.HideRPCButtons; 
            set => App.Settings.HideRPCButtons = !value; 
        }

        public bool RFUEnabled 
        { 
            get => App.Settings.RFUEnabled;
            set
            {
                // if user wants to use rbxfpsunlocker, auto-enable autoclosing by default
                _window.CheckBoxRFUAutoclose.IsChecked = value;
                App.Settings.RFUEnabled = value;
            }
        }

        public bool RFUAutoclose 
        { 
            get => App.Settings.RFUAutoclose; 
            set => App.Settings.RFUAutoclose = value; 
        }

        public bool UseReShade
        {
            get => App.Settings.UseReShade;
            set
            {
                // if user wants to use reshade, auto-enable use of extravi's presets by default
                _window.CheckBoxUseReShadeExtraviPresets.IsChecked = value;
                App.Settings.UseReShade = value;
            }
        }

        public bool UseReShadeExtraviPresets
        {
            get => App.Settings.UseReShadeExtraviPresets;
            set => App.Settings.UseReShadeExtraviPresets = value;
        }

        public bool ReShadeFolderButtonEnabled { get; } = !App.IsFirstRun;
        public string ReShadeFolderButtonTooltip { get; } = App.IsFirstRun ? "Bloxstrap must first be installed before managing ReShade" : "This is the folder that contains all your ReShade resources for presets, shaders and textures.";
        #endregion

        #region Modifications
        public bool ModOldDeathSound 
        { 
            get => App.Settings.UseOldDeathSound; 
            set => App.Settings.UseOldDeathSound = value; 
        }

        public bool ModOldMouseCursor 
        { 
            get => App.Settings.UseOldMouseCursor; 
            set => App.Settings.UseOldMouseCursor = value; 
        }

        public bool ModDisableAppPatch
        {
            get => App.Settings.UseDisableAppPatch;
            set => App.Settings.UseDisableAppPatch = value;
        }

        public bool ModFolderButtonEnabled { get; } = !App.IsFirstRun;
        public string ModFolderButtonTooltip { get; } = App.IsFirstRun ? "Bloxstrap must first be installed before managing mods" : "This is the folder that contains all your file modifications, including presets and any ReShade files needed.";
        #endregion

        #region Installation
        private string installLocation = App.IsFirstRun ? Path.Combine(Directories.LocalAppData, App.ProjectName) : App.BaseDirectory;
        public string InstallLocation 
        { 
            get => installLocation; 
            set
            {
                installLocation = value;
                OnPropertyChanged();
            }
        }

        private bool showAllChannels = !DeployManager.ChannelsAbstracted.Contains(App.Settings.Channel);
        public bool ShowAllChannels 
        { 
            get => showAllChannels; 
            set
            {
                if (value)
                {
                    Channels = DeployManager.ChannelsAll;
                }
                else
                {
                    Channels = DeployManager.ChannelsAbstracted;
                    Channel = DeployManager.DefaultChannel;
                    OnPropertyChanged("Channel");
                }

                showAllChannels = value;
            }
        }

        private IEnumerable<string> channels = DeployManager.ChannelsAbstracted.Contains(App.Settings.Channel) ? DeployManager.ChannelsAbstracted : DeployManager.ChannelsAll;
        public IEnumerable<string> Channels
        {
            get => channels;
            set
            {
                channels = value;
                OnPropertyChanged();
            }
        }

        public string Channel 
        {
            get => App.Settings.Channel;
            set
            {
                Task.Run(() => GetChannelInfo(value));
                App.Settings.Channel = value;
            }
        }

        private string channelInfo = "Getting latest version info, please wait...\n";
        public string ChannelInfo 
        { 
            get => channelInfo;
            set
            {
                channelInfo = value;
                OnPropertyChanged();
            } 
        }

        public bool PromptChannelChange
        {
            get => App.Settings.PromptChannelChange;
            set => App.Settings.PromptChannelChange = value;
        }
        #endregion

        #region Bloxstrap
        public IReadOnlyDictionary<string, Theme> Themes { get; set; } = new Dictionary<string, Theme>()
        {
            { "System Default", Enums.Theme.Default },
            { "Light", Enums.Theme.Light },
            { "Dark", Enums.Theme.Dark },
        };

        public string Theme
        {
            get => Themes.FirstOrDefault(x => x.Value == App.Settings.Theme).Key;
            set
            {
                App.Settings.Theme = Themes[value];
                _window.SetTheme();
            }
        }

        public IReadOnlyDictionary<string, BootstrapperStyle> Dialogs { get; set; } = new Dictionary<string, BootstrapperStyle>()
        {
            { "Vista (2009 - 2011)", BootstrapperStyle.VistaDialog },
            { "Legacy (2009 - 2011)", BootstrapperStyle.LegacyDialog2009 },
            { "Legacy (2011 - 2014)", BootstrapperStyle.LegacyDialog2011 },
            { "Progress (~2014)", BootstrapperStyle.ProgressDialog },
        };

        public string Dialog
        {
            get => Dialogs.FirstOrDefault(x => x.Value == App.Settings.BootstrapperStyle).Key;
            set => App.Settings.BootstrapperStyle = Dialogs[value];
        }

        public IReadOnlyDictionary<string, BootstrapperIcon> Icons { get; set; } = new Dictionary<string, BootstrapperIcon>()
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

        public string Icon
        {
            get => Icons.FirstOrDefault(x => x.Value == App.Settings.BootstrapperIcon).Key;
            set => App.Settings.BootstrapperIcon = Icons[value];
        }

        public bool CreateDesktopIcon
        {
            get => App.Settings.CreateDesktopIcon;
            set => App.Settings.CreateDesktopIcon = value;
        }

        public bool CheckForUpdates
        {
            get => App.Settings.CheckForUpdates;
            set => App.Settings.CheckForUpdates = value;
        }
        #endregion

        public string ConfirmButtonText { get; } = App.IsFirstRun ? "Install" : "Save";

        public PreferencesViewModel(Preferences window)
        {
            _window = window;
            Task.Run(() => GetChannelInfo(App.Settings.Channel));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task GetChannelInfo(string channel)
        {
            ChannelInfo = "Getting latest version info, please wait...\n";

            ClientVersion info = await DeployManager.GetLastDeploy(channel, true);
            string? strTimestamp = info.Timestamp?.ToString("MM/dd/yyyy h:mm:ss tt", App.CultureFormat);

            ChannelInfo = $"Version: v{info.Version} ({info.VersionGuid})\nDeployed: {strTimestamp}";
        }
    }
}
