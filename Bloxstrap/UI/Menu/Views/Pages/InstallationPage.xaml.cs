using Bloxstrap.UI.Menu.ViewModels;

namespace Bloxstrap.UI.Menu.Views.Pages
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
