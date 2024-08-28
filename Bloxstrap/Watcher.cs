using Bloxstrap.Integrations;
using System.CodeDom;
using System.Security.Permissions;

namespace Bloxstrap
{
    public class Watcher : IDisposable
    {
        private int _gameClientPid = 0;

        private readonly InterProcessLock _lock = new("Watcher");

        private readonly List<int> _autoclosePids = new();

        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly DiscordRichPresence? RichPresence;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            string? watcherData = App.LaunchSettings.WatcherFlag.Data;

#if DEBUG
            if (String.IsNullOrEmpty(watcherData))
            {
                string path = Path.Combine(Paths.Versions, App.State.Prop.PlayerVersionGuid, "RobloxPlayerBeta.exe");
                using var gameClientProcess = Process.Start(path);
                _gameClientPid = gameClientProcess.Id;
            }
#else
            if (String.IsNullOrEmpty(watcherData))
                throw new Exception("Watcher data not specified");
#endif

            if (!String.IsNullOrEmpty(watcherData) && _gameClientPid == 0)
            {
                var split = watcherData.Split(';');

                if (split.Length == 0)
                    _ = int.TryParse(watcherData, out _gameClientPid);

                if (split.Length >= 1)
                    _ = int.TryParse(split[0], out _gameClientPid);

                if (split.Length >= 2)
                {
                    foreach (string strPid in split[0].Split(';'))
                    {
                        if (int.TryParse(strPid, out int pid) && pid != 0)
                            _autoclosePids.Add(pid);
                    }
                }
            }

            if (_gameClientPid == 0)
                throw new Exception("Watcher data is invalid");

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new();

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += (_, _) =>
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_gameClientPid);
                        process.CloseMainWindow();
                    };
                }

                if (App.Settings.Prop.UseDiscordRichPresence)
                    RichPresence = new(ActivityWatcher);
            }

            _notifyIcon = new(this);
        }

        public void KillRobloxProcess() => KillProcess(_gameClientPid);

        public void KillProcess(int pid)
        {
            using var process = Process.GetProcessById(pid);
            
            App.Logger.WriteLine("Watcher::KillProcess", $"Killing process '{process.ProcessName}' (PID {process.Id})");

            if (process.HasExited)
            {
                App.Logger.WriteLine("Watcher::KillProcess", $"PID {process.Id} has already exited");
                return;
            }

            process.Kill();
            process.Close();
        }

        public void CloseProcess(int pid)
        {
            using var process = Process.GetProcessById(pid);

            App.Logger.WriteLine("Watcher::CloseProcess", $"Closing process '{process.ProcessName}' (PID {process.Id})");

            if (process.HasExited)
            {
                App.Logger.WriteLine("Watcher::CloseProcess", $"PID {process.Id} has already exited");
                return;
            }

            process.CloseMainWindow();
            process.Close();
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired)
                return;

            ActivityWatcher?.Start();

            while (Utilities.GetProcessesSafe().Any(x => x.Id == _gameClientPid))
                await Task.Delay(1000);

            foreach (int pid in _autoclosePids)
                CloseProcess(pid);
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            _notifyIcon?.Dispose();
            RichPresence?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
