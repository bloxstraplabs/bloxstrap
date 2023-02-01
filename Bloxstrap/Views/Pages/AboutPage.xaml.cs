using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bloxstrap.Helpers;
using CommunityToolkit.Mvvm.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Bloxstrap.Views.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage
    {
        public AboutPage()
        {
            DataContext = new AboutPageViewModel();
            InitializeComponent();
        }
    }

    public class AboutPageViewModel
    {
        public ICommand OpenWebpageCommand => new RelayCommand<string>(OpenWebpage);

        private void OpenWebpage(string? location)
        {
            if (location is null)
                return;

            Utilities.OpenWebsite(location);
        }
    }
}
