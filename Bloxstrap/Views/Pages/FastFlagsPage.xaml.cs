using Bloxstrap.ViewModels;
using System.Windows.Input;
using System;

namespace Bloxstrap.Views.Pages
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
