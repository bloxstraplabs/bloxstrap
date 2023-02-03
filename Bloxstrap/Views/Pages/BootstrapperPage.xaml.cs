using Bloxstrap.ViewModels;

namespace Bloxstrap.Views.Pages
{
    /// <summary>
    /// Interaction logic for BootstrapperPage.xaml
    /// </summary>
    public partial class BootstrapperPage
    {
        public BootstrapperPage()
        {
            DataContext = new BootstrapperViewModel(this);
            InitializeComponent();
        }
    }
}
