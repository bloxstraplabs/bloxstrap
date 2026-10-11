using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class RegionSelectorPage
    {
        public RegionSelectorPage()
        {
            DataContext = new RegionSelectorViewModel();
            InitializeComponent();
        }

        private void GameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is RegionSelectorViewModel viewModel)
                viewModel.SearchCommand.Execute(null);
        }
    }
}
