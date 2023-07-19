using System.Windows;

using Bloxstrap.Integrations;
using Bloxstrap.UI.Elements.ContextMenu;

namespace Bloxstrap.UI
{
    public class NotifyIconWrapper : IDisposable
    {
        bool _disposed = false;

        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        private readonly MenuContainer _menuContainer = new();
        private RobloxActivity? _activityWatcher;

        public DiscordRichPresence? RichPresenceIntegration;
        
        EventHandler? _alertClickHandler;

        public NotifyIconWrapper()
        {
            App.Logger.WriteLine("[NotifyIconWrapper::NotifyIconWrapper] Initializing notification area icon");

            _notifyIcon = new()
            {
                Icon = Properties.Resources.IconBloxstrap,
                Text = App.ProjectName,
                Visible = true
            };

            _notifyIcon.MouseClick += MouseClickEventHandler;

            _menuContainer.Dispatcher.BeginInvoke(_menuContainer.ShowDialog);
            _menuContainer.Closing += (_, _) => App.Logger.WriteLine("[NotifyIconWrapper::NotifyIconWrapper] Context menu container closed");
        }

        public void SetActivityWatcher(RobloxActivity activityWatcher)
        {
            if (_activityWatcher is not null)
                return;

            _activityWatcher = activityWatcher;
            _activityWatcher.OnGameJoin += (_, _) => Task.Run(OnGameJoin);
            _activityWatcher.OnGameLeave += OnGameLeave;
        }

        public async void OnGameJoin()
        {
            if (!App.Settings.Prop.ShowServerDetails)
                return;

            App.Logger.WriteLine($"[NotifyIconWrapper::OnActivityGameJoin] Getting game/server information");

            string machineAddress = _activityWatcher!.ActivityMachineAddress;
            string machineLocation = "";

            // basically nobody has a free public access geolocation api that's accurate,
            // the ones that do require an api key which isn't suitable for a client-side application like this
            // so, hopefully this is reliable enough?
            string locationCity = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/city");
            string locationRegion = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/region");
            string locationCountry = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/country");

            locationCity = locationCity.ReplaceLineEndings("");
            locationRegion = locationRegion.ReplaceLineEndings("");
            locationCountry = locationCountry.ReplaceLineEndings("");

            if (String.IsNullOrEmpty(locationCountry))
                machineLocation = "N/A";
            else if (locationCity == locationRegion)
                machineLocation = $"{locationRegion}, {locationCountry}";
            else
                machineLocation = $"{locationCity}, {locationRegion}, {locationCountry}";

            _menuContainer.Dispatcher.Invoke(() => _menuContainer.ServerDetailsMenuItem.Visibility = Visibility.Visible);

            ShowAlert("Connnected to server", $"Location: {machineLocation}\nClick to copy Instance ID", 10, (_, _) => System.Windows.Clipboard.SetText(_activityWatcher.ActivityJobId));
        }

        public void OnGameLeave(object? sender, EventArgs e)
        {
            _menuContainer.Dispatcher.Invoke(() => _menuContainer.ServerDetailsMenuItem.Visibility = Visibility.Collapsed);
        }

        public void MouseClickEventHandler(object? sender, System.Windows.Forms.MouseEventArgs e) 
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            _menuContainer.Activate();
            _menuContainer.ContextMenu.IsOpen = true;
        }

        public void ShowAlert(string caption, string message, int duration, EventHandler? clickHandler)
        {
            string id = Guid.NewGuid().ToString()[..8];

            App.Logger.WriteLine($"[NotifyIconWrapper::ShowAlert] [{id}] Showing alert for {duration} seconds (clickHandler={clickHandler is not null})");
            App.Logger.WriteLine($"[NotifyIconWrapper::ShowAlert] [{id}] {caption}: {message.Replace("\n", "\\n")}");

            _notifyIcon.BalloonTipTitle = caption;
            _notifyIcon.BalloonTipText = message;

            if (_alertClickHandler is not null)
            {
                App.Logger.WriteLine($"[NotifyIconWrapper::ShowAlert] [{id}] Previous alert still present, erasing click handler");
                _notifyIcon.BalloonTipClicked -= _alertClickHandler;
            }

            _alertClickHandler = clickHandler;
            _notifyIcon.BalloonTipClicked += clickHandler;

            _notifyIcon.ShowBalloonTip(duration);

            Task.Run(async () =>
            {
                await Task.Delay(duration * 1000);
             
                _notifyIcon.BalloonTipClicked -= clickHandler;

                App.Logger.WriteLine($"[NotifyIconWrapper::ShowAlert] [{id}] Duration over, erasing current click handler");

                if (_alertClickHandler == clickHandler)
                    _alertClickHandler = null;
                else
                    App.Logger.WriteLine($"[NotifyIconWrapper::ShowAlert] [{id}] Click handler has been overriden by another alert");
            });
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            App.Logger.WriteLine($"[NotifyIconWrapper::Dispose] Disposing NotifyIcon");

            _menuContainer.Dispatcher.Invoke(_menuContainer.Close);
            _notifyIcon.Dispose();

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
