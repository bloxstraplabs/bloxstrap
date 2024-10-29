using Bloxstrap.UI.ViewModels.Settings;
using System.Windows;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagEditorWarningPage.xaml
    /// </summary>
    public partial class FastFlagEditorWarningPage
    {
        private FastFlagEditorWarningViewModel _viewModel;
        private bool _initialLoad = false;

        public FastFlagEditorWarningPage()
        {
            _viewModel = new FastFlagEditorWarningViewModel(this);
            DataContext = _viewModel;
            _viewModel.StartCountdown();

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to reset timer

            if (!_initialLoad)
            {
                _initialLoad = true;
                return;
            }

            _viewModel.StartCountdown();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopCountdown();
        }
    }
}
