using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class GameProfilesPage
    {
        public GameProfilesPage()
        {
            DataContext = new GameProfilesViewModel();
            InitializeComponent();
        }
    }
}
