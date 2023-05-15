using System;
using System.Windows.Input;

using Bloxstrap.UI.Menu.ViewModels;

namespace Bloxstrap.UI.Menu.Views.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class FastFlagsPage
    {
        public FastFlagsPage()
        {
            DataContext = new FastFlagsViewModel();
            InitializeComponent();
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) => e.Handled = !Int32.TryParse(e.Text, out int _);
    }
}
