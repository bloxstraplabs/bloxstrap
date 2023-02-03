using System.Windows.Controls;
using System;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Bloxstrap.Enums;
using Bloxstrap.ViewModels;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Appearance;

namespace Bloxstrap.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        public MainWindow()
        {
            DataContext = new MainWindowViewModel(this);
            SetTheme();
            InitializeComponent();
        }

        public void SetTheme()
        {
            var theme = ThemeType.Light;

            if (App.Settings.Theme.GetFinal() == Enums.Theme.Dark)
                theme = ThemeType.Dark;

            _themeService.SetTheme(theme);
            _themeService.SetSystemAccent();
        }

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
