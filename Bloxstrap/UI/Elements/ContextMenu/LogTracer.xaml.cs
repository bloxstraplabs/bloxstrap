using System.Windows.Controls;

using Bloxstrap.UI.ViewModels.ContextMenu;

namespace Bloxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for LogTracer.xaml
    /// </summary>
    public partial class LogTracer
    {
        private readonly LogTracerViewModel _viewModel;

        public LogTracer(RobloxActivity activityWatcher)
        {
            _viewModel = new LogTracerViewModel(this, activityWatcher);
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e) => ScrollViewer.ScrollToEnd();
    }
}
