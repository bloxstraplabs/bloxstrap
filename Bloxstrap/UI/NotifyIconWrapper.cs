using System.Windows;

using Bloxstrap.Integrations;
using Bloxstrap.UI.Elements.ContextMenu;

namespace Bloxstrap.UI
{
    public class NotifyIconWrapper : IDisposable
    {
        // lol who needs properly structured mvvm and xaml when you have the absolute catastrophe that this is

        private bool _disposing = false;

        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        private MenuContainer? _menuContainer;
        
        private ActivityWatcher? _activityWatcher;
        private DiscordRichPresence? _richPresenceHandler;
        private int? _processId;

        EventHandler? _alertClickHandler;

        public NotifyIconWrapper()
        {
            App.Logger.WriteLine("NotifyIconWrapper::NotifyIconWrapper", "Initializing notification area icon");

            _notifyIcon = new()
            {
                Icon = Properties.Resources.IconBloxstrap,
                Text = App.ProjectName,
                Visible = true
            };

            _notifyIcon.MouseClick += MouseClickEventHandler;
        }

        #region Handler registers
        public void SetRichPresenceHandler(DiscordRichPresence richPresenceHandler)
        {
            if (_richPresenceHandler is not null)
                return;

            _richPresenceHandler = richPresenceHandler;
        }

        public void SetActivityWatcher(ActivityWatcher activityWatcher)
        {
            if (_activityWatcher is not null)
                return;

            _activityWatcher = activityWatcher;

            if (App.Settings.Prop.ShowServerDetails)
                _activityWatcher.OnGameJoin += (_, _) => Task.Run(OnGameJoin);
        }

        public void SetProcessId(int processId)
        {
            if (_processId is not null)
                return;

            _processId = processId;
        }
        #endregion

        #region Context menu
        public void InitializeContextMenu()
        {
            if (_menuContainer is not null || _disposing)
                return;

            App.Logger.WriteLine("NotifyIconWrapper::InitializeContextMenu", "Initializing context menu");

            _menuContainer = new(_activityWatcher, _richPresenceHandler, _processId);
            _menuContainer.ShowDialog();
        }

        public void MouseClickEventHandler(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right || _menuContainer is null)
                return;

            _menuContainer.Activate();
            _menuContainer.ContextMenu.IsOpen = true;
        }
        #endregion

        #region Activity handlers
        public async void OnGameJoin()
        {
            string serverLocation = await _activityWatcher!.GetServerLocation();

            ShowAlert(
                $"Connected to {_activityWatcher.ActivityServerType.ToString().ToLower()} server", 
                $"Located at {serverLocation}\nClick for more information", 
                10, 
                (_, _) => _menuContainer?.ShowServerInformationWindow()
            );
        }
        #endregion

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

            _menuContainer?.Dispatcher.Invoke(_menuContainer.Close);
            _notifyIcon?.Dispose();


            GC.SuppressFinalize(this);
        }
    }
}
