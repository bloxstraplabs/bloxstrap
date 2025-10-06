using Bloxstrap.Integrations;
using Bloxstrap.UI.Elements.About;
using Bloxstrap.UI.Elements.ContextMenu;

namespace Bloxstrap.UI
{
    public class NotifyIconWrapper : IDisposable
    {
        // lol who needs properly structured mvvm and xaml when you have the absolute catastrophe that this is

        private bool _disposing = false;

        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        
        private readonly MenuContainer _menuContainer;
        
        private readonly Watcher _watcher;

        private ActivityWatcher? _activityWatcher => _watcher.ActivityWatcher;

        EventHandler? _alertClickHandler;

        public NotifyIconWrapper(Watcher watcher)
        {
            App.Logger.WriteLine("NotifyIconWrapper::NotifyIconWrapper", "Initializing notification area icon");

            _watcher = watcher;

            _notifyIcon = new(new System.ComponentModel.Container())
            {
                Icon = Properties.Resources.IconBloxstrap,
                Text = "Fishstrap",
                Visible = true
            };

            _notifyIcon.MouseClick += MouseClickEventHandler;

            if (_activityWatcher is not null && (App.Settings.Prop.ShowServerDetails || App.Settings.Prop.ShowServerUptime))
                _activityWatcher.OnGameJoin += OnGameJoin;

            _menuContainer = new(_watcher);
            _menuContainer.Show();
        }

        #region Context menu
        public void MouseClickEventHandler(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            _menuContainer.Activate();
            _menuContainer.ContextMenu.IsOpen = true;
        }
        #endregion

        #region Activity handlers
        public async void OnGameJoin(object? sender, EventArgs e)
        {
            if (_activityWatcher is null)
                return;

            string title = _activityWatcher.Data.ServerType switch
            {
                ServerType.Public => Strings.ContextMenu_ServerInformation_Notification_Title_Public,
                ServerType.Private => Strings.ContextMenu_ServerInformation_Notification_Title_Private,
                ServerType.Reserved => Strings.ContextMenu_ServerInformation_Notification_Title_Reserved,
                _ => ""
            };

            bool locationActive = App.Settings.Prop.ShowServerDetails;
            bool uptimeActive = App.Settings.Prop.ShowServerUptime;

            string? serverLocation = "";
            if (locationActive)
                serverLocation = await _activityWatcher.Data.QueryServerLocation();

            string? serverUptime = "";
            if (uptimeActive)
            {
                DateTime? serverTime = await _activityWatcher.Data.QueryServerTime();
                TimeSpan _serverUptime = DateTime.UtcNow - serverTime.Value;

                if (_serverUptime.TotalSeconds > 60)
                    serverUptime = Time.FormatTimeSpan(_serverUptime);
                else
                    serverUptime = Strings.ContextMenu_ServerInformation_Notification_ServerNotTracked;
            }

            if (
                string.IsNullOrEmpty(serverLocation) && locationActive ||
                string.IsNullOrEmpty(serverUptime) && uptimeActive
                )
                return;

            string notifContent = Strings.Common_UnknownStatus;

            // since we dont have an actual localization, this is probably the best way of doing that
            if (locationActive && !uptimeActive)
                notifContent = String.Format(Strings.ContextMenu_ServerInformation_Notification_Text, serverLocation);
            else if (!locationActive && uptimeActive)
                notifContent = String.Format(Strings.ContextMenu_ServerInformationUptime_Notification_Text, serverUptime);
            else if (locationActive && uptimeActive)
                notifContent = String.Format(Strings.ContextMenu_ServerInformationUptimeAndLocation_Notification_Text, serverLocation, serverUptime);

            ShowAlert(
                title,
                notifContent,
                10,
                (_, _) => _menuContainer.ShowServerInformationWindow()
            );
        }
        #endregion

        // we may need to create our own handler for this, because this sorta sucks
        public void ShowAlert(string caption, string message, int duration, EventHandler? clickHandler)
        {
            string id = Guid.NewGuid().ToString()[..8];

            string LOG_IDENT = $"NotifyIconWrapper::ShowAlert.{id}";

            App.Logger.WriteLine(LOG_IDENT, $"Showing alert for {duration} seconds (clickHandler={clickHandler is not null})");
            App.Logger.WriteLine(LOG_IDENT, $"{caption}: {message.Replace("\n", "\\n")}");

            _notifyIcon.BalloonTipTitle = caption;
            _notifyIcon.BalloonTipText = message;

            if (_alertClickHandler is not null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Previous alert still present, erasing click handler");
                _notifyIcon.BalloonTipClicked -= _alertClickHandler;
            }

            _alertClickHandler = clickHandler;
            _notifyIcon.BalloonTipClicked += clickHandler;

            _notifyIcon.ShowBalloonTip(duration);

            Task.Run(async () =>
            {
                await Task.Delay(duration * 1000);
             
                _notifyIcon.BalloonTipClicked -= clickHandler;

                App.Logger.WriteLine(LOG_IDENT, "Duration over, erasing current click handler");

                if (_alertClickHandler == clickHandler)
                    _alertClickHandler = null;
                else
                    App.Logger.WriteLine(LOG_IDENT, "Click handler has been overridden by another alert");
            });
        }

        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;

            App.Logger.WriteLine("NotifyIconWrapper::Dispose", "Disposing NotifyIcon");

            _menuContainer.Dispatcher.Invoke(_menuContainer.Close);
            _notifyIcon.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
