using System.Windows.Forms;

using Bloxstrap.UI.Elements;

namespace Bloxstrap.UI
{
    public class NotifyIconWrapper : IDisposable
    {
        bool _disposed = false;

        private readonly NotifyIcon _notifyIcon;
        private readonly NotifyIconMenu _contextMenuWrapper = new();
        
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

            _contextMenuWrapper.Dispatcher.BeginInvoke(_contextMenuWrapper.ShowDialog);

            _contextMenuWrapper.Closing += (_, _) => App.Logger.WriteLine("[NotifyIconWrapper::NotifyIconWrapper] Context menu wrapper closing");
        }

        public void MouseClickEventHandler(object? sender, MouseEventArgs e) 
        {
            if (e.Button != MouseButtons.Right)
                return;

            _contextMenuWrapper.Activate();
            _contextMenuWrapper.ContextMenu.IsOpen = true;
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

            _contextMenuWrapper.Dispatcher.Invoke(_contextMenuWrapper.Close);
            _notifyIcon.Dispose();

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
