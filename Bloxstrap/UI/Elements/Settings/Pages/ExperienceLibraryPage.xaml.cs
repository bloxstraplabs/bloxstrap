using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class ExperienceLibraryPage
    {
        public ExperienceLibraryPage()
        {
            DataContext = new ExperienceLibraryViewModel();
            InitializeComponent();
        }
    }
}
