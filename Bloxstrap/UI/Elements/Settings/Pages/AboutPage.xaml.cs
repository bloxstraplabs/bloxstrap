using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
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
