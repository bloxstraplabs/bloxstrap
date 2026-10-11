using Bloxstrap.AppData;
using System.Windows.Threading;

using Bloxstrap.Integrations;
using Bloxstrap.Models;
using Bloxstrap.UI.Elements.Overlay;

namespace Bloxstrap
{
    public class Watcher : IDisposable
    {
        private readonly InterProcessLock _lock = null!;

        private readonly WatcherData? _watcherData;

        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly DiscordRichPresence? RichPresence;

        public readonly StudioRichPresence? StudioRichPresence;

        public readonly SocdResolver? Socd;

        public readonly WindowControl? WindowControl;

        private readonly PerformanceMonitor? _performance;

        private RobloxWindow? _robloxWindow;

        private CrosshairWindow? _crosshair;

        private DispatcherTimer? _windowTimer;

        private string? _gameTitle;

        private System.Drawing.Icon? _gameIcon;

        private System.Drawing.Icon? _customIcon;

        private IntPtr _appliedIcon;

        private IntPtr _appliedIconWindow;

        private (IntPtr Small, IntPtr Big) _originalIcons;

        private bool _gameTitleApplied;

        private readonly List<(CustomIntegration Integration, int ProcessId)> _gameIntegrations = new();

        public static string GetLockName(bool isStudio) => isStudio ? "StudioWatcher" : "Watcher";

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

