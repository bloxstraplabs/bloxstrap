using System.Windows;

using Bloxstrap.Exceptions;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        private bool _manualChannelEntry = !RobloxDeployment.SelectableChannels.Contains(App.Settings.Prop.Channel);

        public BehaviourViewModel()
        {
            Task.Run(() => LoadChannelDeployInfo(App.Settings.Prop.Channel));
        }

        private async Task LoadChannelDeployInfo(string channel)
        {
            const string LOG_IDENT = "BehaviourViewModel::LoadChannelDeployInfo";
            
            LoadingSpinnerVisibility = Visibility.Visible;
            LoadingErrorVisibility = Visibility.Collapsed;
            ChannelInfoLoadingText = "Fetching latest deploy info, please wait...";
            ChannelDeployInfo = null;

            OnPropertyChanged(nameof(LoadingSpinnerVisibility));
            OnPropertyChanged(nameof(LoadingErrorVisibility));
            OnPropertyChanged(nameof(ChannelInfoLoadingText));
            OnPropertyChanged(nameof(ChannelDeployInfo));

            try
            {
                ClientVersion info = await RobloxDeployment.GetInfo(channel, true);

                ChannelWarningVisibility = info.IsBehindDefaultChannel ? Visibility.Visible : Visibility.Collapsed;

                ChannelDeployInfo = new DeployInfo
                {
                    Version = info.Version,
                    VersionGuid = info.VersionGuid,
                    Timestamp = info.Timestamp?.ToFriendlyString()!
                };

                OnPropertyChanged(nameof(ChannelWarningVisibility));
                OnPropertyChanged(nameof(ChannelDeployInfo));
            }
            catch (HttpResponseUnsuccessfulException ex)
            {
                LoadingSpinnerVisibility = Visibility.Collapsed;
                LoadingErrorVisibility = Visibility.Visible;

                ChannelInfoLoadingText = ex.ResponseMessage.StatusCode switch
                {
                    HttpStatusCode.NotFound => "The specified channel name does not exist.",
                    _ => $"Failed to fetch information! (HTTP {ex.ResponseMessage.StatusCode})",
                };

                OnPropertyChanged(nameof(LoadingSpinnerVisibility));
                OnPropertyChanged(nameof(LoadingErrorVisibility));
                OnPropertyChanged(nameof(ChannelInfoLoadingText));
            }
            catch (Exception ex)
            {
                LoadingSpinnerVisibility = Visibility.Collapsed;
                LoadingErrorVisibility = Visibility.Visible;

                App.Logger.WriteLine(LOG_IDENT, "An exception occurred while fetching channel information");
                App.Logger.WriteException(LOG_IDENT, ex);

                ChannelInfoLoadingText = $"Failed to fetch information! ({ex.Message})";

                OnPropertyChanged(nameof(LoadingSpinnerVisibility));
                OnPropertyChanged(nameof(LoadingErrorVisibility));
                OnPropertyChanged(nameof(ChannelInfoLoadingText));
            }
        }

        public Visibility LoadingSpinnerVisibility { get; private set; } = Visibility.Visible;
        public Visibility LoadingErrorVisibility { get; private set; } = Visibility.Collapsed;
        public Visibility ChannelWarningVisibility { get; private set; } = Visibility.Collapsed;

        public DeployInfo? ChannelDeployInfo { get; private set; } = null;
        public string ChannelInfoLoadingText { get; private set; } = null!;

        public bool CreateDesktopIcon
        {
            get => App.Settings.Prop.CreateDesktopIcon;
            set => App.Settings.Prop.CreateDesktopIcon = value;
        }

        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;
            set => App.Settings.Prop.CheckForUpdates = value;
        }

        public IEnumerable<string> Channels => RobloxDeployment.SelectableChannels;

        public string SelectedChannel
        {
            get => App.Settings.Prop.Channel;
            set
            {
                value = value.Trim();

                if (String.IsNullOrEmpty(value))
                    value = RobloxDeployment.DefaultChannel;
                
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
                    string? matchingChannel = Channels.Where(x => x.ToLowerInvariant() == SelectedChannel.ToLowerInvariant()).FirstOrDefault();
                    SelectedChannel = string.IsNullOrEmpty(matchingChannel) ? RobloxDeployment.DefaultChannel : matchingChannel;
                }

                OnPropertyChanged(nameof(SelectedChannel));
            }
        }

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
