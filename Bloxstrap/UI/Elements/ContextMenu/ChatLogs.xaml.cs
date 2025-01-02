using Bloxstrap.Integrations;
using Bloxstrap.UI.ViewModels.ContextMenu;

namespace Bloxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for ChatLogs.xaml
    /// </summary>
    public partial class ChatLogs
    {
        public ChatLogs(ActivityWatcher watcher)
        {
            var viewModel = new ChatLogsViewModel(watcher);

            viewModel.RequestCloseEvent += (_, _) => Close();

            DataContext = viewModel;
            InitializeComponent();
        }
    }
}