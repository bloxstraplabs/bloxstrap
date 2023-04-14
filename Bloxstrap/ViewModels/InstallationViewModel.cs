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
using System.Diagnostics;

namespace Bloxstrap.ViewModels
{
    public class InstallationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private IEnumerable<string> _channels = DeployManager.ChannelsAbstracted.Contains(App.Settings.Prop.Channel) ? DeployManager.ChannelsAbstracted : DeployManager.ChannelsAll;
        private bool _showAllChannels = !DeployManager.ChannelsAbstracted.Contains(App.Settings.Prop.Channel);

        public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);
        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);


        public DeployInfo? ChannelDeployInfo { get; private set; } = null; //new DeployInfo(){ Version = "hi", VersionGuid = "hi", Timestamp = "January 25 2023 at 6:03:48 PM" };

        public InstallationViewModel()
        {
            Task.Run(() => LoadChannelDeployInfo(App.Settings.Prop.Channel));
        }

        private async Task LoadChannelDeployInfo(string channel)
        {
            ChannelDeployInfo = null;
            OnPropertyChanged(nameof(ChannelDeployInfo));

            App.DeployManager.SetChannel(channel);
            ClientVersion info = await App.DeployManager.GetLastDeploy(true);

            ChannelDeployInfo = new DeployInfo
            {
                Version = info.Version, 
                VersionGuid = info.VersionGuid, 
                Timestamp = info.Timestamp?.ToString("dddd, d MMMM yyyy 'at' h:mm:ss tt", App.CultureFormat)!
            };

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

        private void OpenFolder()
        {
            Process.Start("explorer.exe", Directories.Base);
        }

        public string InstallLocation
        {
            get => App.BaseDirectory; 
            set => App.BaseDirectory = value;
        }

        public IEnumerable<string> Channels
        {
            get
            {
                if (_channels == DeployManager.ChannelsAll && !_channels.Contains(App.Settings.Prop.Channel))
                    _channels = _channels.Append(App.Settings.Prop.Channel);

                return _channels;
            }
            set => _channels = value;
        }

        public string Channel
        {
            get => App.Settings.Prop.Channel;
            set
            {
                //Task.Run(() => GetChannelInfo(value));
                Task.Run(() => LoadChannelDeployInfo(value));
                App.Settings.Prop.Channel = value;
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

                    if (!Channels.Contains(Channel))
                    {
                        Channel = DeployManager.DefaultChannel;
                        OnPropertyChanged(nameof(Channel));
                    }
                }

                OnPropertyChanged(nameof(Channels));

                _showAllChannels = value;
            }
        }
    }
}
