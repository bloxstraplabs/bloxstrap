using System.Windows;

using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void NavigateToSection(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { Tag: string pageTag }
                && Window.GetWindow(this) is INavigationWindow window)
            {
                window.GetNavigation().Navigate(pageTag);
            }
        }
    }
}
