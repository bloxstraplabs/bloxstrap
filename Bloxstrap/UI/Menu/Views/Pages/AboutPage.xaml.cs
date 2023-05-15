using Bloxstrap.UI.Menu.ViewModels;

namespace Bloxstrap.UI.Menu.Views.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage
    {
        public AboutPage()
        {
            DataContext = new AboutViewModel();
            InitializeComponent();
        }
    }
}
