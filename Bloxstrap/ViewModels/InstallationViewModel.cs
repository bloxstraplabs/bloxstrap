using Bloxstrap.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using Wpf.Ui.Mvvm.Interfaces;
using System.ComponentModel;
using Bloxstrap.Helpers;
using Bloxstrap.Models;

namespace Bloxstrap.ViewModels
{
    public class InstallationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private IEnumerable<string> _channels = DeployManager.ChannelsAbstracted.Contains(App.Settings.Channel) ? DeployManager.ChannelsAbstracted : DeployManager.ChannelsAll;
        private bool _showAllChannels = !DeployManager.ChannelsAbstracted.Contains(App.Settings.Channel);

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);

        public DeployInfo? ChannelDeployInfo { get; private set; } = null; //new DeployInfo(){ Version = "hi", VersionGuid = "hi", Timestamp = "January 25 2023 at 6:03:48 PM" };

        public InstallationViewModel()
        {
            Task.Run(() => LoadChannelDeployInfo(App.Settings.Channel));
        }

        private async Task LoadChannelDeployInfo(string channel)
        {
            ChannelDeployInfo = null;
            OnPropertyChanged(nameof(ChannelDeployInfo));

            ClientVersion info = await DeployManager.GetLastDeploy(channel, true);
            string? strTimestamp = info.Timestamp?.ToString("MM/dd/yyyy h:mm:ss tt", App.CultureFormat);

            ChannelDeployInfo = new DeployInfo() { Version = info.Version, VersionGuid = info.VersionGuid, Timestamp = strTimestamp! };
            OnPropertyChanged(nameof(ChannelDeployInfo));
        }

        private void BrowseInstallLocation()
        {
            using var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                InstallLocation = dialog.SelectedPath;
                OnPropertyChanged(nameof(InstallLocation));
            }
        }

        public string InstallLocation
        {
            get => App.BaseDirectory; 
            set => App.BaseDirectory = value;
        }

        public bool CreateDesktopIcon
        {
            get => App.Settings.CreateDesktopIcon;
            set => App.Settings.CreateDesktopIcon = value;
        }

        public IEnumerable<string> Channels
        {
            get => _channels;
            set => _channels = value;
        }

        public string Channel
        {
            get => App.Settings.Channel;
            set
            {
                //Task.Run(() => GetChannelInfo(value));
                Task.Run(() => LoadChannelDeployInfo(value));
                App.Settings.Channel = value;
            }
        }

        public bool ShowAllChannels
        {
            get => _showAllChannels;
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
                    OnPropertyChanged(nameof(Channel));
                }

                OnPropertyChanged(nameof(Channels));

                _showAllChannels = value;
            }
        }
    }
}
