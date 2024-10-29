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

        public FastFlagEditorWarningPage()
        {
            _viewModel = new FastFlagEditorWarningViewModel(this);
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StartCountdown();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopCountdown();
        }
    }
}
