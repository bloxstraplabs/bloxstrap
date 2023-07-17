using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Menu.Pages
{
    /// <summary>
    /// Interaction logic for InstallationPage.xaml
    /// </summary>
    public partial class InstallationPage
    {
        public InstallationPage()
        {
            DataContext = new InstallationViewModel();
            InitializeComponent();
        }
    }
}
