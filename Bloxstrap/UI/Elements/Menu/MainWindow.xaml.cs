using System.Windows.Controls;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Elements.Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        public MainWindow()
        {
            App.Logger.WriteLine("MainWindow::MainWindow", "Initializing menu");

            DataContext = new MainWindowViewModel(this);
            SetTheme();
            InitializeComponent();
        }

        public void SetTheme()
        {
            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();
        }

        public void OpenWiki(object? sender, EventArgs e) => Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/wiki");

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods
    }
}