            if (String.IsNullOrEmpty(watcherDataArg))
            {
#if DEBUG
                string path = new RobloxPlayerData().ExecutablePath;
                if (!File.Exists(path))
                    throw new ApplicationException("Roblox player is not been installed");

                using var gameClientProcess = Process.Start(path);

                _watcherData = new() { ProcessId = gameClientProcess.Id };
#else
                throw new Exception("Watcher data not specified");
#endif
            }
            else
            {
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));
            }

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            _lock = new(GetLockName(_watcherData.IsStudio));

            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            if (_watcherData.IsStudio)
            {
                ActivityWatcher = new(_watcherData.LogFile);

                if (App.Settings.Prop.StudioRichPresence)
                    StudioRichPresence = new(ActivityWatcher);

                _customIcon = WindowIcons.Load(App.Settings.Prop.StudioWindowIcon, App.Settings.Prop.StudioIconPath);

                if (_customIcon is not null)
                    StartStudioIcon();

                return;
            }

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new(_watcherData.LogFile);

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += delegate
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.CloseMainWindow();
                    };
                }

                ActivityWatcher.OnGameJoin += (_, _) => GameStatsTracker.RecordServer(ActivityWatcher.Data);

                if (App.Settings.Prop.PerformanceLog)
                {
                    _performance = new PerformanceMonitor(_watcherData.ProcessId);
                    ActivityWatcher.OnGameJoin += (_, _) => _performance.BeginSession();
                }

                if (App.Settings.Prop.TrackGameStats || App.Settings.Prop.SessionRecap || App.Settings.Prop.PerformanceLog)
                    ActivityWatcher.OnGameLeave += (_, _) => Task.Run(() => OnGameLeave(ActivityWatcher.History.First()));

                if (App.Settings.Prop.CustomIntegrations.Any(x => x.IsGameSpecific))
                {
                    ActivityWatcher.OnGameJoin += (_, _) => StartGameIntegrations(ActivityWatcher.Data);
                    ActivityWatcher.OnGameLeave += (_, _) => StopGameIntegrations(false);
                }

                if (App.Settings.Prop.AutoRejoin)
                    ActivityWatcher.OnGameLeave += (_, _) => Task.Run(() => CheckRejoin(ActivityWatcher.History.First()));

                if (App.Settings.Prop.UseDiscordRichPresence)
                    RichPresence = new(ActivityWatcher);

                if (App.Settings.Prop.WindowControlEnabled)
                    WindowControl = new(ActivityWatcher, _watcherData.ProcessId, new AppData.RobloxPlayerData().Directory);
            }

            var settings = App.Settings.Prop;

            if (settings.SocdEnabled)
                Socd = new SocdResolver(_watcherData.ProcessId, settings.SocdLayout, settings.SocdMode);

            bool anyProfileCrosshair = settings.GameProfiles.Any(x => x.Crosshair == ProfileToggle.On);

            bool gameWindowInfo = ActivityWatcher is not null && (settings.RobloxGameTitle || settings.RobloxGameIcon);

            if (gameWindowInfo)
            {
                ActivityWatcher!.OnGameJoin += (_, _) => Task.Run(() => LoadGameWindowInfo(ActivityWatcher.Data));
                ActivityWatcher.OnGameLeave += (_, _) =>
                {
                    _gameTitle = null;
                    _gameIcon = null;
                };
            }

            _customIcon = WindowIcons.Load(settings.RobloxWindowIcon, settings.RobloxIconPath);

            if (settings.MemoryTrimmer)
                Task.Run(RunMemoryTrimmer);

            if (settings.CloseCrashHandler)
                Task.Run(CloseCrashHandler);

            if (!String.IsNullOrWhiteSpace(settings.RobloxCpuAffinity))
                SetRobloxAffinity(settings.RobloxCpuAffinity);

            if (!String.IsNullOrEmpty(settings.RobloxWindowTitle) || settings.RobloxBorderless || settings.CrosshairEnabled || settings.ThrottleInBackground || anyProfileCrosshair || gameWindowInfo || _customIcon is not null)
                StartWindowFeatures(settings.CrosshairEnabled || anyProfileCrosshair);

            _notifyIcon = new(this);

            if (WindowControl is not null)
                WindowControl.ShowNotification = (title, caption, duration) => _notifyIcon.ShowAlert(title, caption, duration, null);
        }

        private bool ShouldShowCrosshair()
        {
            var activity = ActivityWatcher?.Data;
            var profile = activity is not null && ActivityWatcher!.InGame ? GameProfiles.Find(activity.PlaceId, activity.UniverseId) : null;

            return profile?.Crosshair switch
            {
                ProfileToggle.On => true,
                ProfileToggle.Off => false,
                _ => App.Settings.Prop.CrosshairEnabled
            };
        }

        private void SetRobloxPriority(bool foreground)
        {
            var priority = !foreground ? ProcessPriorityClass.BelowNormal : App.Settings.Prop.RobloxPriority switch
            {
                ProcessPriorityLevel.High => ProcessPriorityClass.High,
                ProcessPriorityLevel.AboveNormal => ProcessPriorityClass.AboveNormal,
                _ => ProcessPriorityClass.Normal
            };

            try
            {
                using var process = Process.GetProcessById(_watcherData!.ProcessId);
                process.PriorityClass = priority;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("Watcher::SetRobloxPriority", ex);
            }
        }

        private void StartWindowFeatures(bool useCrosshair)
        {
            var settings = App.Settings.Prop;

            _robloxWindow = new RobloxWindow(_watcherData!.ProcessId);

            if (useCrosshair)
                _crosshair = new CrosshairWindow();

            bool? wasForeground = null;
            var interval = useCrosshair ? TimeSpan.FromMilliseconds(50) : TimeSpan.FromSeconds(1);

            _windowTimer = new DispatcherTimer(interval, DispatcherPriority.Normal, (_, _) =>
            {
                if (!_robloxWindow.Refresh())
                {
                    _crosshair?.Hide();
                    return;
                }

                bool gameControlsWindow = WindowControl is { Active: true } or { TitleChanged: true };

                if (!gameControlsWindow && !String.IsNullOrEmpty(settings.RobloxWindowTitle))
                {
                    _robloxWindow.ApplyTitle(settings.RobloxWindowTitle);
                }
                else if (!gameControlsWindow && _gameTitle is not null)
                {
                    _robloxWindow.ApplyTitle(_gameTitle);
                    _gameTitleApplied = true;
                }
                else if (!gameControlsWindow && _gameTitleApplied)
                {
                    _robloxWindow.ApplyTitle("Roblox");
                    _gameTitleApplied = false;
                }

                var icon = (_gameIcon ?? _customIcon)?.Handle ?? IntPtr.Zero;

                if (icon != IntPtr.Zero && (icon != _appliedIcon || _robloxWindow.Handle != _appliedIconWindow))
                {
                    if (_robloxWindow.Handle != _appliedIconWindow)
                        _originalIcons = _robloxWindow.GetIcons();

                    _robloxWindow.ApplyIcon(icon);
                    _appliedIcon = icon;
                    _appliedIconWindow = _robloxWindow.Handle;
                }
                else if (icon == IntPtr.Zero && _appliedIcon != IntPtr.Zero && _robloxWindow.Handle == _appliedIconWindow)
                {
                    _robloxWindow.RestoreIcons(_originalIcons);
                    _appliedIcon = IntPtr.Zero;
                }

                if (settings.RobloxBorderless && WindowControl is not { Active: true })
                    _robloxWindow.ApplyBorderless();

                if (settings.ThrottleInBackground)
                {
                    bool foreground = _robloxWindow.IsForeground;

                    if (foreground != wasForeground)
                    {
                        wasForeground = foreground;
                        SetRobloxPriority(foreground);
                    }
                }

                if (_crosshair is not null)
                {
                    if (ShouldShowCrosshair())
                        _crosshair.Follow(_robloxWindow);
                    else if (_crosshair.IsVisible)
                        _crosshair.Hide();
                }
            }, Dispatcher.CurrentDispatcher);

            _windowTimer.Start();
        }

        private async Task LoadGameWindowInfo(ActivityData activity)
        {
            const string LOG_IDENT = "Watcher::LoadGameWindowInfo";

            try
            {
                if (activity.UniverseDetails is null)
                {
                    await UniverseDetails.FetchSingle(activity.UniverseId);
                    activity.UniverseDetails = UniverseDetails.LoadFromCache(activity.UniverseId);
                }

                var details = activity.UniverseDetails;

                if (details is null || ActivityWatcher?.Data != activity)
                    return;

                if (App.Settings.Prop.RobloxGameTitle)
                {
                    _gameTitle = details.Data.Name;

                    if (App.Settings.Prop.RobloxGameTitlePlayerCount)
                        _gameTitle += $" ({details.Data.Playing:N0} playing)";
                }

                if (App.Settings.Prop.RobloxGameIcon && !String.IsNullOrEmpty(details.Thumbnail.ImageUrl))
                    _gameIcon = WindowIcons.FromImage(await App.HttpClient.GetByteArrayAsync(details.Thumbnail.ImageUrl));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private void StartStudioIcon()
        {
            _robloxWindow = new RobloxWindow(_watcherData!.ProcessId);

            _windowTimer = new DispatcherTimer(TimeSpan.FromSeconds(2), DispatcherPriority.Normal, (_, _) =>
            {
                if (!_robloxWindow.Refresh() || _customIcon is null)
                    return;

                if (_customIcon.Handle != _appliedIcon || _robloxWindow.Handle != _appliedIconWindow)
                {
                    _robloxWindow.ApplyIcon(_customIcon.Handle);
                    _appliedIcon = _customIcon.Handle;
                    _appliedIconWindow = _robloxWindow.Handle;
                }
            }, Dispatcher.CurrentDispatcher);

            _windowTimer.Start();
        }

        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        private static extern bool EmptyWorkingSet(IntPtr process);

        private async Task RunMemoryTrimmer()
        {
            const string LOG_IDENT = "Watcher::RunMemoryTrimmer";

            while (true)
            {
                await Task.Delay(Math.Clamp(App.Settings.Prop.MemoryTrimIntervalSeconds, 15, 3600) * 1000);

                try
                {
                    using var process = Process.GetProcessById(_watcherData!.ProcessId);

                    long before = process.WorkingSet64 / 1024 / 1024;

                    if (before < App.Settings.Prop.MemoryTrimThresholdMb)
                        continue;

                    EmptyWorkingSet(process.Handle);
                    process.Refresh();

                    App.Logger.WriteLine(LOG_IDENT, $"Trimmed Roblox from {before} MB to {process.WorkingSet64 / 1024 / 1024} MB");
                }
                catch (ArgumentException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }

        private async Task CloseCrashHandler()
        {
            for (int i = 0; i < 12; i++)
            {
                await Task.Delay(5000);

                foreach (var process in Process.GetProcessesByName("RobloxCrashHandler"))
                {
                    try
                    {
                        App.Logger.WriteLine("Watcher::CloseCrashHandler", $"Closing RobloxCrashHandler (pid={process.Id})");
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException("Watcher::CloseCrashHandler", ex);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }

        private void SetRobloxAffinity(string cores)
        {
            const string LOG_IDENT = "Watcher::SetRobloxAffinity";

            long mask = 0;

            foreach (string part in cores.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string[] range = part.Split('-', StringSplitOptions.TrimEntries);

                if (!int.TryParse(range[0], out int start))
                    continue;

                int end = range.Length > 1 && int.TryParse(range[1], out int parsed) ? parsed : start;

                for (int core = Math.Max(start, 0); core <= end && core < Environment.ProcessorCount && core < 64; core++)
                    mask |= 1L << core;
            }

            if (mask == 0)
                return;

            try
            {
                using var process = Process.GetProcessById(_watcherData!.ProcessId);
                process.ProcessorAffinity = new IntPtr(mask);
                App.Logger.WriteLine(LOG_IDENT, $"Set affinity to {cores} (mask={mask:X})");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private async Task OnGameLeave(ActivityData activity)
        {
            var details = activity.UniverseDetails ?? UniverseDetails.LoadFromCache(activity.UniverseId);
            string name = details?.Data.Name ?? Strings.Common_Unknown;

            var performance = _performance?.EndSession(activity, name);

            if (App.Settings.Prop.TrackGameStats)
                await GameStatsTracker.Record(activity);

            if (!App.Settings.Prop.SessionRecap)
                return;

            double seconds = ((activity.TimeLeft ?? DateTime.Now) - activity.TimeJoined).TotalSeconds;

            if (seconds < 60)
                return;

            string message = String.Format(Strings.Watcher_SessionRecap_Message, name, UI.ViewModels.Settings.PlaytimeViewModel.FormatDuration(seconds));

            if (App.Settings.Prop.TrackGameStats && GameStatsTracker.Load().Games.TryGetValue(activity.UniverseId, out var stats))
                message += " " + String.Format(Strings.Watcher_SessionRecap_Total, UI.ViewModels.Settings.PlaytimeViewModel.FormatDuration(stats.TotalSeconds));

            if (performance is not null)
                message += " " + String.Format(Strings.Watcher_SessionRecap_Performance, performance.AverageCpu.ToString("0"), performance.AverageGpu.ToString("0"), performance.PeakMemoryMb.ToString("N0"));

            _notifyIcon?.ShowAlert(Strings.Watcher_SessionRecap_Title, message, 8, null);
        }

        private DateTime _lastBreakReminder = DateTime.Now;

        private void CheckBreakReminder()
        {
            int minutes = App.Settings.Prop.BreakReminderMinutes;

            if (minutes <= 0 || (DateTime.Now - _lastBreakReminder).TotalMinutes < minutes)
                return;

            _lastBreakReminder = DateTime.Now;
            _notifyIcon?.ShowAlert(Strings.Watcher_BreakReminder_Title, String.Format(Strings.Watcher_BreakReminder_Message, UI.ViewModels.Settings.PlaytimeViewModel.FormatDuration(minutes * 60)), 10, null);
        }

        private void StartGameIntegrations(ActivityData activity)
        {
            const string LOG_IDENT = "Watcher::StartGameIntegrations";

            foreach (var integration in App.Settings.Prop.CustomIntegrations.Where(x => x.IsGameSpecific))
            {
                long id = long.Parse(integration.PlaceId.Trim());

                if (id != activity.PlaceId && id != activity.UniverseId)
                    continue;

                if (_gameIntegrations.Any(x => x.Integration == integration))
                    continue;

                App.Logger.WriteLine(LOG_IDENT, $"Launching game integration '{integration.Name}' for {id}");

                try
                {
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = integration.Location,
                        Arguments = integration.LaunchArgs.Replace("\r\n", " "),
                        WorkingDirectory = Path.GetDirectoryName(integration.Location),
                        UseShellExecute = true
                    });

                    _gameIntegrations.Add((integration, process?.Id ?? 0));
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to launch integration '{integration.Name}'");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }

        private void StopGameIntegrations(bool robloxClosed)
        {
            foreach (var entry in _gameIntegrations.ToList())
            {
                bool close = entry.Integration.CloseWithGame || (robloxClosed && entry.Integration.AutoClose);

                if (!close)
                    continue;

                if (entry.ProcessId != 0)
                    CloseProcess(entry.ProcessId);

                _gameIntegrations.Remove(entry);
            }
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private async Task CheckRejoin(ActivityData data)
        {
            const string LOG_IDENT = "Watcher::CheckRejoin";

            await Task.Delay(2000);

            if (ActivityWatcher is null || ActivityWatcher.InGame)
                return;

            int reason = ActivityWatcher.LastDisconnectReason;
            var category = RejoinRules.GetCategory(reason);

            App.Logger.WriteLine(LOG_IDENT, $"Left game with reason {reason} ({category})");

            if (category is null || !RejoinRules.IsEnabled(category.Value))
                return;

            if (!RejoinRules.TryRecordAttempt())
            {
                App.Logger.WriteLine(LOG_IDENT, "Not rejoining, attempt limit reached");
                _notifyIcon?.ShowAlert(Strings.Watcher_Rejoin_LimitTitle, Strings.Watcher_Rejoin_LimitMessage, 8, null);
                return;
            }

            bool sameServer = category is RejoinCategory.Idle or RejoinCategory.ConnectionLost && data.ServerType == ServerType.Public;

            await RejoinServer(data, sameServer);
        }

        private async Task RejoinServer(ActivityData data, bool sameServer)
        {
            const string LOG_IDENT = "Watcher::RejoinServer";

            App.Logger.WriteLine(LOG_IDENT, $"Rejoining {data} (sameServer={sameServer})");

            await Task.Delay(Math.Clamp(App.Settings.Prop.RejoinDelaySeconds, 0, 60) * 1000);

            try
            {
                Utilities.ShellExecute(sameServer ? data.GetInviteDeeplink(false) : $"roblox://experiences/start?placeId={data.PlaceId}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to start rejoin");
                App.Logger.WriteException(LOG_IDENT, ex);
                return;
            }

            CloseProcess(_watcherData!.ProcessId, true);
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired || _watcherData is null)
                return;

            if (App.Settings.Prop.MultiInstanceLaunching && !_watcherData.IsStudio)
                RobloxSingleton.Hold();

            ActivityWatcher?.Start();

            while (Utilities.GetProcessesSafe().Any(x => x.Id == _watcherData.ProcessId))
            {
                if (!_watcherData.IsStudio)
                    CheckBreakReminder();

                await Task.Delay(1000);
            }

            if (App.Settings.Prop.TrackGameStats && ActivityWatcher is { InGame: true } && !_watcherData.IsStudio)
                await GameStatsTracker.Record(ActivityWatcher.Data);

            if (_watcherData.RestoreRobloxSettings is not null)
            {
                await Task.Delay(3000);
                GameProfiles.RestoreRobloxSettings(_watcherData.RestoreRobloxSettings);
            }

            StopGameIntegrations(true);

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            if (App.LaunchSettings.TestModeFlag.Active)
                Process.Start(Paths.Process, "-settings -testmode");
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            _notifyIcon?.Dispose();
            RichPresence?.Dispose();
            StudioRichPresence?.Dispose();
            Socd?.Dispose();
            WindowControl?.Dispose();
            _performance?.Dispose();
            RobloxSingleton.Release();

            _windowTimer?.Dispatcher.Invoke(() =>
            {
                _windowTimer.Stop();
                _crosshair?.Close();
            });

            GC.SuppressFinalize(this);
        }
    }
}
