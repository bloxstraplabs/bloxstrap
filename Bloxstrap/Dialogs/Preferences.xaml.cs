using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;
using Bloxstrap.Models;

using REghZyFramework.Themes;
using System.Windows.Forms;

namespace Bloxstrap.Dialogs
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

            Program.SettingsManager.ShouldSave = false;

            this.Icon = Imaging.CreateBitmapSourceFromHIcon(
                Properties.Resources.IconBloxstrap_ico.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            this.Title = Program.ProjectName;

            // just in case i guess?
            if (!Environment.Is64BitOperatingSystem)
                this.CheckBoxRFUEnabled.IsEnabled = false;
        }

        public void SetTheme()
        {
            string theme = "Light";

            if (Program.Settings.Theme.GetFinal() == Theme.Dark)
                theme = "ColourfulDark";

            this.Resources.MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"Dialogs/Themes/{theme}Theme.xaml", UriKind.Relative) };
        }

        private void ButtonOpenModFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Directories.Modifications);
        }

        private void ButtonLocationBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                    ViewModel.InstallLocation = dialog.SelectedPath;
            }
        }

        private void ButtonPreview_Click(object sender, EventArgs e)
        {
            //this.Visible = false;
            Program.Settings.BootstrapperStyle.Show();
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

        #region Integrations
        public bool DRPEnabled 
        { 
            get => Program.Settings.UseDiscordRichPresence; 
            set => Program.Settings.UseDiscordRichPresence = value; 
        }

        public bool DRPButtons 
        { 
            get => !Program.Settings.HideRPCButtons; 
            set => Program.Settings.HideRPCButtons = !value; 
        }

        public bool RFUEnabled 
        { 
            get => Program.Settings.RFUEnabled; 
            set => Program.Settings.RFUEnabled = value; 
        }

        public bool RFUAutoclose 
        { 
            get => Program.Settings.RFUAutoclose; 
            set => Program.Settings.RFUAutoclose = value; 
        }

        public bool ModOldDeathSound 
        { 
            get => Program.Settings.UseOldDeathSound; 
            set => Program.Settings.UseOldDeathSound = value; 
        }

        public bool ModOldMouseCursor 
        { 
            get => Program.Settings.UseOldMouseCursor; 
            set => Program.Settings.UseOldMouseCursor = value; 
        }

        public bool ModDisableAppPatch
        {
            get => Program.Settings.UseDisableAppPatch;
            set => Program.Settings.UseDisableAppPatch = value;
        }

        public bool ModFolderButtonEnabled { get; } = !Program.IsFirstRun;
        public string ModFolderButtonText { get; } = Program.IsFirstRun ? "Custom mods can be added after installing Bloxstrap" : "Open mod folder";
        #endregion

        #region Installation
        private string installLocation = Program.IsFirstRun ? Path.Combine(Program.LocalAppData, Program.ProjectName) : Program.BaseDirectory;
        public string InstallLocation 
        { 
            get => installLocation; 
            set
            {
                installLocation = value;
                OnPropertyChanged();
            }
        }

        private bool showAllChannels = !DeployManager.ChannelsAbstracted.Contains(Program.Settings.Channel);
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

        private IEnumerable<string> channels = DeployManager.ChannelsAbstracted.Contains(Program.Settings.Channel) ? DeployManager.ChannelsAbstracted : DeployManager.ChannelsAll;
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
            get => Program.Settings.Channel;
            set
            {
                Task.Run(() => GetChannelInfo(value));
                Program.Settings.Channel = value;
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

        public bool CheckForUpdates
        {
            get => Program.Settings.CheckForUpdates;
            set => Program.Settings.CheckForUpdates = value;
        }
        #endregion

        #region Style
        public IReadOnlyDictionary<string, Theme> Themes { get; set; } = new Dictionary<string, Theme>()
        {
            { "System Default", Enums.Theme.Default },
            { "Light", Enums.Theme.Light },
            { "Dark", Enums.Theme.Dark },
        };

        public string Theme
        {
            get => Themes.FirstOrDefault(x => x.Value == Program.Settings.Theme).Key;
            set
            {
                Program.Settings.Theme = Themes[value];
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
            get => Dialogs.FirstOrDefault(x => x.Value == Program.Settings.BootstrapperStyle).Key;
            set => Program.Settings.BootstrapperStyle = Dialogs[value];
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
            get => Icons.FirstOrDefault(x => x.Value == Program.Settings.BootstrapperIcon).Key;
            set => Program.Settings.BootstrapperIcon = Icons[value];
        }
        #endregion

        public string ConfirmButtonText { get; } = Program.IsFirstRun ? "Install" : "Save";

        public PreferencesViewModel(Preferences window)
        {
            _window = window;
            Task.Run(() => GetChannelInfo(Program.Settings.Channel));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task GetChannelInfo(string channel)
        {
            ChannelInfo = "Getting latest version info, please wait...\n";

            ClientVersion info = await DeployManager.GetLastDeploy(channel, true);
            string? strTimestamp = info.Timestamp?.ToString("MM/dd/yyyy h:mm:ss tt", Program.CultureFormat);

            ChannelInfo = $"Version: v{info.Version} ({info.VersionGuid})\nDeployed: {strTimestamp}";
        }
    }
}
