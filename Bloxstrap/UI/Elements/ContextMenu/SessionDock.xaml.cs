using System.Windows;

using Bloxstrap.Integrations;
using Bloxstrap.UI.ViewModels.ContextMenu;

namespace Bloxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// A compact companion panel for an active Roblox session.
    /// </summary>
    public partial class SessionDock
    {
        private readonly Action _showServerInformation;
        private readonly Action _showGameHistory;
        private readonly Action _openProfiles;

        public SessionDock(Watcher watcher, Action showServerInformation, Action showGameHistory, Action openProfiles)
        {
            _showServerInformation = showServerInformation;
            _showGameHistory = showGameHistory;
            _openProfiles = openProfiles;

            DataContext = new ServerInformationViewModel(watcher);
            InitializeComponent();

            Activated += (_, _) => Topmost = true;
        }

        private void ServerInfoButton_Click(object sender, RoutedEventArgs e) => ShowModalWindow(_showServerInformation);

        private void HistoryButton_Click(object sender, RoutedEventArgs e) => ShowModalWindow(_showGameHistory);

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            _openProfiles();
        }

        private void ShowModalWindow(Action showWindow)
        {
            Topmost = false;

            try
            {
                showWindow();
            }
            finally
            {
                if (IsLoaded)
                {
                    Topmost = true;
                    Activate();
                }
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(App.ProjectHelpLink))
                Utilities.ShellExecute(App.ProjectHelpLink);
        }
    }
}
