namespace Bloxstrap.UI.ViewModels.Menu
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        private string _oldPlayerVersionGuid = "";
        private string _oldStudioVersionGuid = "";

        public BehaviourViewModel()
        {
            Task.Run(() => LoadChannelDeployInfo(App.Settings.Prop.Channel));
        }

        private async Task LoadChannelDeployInfo(string channel)
        {
            const string LOG_IDENT = "BehaviourViewModel::LoadChannelDeployInfo";

            ShowLoadingError = false;
            OnPropertyChanged(nameof(ShowLoadingError));

            ChannelInfoLoadingText = "Fetching latest deploy info, please wait...";
            OnPropertyChanged(nameof(ChannelInfoLoadingText));

            ChannelDeployInfo = null;
            OnPropertyChanged(nameof(ChannelDeployInfo));

            try
            {
                ClientVersion info = await RobloxDeployment.GetInfo(channel, true);

                ShowChannelWarning = info.IsBehindDefaultChannel;
                OnPropertyChanged(nameof(ShowChannelWarning));

                ChannelDeployInfo = new DeployInfo
                {
                    Version = info.Version,
                    VersionGuid = info.VersionGuid,
                    Timestamp = info.Timestamp?.ToFriendlyString()!
                };

                OnPropertyChanged(nameof(ChannelDeployInfo));
            }
            catch (HttpResponseException ex)
            {
                ShowLoadingError = true;
                OnPropertyChanged(nameof(ShowLoadingError));

                ChannelInfoLoadingText = ex.ResponseMessage.StatusCode switch
                {
                    HttpStatusCode.NotFound => "The specified channel name does not exist.",
                    _ => $"Failed to fetch information! (HTTP {(int)ex.ResponseMessage.StatusCode} - {ex.ResponseMessage.ReasonPhrase})",
                };
                OnPropertyChanged(nameof(ChannelInfoLoadingText));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "An exception occurred while fetching channel information");
                App.Logger.WriteException(LOG_IDENT, ex);

                ShowLoadingError = true;
                OnPropertyChanged(nameof(ShowLoadingError));
                
                ChannelInfoLoadingText = $"Failed to fetch information! ({ex.Message})";
                OnPropertyChanged(nameof(ChannelInfoLoadingText));
            }
        }

        public bool ShowLoadingError { get; set; } = false;
        public bool ShowChannelWarning { get; set; } = false;

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

        public string SelectedChannel
        {
            get => App.Settings.Prop.Channel;
            set
            {
                value = value.Trim();
                Task.Run(() => LoadChannelDeployInfo(value));
                App.Settings.Prop.Channel = value;
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

        public bool ForceRobloxReinstallation
        {
            // wouldnt it be better to check old version guids?
            // what about fresh installs?
            get => String.IsNullOrEmpty(App.State.Prop.PlayerVersionGuid) && String.IsNullOrEmpty(App.State.Prop.StudioVersionGuid);
            set
            {
                if (value)
                {
                    _oldPlayerVersionGuid = App.State.Prop.PlayerVersionGuid;
                    _oldStudioVersionGuid = App.State.Prop.StudioVersionGuid;
                    App.State.Prop.PlayerVersionGuid = "";
                    App.State.Prop.StudioVersionGuid = "";
                }
                else
                {
                    App.State.Prop.PlayerVersionGuid = _oldPlayerVersionGuid;
                    App.State.Prop.StudioVersionGuid = _oldStudioVersionGuid;
                }
            }
        }
    }
}
