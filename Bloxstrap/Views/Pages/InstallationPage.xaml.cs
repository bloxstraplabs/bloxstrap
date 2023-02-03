using Bloxstrap.ViewModels;

namespace Bloxstrap.Views.Pages
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
