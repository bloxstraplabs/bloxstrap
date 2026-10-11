using System.Collections.ObjectModel;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class IntegrationsViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand AddIntegrationCommand => new RelayCommand(AddIntegration);

        public ICommand DeleteIntegrationCommand => new RelayCommand(DeleteIntegration);

        public ICommand BrowseIntegrationLocationCommand => new RelayCommand(BrowseIntegrationLocation);

        private void AddIntegration()
        {
            CustomIntegrations.Add(new CustomIntegration()
            {
                Name = Strings.Menu_Integrations_Custom_NewIntegration
            });

            SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;

            OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        private void DeleteIntegration()
        {
            if (SelectedCustomIntegration is null)
                return;

            CustomIntegrations.Remove(SelectedCustomIntegration);

            if (CustomIntegrations.Count > 0)
            {
                SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
                OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            }

            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        private void BrowseIntegrationLocation()
        {
            if (SelectedCustomIntegration is null)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.Menu_AllFiles}|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            SelectedCustomIntegration.Name = dialog.SafeFileName;
            SelectedCustomIntegration.Location = dialog.FileName;
            OnPropertyChanged(nameof(SelectedCustomIntegration));
        }

        public bool ActivityTrackingEnabled
        {
            get => App.Settings.Prop.EnableActivityTracking;
            set
            {
                App.Settings.Prop.EnableActivityTracking = value;

                if (!value)
                {
                    ShowServerDetailsEnabled = value;
                    DisableAppPatchEnabled = value;
                    AutoRejoinEnabled = value;
                    PlaytimeCounterEnabled = value;
                    DiscordActivityEnabled = value;
                    DiscordActivityJoinEnabled = value;

                    OnPropertyChanged(nameof(ShowServerDetailsEnabled));
                    OnPropertyChanged(nameof(DisableAppPatchEnabled));
                    OnPropertyChanged(nameof(AutoRejoinEnabled));
                    OnPropertyChanged(nameof(PlaytimeCounterEnabled));
                    OnPropertyChanged(nameof(DiscordActivityEnabled));
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                }
            }
        }

        public bool ShowServerDetailsEnabled
        {
            get => App.Settings.Prop.ShowServerDetails;
            set => App.Settings.Prop.ShowServerDetails = value;
        }

        public bool AutoRejoinEnabled
        {
            get => App.Settings.Prop.AutoRejoin;
            set => App.Settings.Prop.AutoRejoin = value;
        }

        public bool PlaytimeCounterEnabled
        {
            get => App.Settings.Prop.PlaytimeCounter;
            set => App.Settings.Prop.PlaytimeCounter = value;
        }

        public bool DisableRobloxScreenshots
        {
            get => RobloxCaptureBlocker.IsBlocked(RobloxCaptureBlocker.ScreenshotsFolder);
            set => RobloxCaptureBlocker.SetBlocked(RobloxCaptureBlocker.ScreenshotsFolder, value);
        }

        public bool DisableRobloxRecordings
        {
            get => RobloxCaptureBlocker.IsBlocked(RobloxCaptureBlocker.RecordingsFolder);
            set => RobloxCaptureBlocker.SetBlocked(RobloxCaptureBlocker.RecordingsFolder, value);
        }

        public bool DiscordActivityEnabled
        {
            get => App.Settings.Prop.UseDiscordRichPresence;
            set
            {
                App.Settings.Prop.UseDiscordRichPresence = value;

                if (!value)
                {
                    DiscordActivityJoinEnabled = value;
                    DiscordAccountOnProfile = value;
                    CustomStatusDisplayEnabled = value;
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                    OnPropertyChanged(nameof(DiscordAccountOnProfile));
                    OnPropertyChanged(nameof(CustomStatusDisplayEnabled));
                }
            }
        }

        public bool DiscordActivityJoinEnabled
        {
            get => !App.Settings.Prop.HideRPCButtons;
            set => App.Settings.Prop.HideRPCButtons = !value;
        }

        public bool DiscordAccountOnProfile
        {
            get => App.Settings.Prop.ShowAccountOnRichPresence;
            set => App.Settings.Prop.ShowAccountOnRichPresence = value;
        }

        public bool StudioRichPresenceEnabled
        {
            get => App.Settings.Prop.StudioRichPresence;
            set => App.Settings.Prop.StudioRichPresence = value;
        }

        public bool CustomStatusDisplayEnabled
        {
            get => App.Settings.Prop.UseCustomStatusDisplay;
            set => App.Settings.Prop.UseCustomStatusDisplay = value;
        }

        public IEnumerable<TrayDoubleClickAction> TrayDoubleClickActions { get; } = Enum.GetValues<TrayDoubleClickAction>();

        public TrayDoubleClickAction TrayDoubleClickAction
        {
            get => App.Settings.Prop.TrayDoubleClickAction;
            set => App.Settings.Prop.TrayDoubleClickAction = value;
        }

        public bool ShowServerUptimeEnabled
        {
            get => App.Settings.Prop.ShowServerUptime;
            set => App.Settings.Prop.ShowServerUptime = value;
        }

        public bool UsingReistrapEnabled
        {
            get => App.Settings.Prop.ShowUsingReistrapInRichPresence;
            set => App.Settings.Prop.ShowUsingReistrapInRichPresence = value;
        }

        public bool StudioShowGameName
        {
            get => App.Settings.Prop.StudioShowGameName;
            set => App.Settings.Prop.StudioShowGameName = value;
        }

        public bool StudioShowGameIcon
        {
            get => App.Settings.Prop.StudioShowGameIcon;
            set => App.Settings.Prop.StudioShowGameIcon = value;
        }

        public bool StudioShowTesting
        {
            get => App.Settings.Prop.StudioShowTesting;
            set => App.Settings.Prop.StudioShowTesting = value;
        }

        public bool StudioShowGameButton
        {
            get => App.Settings.Prop.StudioShowGameButton;
            set => App.Settings.Prop.StudioShowGameButton = value;
        }

        public bool StudioShowScript
        {
            get => App.Settings.Prop.StudioShowScript;
            set => App.Settings.Prop.StudioShowScript = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.Prop.UseDisableAppPatch;
            set => App.Settings.Prop.UseDisableAppPatch = value;
        }
        public ObservableCollection<CustomIntegration> CustomIntegrations
        {
            get => App.Settings.Prop.CustomIntegrations;
            set => App.Settings.Prop.CustomIntegrations = value;
        }

        public CustomIntegration? SelectedCustomIntegration { get; set; }
        public int SelectedCustomIntegrationIndex { get; set; }
        public bool IsCustomIntegrationSelected => SelectedCustomIntegration is not null;

        // Window control

        public bool WindowControlEnabled
        {
            get => App.Settings.Prop.WindowControlEnabled;
            set => App.Settings.Prop.WindowControlEnabled = value;
        }

        public bool WindowControlMove
        {
            get => App.Settings.Prop.WindowControlMove;
            set => App.Settings.Prop.WindowControlMove = value;
        }

        public bool WindowControlTitle
        {
            get => App.Settings.Prop.WindowControlTitle;
            set => App.Settings.Prop.WindowControlTitle = value;
        }

        public bool WindowControlTransparency
        {
            get => App.Settings.Prop.WindowControlTransparency;
            set => App.Settings.Prop.WindowControlTransparency = value;
        }

        public bool WindowControlColor
        {
            get => App.Settings.Prop.WindowControlColor;
            set => App.Settings.Prop.WindowControlColor = value;
        }

        public bool WindowControlWallpaper
        {
            get => App.Settings.Prop.WindowControlWallpaper;
            set => App.Settings.Prop.WindowControlWallpaper = value;
        }

        public bool WindowControlDesktop
        {
            get => App.Settings.Prop.WindowControlDesktop;
            set => App.Settings.Prop.WindowControlDesktop = value;
        }

        public bool WindowControlAllGames
        {
            get => App.Settings.Prop.WindowControlAllGames;
            set => App.Settings.Prop.WindowControlAllGames = value;
        }

        public IEnumerable<WindowMonitorMode> WindowMonitorModes { get; } = Enum.GetValues<WindowMonitorMode>();

        public WindowMonitorMode WindowControlMonitor
        {
            get => App.Settings.Prop.WindowControlMonitor;
            set => App.Settings.Prop.WindowControlMonitor = value;
        }

        public int WindowControlFps
        {
            get => App.Settings.Prop.WindowControlFps;
            set
            {
                App.Settings.Prop.WindowControlFps = value;
                OnPropertyChanged(nameof(WindowControlFps));
            }
        }

        private readonly JsonManager<WindowPermissions> _windowPermissions = new();

        public string WindowPermissionsText
        {
            get
            {
                _windowPermissions.Load(false);
                return String.Format(Strings.Menu_Integrations_WindowControl_Permissions_Description, _windowPermissions.Prop.Allowed.Count, _windowPermissions.Prop.Blocked.Count);
            }
        }

        public ICommand ResetWindowPermissionsCommand => new RelayCommand(() =>
        {
            _windowPermissions.Prop = new();
            _windowPermissions.Save();
            OnPropertyChanged(nameof(WindowPermissionsText));
        });

        // Rejoin

        public bool RejoinOnIdle
        {
            get => App.Settings.Prop.RejoinOnIdle;
            set => App.Settings.Prop.RejoinOnIdle = value;
        }

        public bool RejoinOnConnectionLost
        {
            get => App.Settings.Prop.RejoinOnConnectionLost;
            set => App.Settings.Prop.RejoinOnConnectionLost = value;
        }

        public bool RejoinOnShutdown
        {
            get => App.Settings.Prop.RejoinOnShutdown;
            set => App.Settings.Prop.RejoinOnShutdown = value;
        }

        public bool RejoinOnTeleportFailure
        {
            get => App.Settings.Prop.RejoinOnTeleportFailure;
            set => App.Settings.Prop.RejoinOnTeleportFailure = value;
        }

        public int RejoinMaxAttempts
        {
            get => App.Settings.Prop.RejoinMaxAttempts;
            set
            {
                App.Settings.Prop.RejoinMaxAttempts = value;
                OnPropertyChanged(nameof(RejoinMaxAttempts));
            }
        }

        public int RejoinDelaySeconds
        {
            get => App.Settings.Prop.RejoinDelaySeconds;
            set
            {
                App.Settings.Prop.RejoinDelaySeconds = value;
                OnPropertyChanged(nameof(RejoinDelaySeconds));
            }
        }
    }
}
