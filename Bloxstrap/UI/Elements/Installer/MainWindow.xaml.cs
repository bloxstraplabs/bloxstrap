using System.Windows.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using System.ComponentModel;
using System.Windows;

using Bloxstrap.UI.ViewModels.Installer;
using Bloxstrap.UI.Elements.Installer.Pages;
using Bloxstrap.UI.Elements.Base;
using System.Windows.Media.Animation;
using System.Reflection.Metadata.Ecma335;
using Bloxstrap.Resources;

namespace Bloxstrap.UI.Elements.Installer
{

    public partial class MainWindow : WpfUiWindow, INavigationWindow
    {
        internal readonly MainWindowViewModel _viewModel = new();

        private Type _currentPage = typeof(WelcomePage);

        private List<Type> _pages = new() { typeof(WelcomePage), typeof(InstallPage), typeof(CompletionPage) };

        private DateTimeOffset _lastNavigation = DateTimeOffset.Now;

        public Func<bool>? NextPageCallback;

        public NextAction CloseAction = NextAction.Terminate;

        public bool Finished => _currentPage == _pages.Last();

        public MainWindow()
        {
            _viewModel.CloseWindowRequest += (_, _) => CloseWindow();

            _viewModel.PageRequest += (_, type) =>
            {
                // debounce
                if (DateTimeOffset.Now.Subtract(_lastNavigation).TotalMilliseconds < 500)
                    return;

                if (type == "next")
                    NextPage();
                else if (type == "back")
                    BackPage();

                _lastNavigation = DateTimeOffset.Now;
            };

            DataContext = _viewModel;
            InitializeComponent();

            App.Logger.WriteLine("MainWindow", "Initializing installer window");

            Closing += new CancelEventHandler(MainWindow_Closing);
        }

        void NextPage()
        {
            if (NextPageCallback is not null && !NextPageCallback())
                return;

            if (_currentPage == _pages.Last())
                return;

            var page = _pages[_pages.IndexOf(_currentPage) + 1];

            Navigate(page);

            SetButtonEnabled("next", page != _pages.Last());
            SetButtonEnabled("back", true);
        }

        void BackPage()
        {
            if (_currentPage == _pages.First())
                return;

            var page = _pages[_pages.IndexOf(_currentPage) - 1];

            Navigate(page);

            SetButtonEnabled("next", true);
            SetButtonEnabled("back", page != _pages.First());
        }

        void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (Finished)
                return;

            var result = Frontend.ShowMessageBox(Strings.Installer_ShouldCancel, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                e.Cancel = true;
        }

        public void SetNextButtonText(string text) => _viewModel.SetNextButtonText(text);

        public void SetButtonEnabled(string type, bool state) => _viewModel.SetButtonEnabled(type, state);
        
        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType)
        {
            _currentPage = pageType;
            NextPageCallback = null;
            return RootNavigation.Navigate(pageType);
        }

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods
    }
}
