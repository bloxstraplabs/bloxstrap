using System.Windows;
using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Elements.Menu.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class FastFlagsPage
    {
        public FastFlagsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to synchronize with editor page
            DataContext = new FastFlagsViewModel();
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) => e.Handled = !Int32.TryParse(e.Text, out int _);
    }
}
