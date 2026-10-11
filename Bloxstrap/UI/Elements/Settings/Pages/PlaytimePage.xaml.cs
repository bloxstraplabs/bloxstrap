using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class PlaytimePage
    {
        public PlaytimePage()
        {
            DataContext = new PlaytimeViewModel();
            InitializeComponent();
        }
    }
}
