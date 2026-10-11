using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Models.APIs.Roblox;
using Bloxstrap.Models.SettingTasks;
using Bloxstrap.RobloxInterfaces;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool MultiInstanceLaunching
        {
            get => App.Settings.Prop.MultiInstanceLaunching;
            set => App.Settings.Prop.MultiInstanceLaunching = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool BlockRobloxUpdates
        {
            get => App.Settings.Prop.BlockRobloxUpdates;
            set => App.Settings.Prop.BlockRobloxUpdates = value;
        }

        public bool UseStaticVersionFolder
        {
            get => App.Settings.Prop.UseStaticVersionFolder;
            set => App.Settings.Prop.UseStaticVersionFolder = value;
        }

        public IEnumerable<CleanerAge> CleanerAges { get; } = Enum.GetValues<CleanerAge>();

        public CleanerAge CleanerAge
        {
            get => App.Settings.Prop.CleanerAge;
            set => App.Settings.Prop.CleanerAge = value;
        }

        public bool CleanRobloxLogs
        {
            get => App.Settings.Prop.CleanRobloxLogs;
            set => App.Settings.Prop.CleanRobloxLogs = value;
        }

        public bool CleanRobloxCache
        {
            get => App.Settings.Prop.CleanRobloxCache;
            set => App.Settings.Prop.CleanRobloxCache = value;
        }

        public bool CleanAppLogs
        {
            get => App.Settings.Prop.CleanAppLogs;
            set => App.Settings.Prop.CleanAppLogs = value;
        }

        public bool CleanAppDownloads
        {
            get => App.Settings.Prop.CleanAppDownloads;
            set => App.Settings.Prop.CleanAppDownloads = value;
        }

        public ICommand CleanNowCommand => new AsyncRelayCommand(CleanNow);

        private static async Task CleanNow()
        {
            if (Process.GetProcessesByName(App.RobloxPlayerAppName).Any())
            {
                Frontend.ShowMessageBox(Strings.Menu_Behaviour_Cleaner_RobloxRunning, MessageBoxImage.Information);
                return;
            }

            long freed = await Task.Run(() => Cleaner.Run(true));
            Frontend.ShowMessageBox(String.Format(Strings.Menu_Behaviour_Cleaner_Done, freed / 1024.0 / 1024.0), MessageBoxImage.Information);
        }

        public IEnumerable<ProcessPriorityLevel> PriorityLevels { get; } = Enum.GetValues<ProcessPriorityLevel>();

        public ProcessPriorityLevel RobloxPriority
        {
            get => App.Settings.Prop.RobloxPriority;
            set => App.Settings.Prop.RobloxPriority = value;
        }

        public bool ThrottleInBackground
        {
            get => App.Settings.Prop.ThrottleInBackground;
            set => App.Settings.Prop.ThrottleInBackground = value;
        }

        public IReadOnlyList<string> KnownChannels { get; } = new[] { "production", "zcanary", "zintegration" };

        public ChannelTask PlayerChannelTask { get; } = new("RobloxPlayer");

        public ChannelTask StudioChannelTask { get; } = new("RobloxStudio");

        public class ChannelResult
        {
            public string Name { get; init; } = "";

            public string Status { get; init; } = "";

            public bool Available { get; init; }
        }

        private static readonly string[] CandidateChannels = { "production", "zcanary", "zintegration", "zlive", "zlive1", "znext", "zflag", "zavatar" };

        public ObservableCollection<ChannelResult> FoundChannels { get; } = new();

        public bool IsFindingChannels { get; private set; }

        public ICommand FindChannelsCommand => new AsyncRelayCommand(FindChannels);

        public ICommand UseChannelCommand => new RelayCommand<ChannelResult>(UseChannel);

        private static string GetVersionPath(string channel, string binaryType) => channel == Deployment.DefaultChannel
            ? $"/v2/client-version/{binaryType}"
            : $"/v2/client-version/{binaryType}/channel/{channel}";

        private static async Task<ChannelResult> GetChannelResult(string channel)
        {
            try
            {
                var info = await Http.GetJson<ClientVersion>("https://clientsettingscdn.roblox.com" + GetVersionPath(channel, "WindowsPlayer"));
                return new ChannelResult { Name = channel, Status = String.Format(Strings.Menu_Behaviour_Channels_Public, info.Version), Available = true };
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new ChannelResult { Name = channel, Status = Strings.Menu_Behaviour_Channels_NotPublic };
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
            {
                return new ChannelResult { Name = channel, Status = Strings.Menu_Behaviour_Channels_NotFound };
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BehaviourViewModel::GetChannelResult", ex);
                return new ChannelResult { Name = channel, Status = Strings.Menu_Behaviour_Channels_Failed };
            }
        }

        private async Task FindChannels()
        {
            if (IsFindingChannels)
                return;

            IsFindingChannels = true;
            OnPropertyChanged(nameof(IsFindingChannels));

            FoundChannels.Clear();

            var names = CandidateChannels
                .Concat(new[] { PlayerChannelTask.OriginalState, StudioChannelTask.OriginalState })
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.Length > 0)
                .Distinct();

            var results = await Task.WhenAll(names.Select(GetChannelResult));

            foreach (var result in results.OrderByDescending(x => x.Available).ThenBy(x => x.Name == Deployment.DefaultChannel ? "" : x.Name))
                FoundChannels.Add(result);

            IsFindingChannels = false;
            OnPropertyChanged(nameof(IsFindingChannels));
        }

        private void UseChannel(ChannelResult? result)
        {
            if (result is null || !result.Available)
                return;

            PlayerChannelTask.NewState = result.Name;
            OnPropertyChanged(nameof(PlayerChannelTask));
        }

        public ICommand CheckPlayerChannelCommand => new AsyncRelayCommand(() => CheckChannel(PlayerChannelTask.NewState, "WindowsPlayer"));

        public ICommand CheckStudioChannelCommand => new AsyncRelayCommand(() => CheckChannel(StudioChannelTask.NewState, "WindowsStudio64"));

        private static async Task CheckChannel(string channel, string binaryType)
        {
            const string LOG_IDENT = "BehaviourViewModel::CheckChannel";

            channel = channel.Trim().ToLowerInvariant();

            if (String.IsNullOrEmpty(channel))
                channel = Deployment.DefaultChannel;

            try
            {
                var info = await Http.GetJson<ClientVersion>("https://clientsettingscdn.roblox.com" + GetVersionPath(channel, binaryType));
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Behaviour_Channel_Available, channel, info.Version, info.VersionGuid), MessageBoxImage.Information);
            }
            catch (HttpRequestException ex) when (Deployment.BadChannelCodes.Contains(ex.StatusCode))
            {
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Behaviour_Channel_Unavailable, channel), MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, MessageBoxImage.Error);
            }
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }

        public string RobloxCpuAffinity
        {
            get => App.Settings.Prop.RobloxCpuAffinity;
            set => App.Settings.Prop.RobloxCpuAffinity = value;
        }

        public bool CloseCrashHandler
        {
            get => App.Settings.Prop.CloseCrashHandler;
            set => App.Settings.Prop.CloseCrashHandler = value;
        }

        public bool ForceRobloxLanguage
        {
            get => App.Settings.Prop.ForceRobloxLanguage;
            set => App.Settings.Prop.ForceRobloxLanguage = value;
        }

        public IEnumerable<ChannelChangeMode> ChannelChangeModes { get; } = Enum.GetValues<ChannelChangeMode>();

        public ChannelChangeMode ChannelChangeMode
        {
            get => App.Settings.Prop.ChannelChangeMode;
            set => App.Settings.Prop.ChannelChangeMode = value;
        }

        public string PlayerVersionOverride
        {
            get => App.Settings.Prop.PlayerVersionOverride;
            set => App.Settings.Prop.PlayerVersionOverride = value;
        }

        public string StudioVersionOverride
        {
            get => App.Settings.Prop.StudioVersionOverride;
            set => App.Settings.Prop.StudioVersionOverride = value;
        }

        public int DownloadThreads
        {
            get => App.Settings.Prop.DownloadThreads;
            set
            {
                App.Settings.Prop.DownloadThreads = value;
                OnPropertyChanged(nameof(DownloadThreads));
            }
        }

        public bool MemoryTrimmer
        {
            get => App.Settings.Prop.MemoryTrimmer;
            set => App.Settings.Prop.MemoryTrimmer = value;
        }

        public int MemoryTrimThresholdMb
        {
            get => App.Settings.Prop.MemoryTrimThresholdMb;
            set
            {
                App.Settings.Prop.MemoryTrimThresholdMb = value;
                OnPropertyChanged(nameof(MemoryTrimThresholdMb));
            }
        }

        public int MemoryTrimIntervalSeconds
        {
            get => App.Settings.Prop.MemoryTrimIntervalSeconds;
            set
            {
                App.Settings.Prop.MemoryTrimIntervalSeconds = value;
                OnPropertyChanged(nameof(MemoryTrimIntervalSeconds));
            }
        }
    }
}
