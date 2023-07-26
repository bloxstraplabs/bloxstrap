using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Elements.Menu.Pages
{
    /// <summary>
    /// Interaction logic for ConnectivityTestPage.xaml
    /// </summary>
    public partial class ConnectivityTestPage
    {
        public ConnectivityTestPage()
        {
            DataContext = new ConnectivityTestViewModel();
            InitializeComponent();
        }
    }
}
