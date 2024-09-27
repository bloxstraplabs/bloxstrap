using Bloxstrap.UI.ViewModels.About;

namespace Bloxstrap.UI.Elements.About.Pages
{
    /// <summary>
    /// Interaction logic for SupportersPage.xaml
    /// </summary>
    public partial class SupportersPage
    {
        public SupportersPage()
        {
            DataContext = new SupportersViewModel();
            InitializeComponent();
        }
    }
}
