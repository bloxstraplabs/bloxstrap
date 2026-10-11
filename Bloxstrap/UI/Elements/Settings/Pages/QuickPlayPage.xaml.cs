using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class QuickPlayPage
    {
        public QuickPlayPage()
        {
            DataContext = new QuickPlayViewModel();
            InitializeComponent();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is QuickPlayViewModel viewModel)
                viewModel.SearchCommand.Execute(null);
        }
    }
}
