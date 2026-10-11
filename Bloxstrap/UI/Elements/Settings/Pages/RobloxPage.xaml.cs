using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class RobloxPage
    {
        public RobloxPage()
        {
            DataContext = new RobloxSettingsViewModel();
            InitializeComponent();
        }

        private void ValidateNumber(object sender, TextCompositionEventArgs e) => e.Handled = !UInt32.TryParse(e.Text, out _);
    }
}
