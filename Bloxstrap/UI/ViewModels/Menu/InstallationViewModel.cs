using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Enums;
using Bloxstrap.Extensions;
using Bloxstrap.Models;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class InstallationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _manualChannelEntry = !RobloxDeployment.SelectableChannels.Contains(App.Settings.Prop.Channel);

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);
        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

        public DeployInfo? ChannelDeployInfo { get; private set; } = null;
        public string ChannelInfoLoadingText { get; private set; } = null!;

        public InstallationViewModel()
        {
            Task.Run(() => LoadChannelDeployInfo(App.Settings.Prop.Channel));
        }

        private async Task LoadChannelDeployInfo(string channel)
        {
            ChannelInfoLoadingText = "Fetching latest deploy info, please wait...";
            OnPropertyChanged(nameof(ChannelInfoLoadingText));

            ChannelDeployInfo = null;
            OnPropertyChanged(nameof(ChannelDeployInfo));

            try
            {
                ClientVersion info = await RobloxDeployment.GetInfo(channel, true);

                ChannelDeployInfo = new DeployInfo
                {
                    Version = info.Version,
                    VersionGuid = info.VersionGuid,
                    Timestamp = info.Timestamp?.ToFriendlyString()!
                };

                OnPropertyChanged(nameof(ChannelDeployInfo));
            }
            catch (Exception)
            {
                ChannelInfoLoadingText = "Failed to get deploy info.\nIs the channel name valid?";
                OnPropertyChanged(nameof(ChannelInfoLoadingText));
            }
        }

        private void BrowseInstallLocation()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            InstallLocation = dialog.SelectedPath;

            OnPropertyChanged(nameof(InstallLocation));
        }

        private void OpenFolder()
        {
            Process.Start("explorer.exe", Directories.Base);
        }

        public string InstallLocation
        {
            get => App.BaseDirectory;
            set => App.BaseDirectory = value;
        }

        public IEnumerable<string> Channels => RobloxDeployment.SelectableChannels;

        public string Channel
        {
            get => App.Settings.Prop.Channel;
            set
            {
                value = value.Trim();
                Task.Run(() => LoadChannelDeployInfo(value));
                App.Settings.Prop.Channel = value;
            }
        }

        public bool ManualChannelEntry
        {
            get => _manualChannelEntry;
            set
            {
                _manualChannelEntry = value;

                if (!value)
                {
                    // roblox typically sets channels in all lowercase, so here we find if a case insensitive match exists
                    string? matchingChannel = Channels.Where(x => x.ToLowerInvariant() == Channel.ToLowerInvariant()).FirstOrDefault();
                    Channel = string.IsNullOrEmpty(matchingChannel) ? RobloxDeployment.DefaultChannel : matchingChannel;
                }

                OnPropertyChanged(nameof(Channel));
                OnPropertyChanged(nameof(ChannelComboBoxVisibility));
                OnPropertyChanged(nameof(ChannelTextBoxVisibility));
            }
        }

        // cant use data bindings so i have to do whatever tf this is
        public Visibility ChannelComboBoxVisibility => ManualChannelEntry ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ChannelTextBoxVisibility => ManualChannelEntry ? Visibility.Visible : Visibility.Collapsed;

        // todo - move to enum attributes?
        public IReadOnlyDictionary<string, ChannelChangeMode> ChannelChangeModes => new Dictionary<string, ChannelChangeMode>
        {
            { "Change automatically", ChannelChangeMode.Automatic },
            { "Always prompt", ChannelChangeMode.Prompt },
            { "Never change", ChannelChangeMode.Ignore },
        };

        public string SelectedChannelChangeMode
        {
            get => ChannelChangeModes.FirstOrDefault(x => x.Value == App.Settings.Prop.ChannelChangeMode).Key;
            set => App.Settings.Prop.ChannelChangeMode = ChannelChangeModes[value];
        }
    }
}
