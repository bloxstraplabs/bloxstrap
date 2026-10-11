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
    }
}
