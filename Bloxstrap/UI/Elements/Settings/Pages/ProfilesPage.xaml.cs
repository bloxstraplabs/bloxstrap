using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class ProfilesPage
    {
        public ProfilesPage()
        {
            DataContext = new ProfilesViewModel(this);
            InitializeComponent();
        }
    }
}
