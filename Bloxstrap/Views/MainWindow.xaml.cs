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
        private readonly IDialogService _dialogService = new DialogService();

        public MainWindow()
        {
            DataContext = new MainWindowViewModel(this, _dialogService);
            SetTheme();
            InitializeComponent();
            _dialogService.SetDialogControl(RootDialog);
        }

        public void SetTheme()
        {
            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
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
