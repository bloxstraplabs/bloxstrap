using Bloxstrap.UI.ViewModels.Settings;
using System.Windows;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagEditorWarningPage.xaml
    /// </summary>
    public partial class FastFlagEditorWarningPage
    {
        private bool _initialLoad = false;

        public FastFlagEditorWarningPage()
        {
            DataContext = new FastFlagEditorWarningViewModel(this);
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

            DataContext = new FastFlagEditorWarningViewModel(this);
        }
    }
}
