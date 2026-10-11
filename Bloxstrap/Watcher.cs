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

        private RobloxWindow? _robloxWindow;

        private CrosshairWindow? _crosshair;

        private DispatcherTimer? _windowTimer;

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
                StudioRichPresence = new(ActivityWatcher);
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

                if (App.Settings.Prop.TrackGameStats || App.Settings.Prop.SessionRecap)
                    ActivityWatcher.OnGameLeave += (_, _) => Task.Run(() => OnGameLeave(ActivityWatcher.History.First()));

                if (App.Settings.Prop.AutoRejoin)
                    ActivityWatcher.OnRejoinableDisconnect += (_, data) => Task.Run(() => RejoinServer(data));

                if (App.Settings.Prop.UseDiscordRichPresence)
                    RichPresence = new(ActivityWatcher);
            }

            var settings = App.Settings.Prop;

            bool anyProfileCrosshair = settings.GameProfiles.Any(x => x.Crosshair == ProfileToggle.On);

            if (!String.IsNullOrEmpty(settings.RobloxWindowTitle) || settings.RobloxBorderless || settings.CrosshairEnabled || settings.ThrottleInBackground || anyProfileCrosshair)
                StartWindowFeatures(settings.CrosshairEnabled || anyProfileCrosshair);

            _notifyIcon = new(this);
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

                if (!String.IsNullOrEmpty(settings.RobloxWindowTitle))
                    _robloxWindow.ApplyTitle(settings.RobloxWindowTitle);

                if (settings.RobloxBorderless)
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

        private async Task OnGameLeave(ActivityData activity)
        {
            if (App.Settings.Prop.TrackGameStats)
                await GameStatsTracker.Record(activity);

            if (!App.Settings.Prop.SessionRecap)
                return;

            double seconds = ((activity.TimeLeft ?? DateTime.Now) - activity.TimeJoined).TotalSeconds;

            if (seconds < 60)
                return;

            var details = activity.UniverseDetails ?? UniverseDetails.LoadFromCache(activity.UniverseId);
            string name = details?.Data.Name ?? Strings.Common_Unknown;

            string message = String.Format(Strings.Watcher_SessionRecap_Message, name, UI.ViewModels.Settings.PlaytimeViewModel.FormatDuration(seconds));

            if (App.Settings.Prop.TrackGameStats && GameStatsTracker.Load().Games.TryGetValue(activity.UniverseId, out var stats))
                message += " " + String.Format(Strings.Watcher_SessionRecap_Total, UI.ViewModels.Settings.PlaytimeViewModel.FormatDuration(stats.TotalSeconds));

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

        private async Task RejoinServer(ActivityData data)
        {
            const string LOG_IDENT = "Watcher::RejoinServer";

            App.Logger.WriteLine(LOG_IDENT, $"Rejoining {data}");

            await Task.Delay(3000);

            try
            {
                Utilities.ShellExecute(data.GetInviteDeeplink(false));
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
