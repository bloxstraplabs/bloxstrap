using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Elements.Menu.Pages
{
    /// <summary>
    /// Interaction logic for PreInstallPage.xaml
    /// </summary>
    public partial class PreInstallPage
    {
        public PreInstallPage()
        {
            DataContext = new PreInstallViewModel();
            InitializeComponent();
        }
    }
}
