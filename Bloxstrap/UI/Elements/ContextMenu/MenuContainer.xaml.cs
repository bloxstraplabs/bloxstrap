using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using Bloxstrap.Integrations;

namespace Bloxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for NotifyIconMenu.xaml
    /// </summary>
    public partial class MenuContainer
    {
        // i wouldve gladly done this as mvvm but turns out that data binding just does not work with menuitems for some reason so idk this sucks

        private readonly ActivityWatcher? _activityWatcher;
        private readonly DiscordRichPresence? _richPresenceHandler;

        private LogTracer? _logTracerWindow;
        private ServerInformation? _serverInformationWindow;

        public MenuContainer(ActivityWatcher? activityWatcher, DiscordRichPresence? richPresenceHandler)
        {
            InitializeComponent();
            
            _activityWatcher = activityWatcher;
            _richPresenceHandler = richPresenceHandler;

            if (_activityWatcher is not null)
            {
                if (App.Settings.Prop.OhHeyYouFoundMe)
                    LogTracerMenuItem.Visibility = Visibility.Visible;
             
                _activityWatcher.OnGameJoin += ActivityWatcher_OnGameJoin;
                _activityWatcher.OnGameLeave += ActivityWatcher_OnGameLeave;
            }

            if (_richPresenceHandler is not null)
                RichPresenceMenuItem.Visibility = Visibility.Visible;

            VersionTextBlock.Text = $"{App.ProjectName} v{App.Version}";
        }

        public void ShowServerInformationWindow()
        {
            if (_serverInformationWindow is null)
            {
                _serverInformationWindow = new ServerInformation(_activityWatcher!);
                _serverInformationWindow.Closed += (_, _) => _serverInformationWindow = null;
            }

            if (!_serverInformationWindow.IsVisible)
                _serverInformationWindow.Show();

            _serverInformationWindow.Activate();
        }

        private void ActivityWatcher_OnGameJoin(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (_activityWatcher?.ActivityServerType == ServerType.Public)
                    InviteDeeplinkMenuItem.Visibility = Visibility.Visible;

                ServerDetailsMenuItem.Visibility = Visibility.Visible;
            });
        }

        private void ActivityWatcher_OnGameLeave(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                InviteDeeplinkMenuItem.Visibility = Visibility.Collapsed;
                ServerDetailsMenuItem.Visibility = Visibility.Collapsed;

                _serverInformationWindow?.Close();
            });
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            // this is an awful hack lmao im so sorry to anyone who reads this
            // this is done to register the context menu wrapper as a tool window so it doesnt appear in the alt+tab switcher
            // https://stackoverflow.com/a/551847/11852173

            HWND hWnd = (HWND)new WindowInteropHelper(this).Handle;

            int exStyle = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            exStyle |= 0x00000080; //NativeMethods.WS_EX_TOOLWINDOW;
            PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
        }

        private void Window_Closed(object sender, EventArgs e) => App.Logger.WriteLine("MenuContainer::Window_Closed", "Context menu container closed");

        private void RichPresenceMenuItem_Click(object sender, RoutedEventArgs e) => _richPresenceHandler?.SetVisibility(((MenuItem)sender).IsChecked);

        private void InviteDeeplinkMenuItem_Click(object sender, RoutedEventArgs e) => Clipboard.SetText($"roblox://experiences/start?placeId={_activityWatcher?.ActivityPlaceId}&gameInstanceId={_activityWatcher?.ActivityJobId}");

        private void ServerDetailsMenuItem_Click(object sender, RoutedEventArgs e) => ShowServerInformationWindow();

        private void LogTracerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_logTracerWindow is null)
            {
                _logTracerWindow = new LogTracer(_activityWatcher!);
                _logTracerWindow.Closed += (_, _) => _logTracerWindow = null;;
            }

            if (!_logTracerWindow.IsVisible)
                _logTracerWindow.Show();

            _logTracerWindow.Activate();
        }
    }
}
