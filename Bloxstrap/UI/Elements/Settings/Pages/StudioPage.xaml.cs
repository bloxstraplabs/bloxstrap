using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class StudioPage
    {
        public StudioPage()
        {
            DataContext = new StudioViewModel(this);
            InitializeComponent();
        }
    }
}
