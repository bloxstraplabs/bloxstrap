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

namespace Bloxstrap.UI.Elements.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /// The logic behind this wizard-like interface is full of gross hacks, but there's no easy way to do this and I've tried to 
    /// make it as nice and MVVM-"""conformant""" as can possibly be ¯\_(ツ)_/¯
    /// 
    /// Page ViewModels can request changing of navigation button states through the following call flow:
    /// - Page ViewModel holds event for requesting button state change
    /// - Page CodeBehind subscribes to event on page creation
    /// - Page ViewModel invokes event when ready
    /// - Page CodeBehind receives it, gets MainWindow, and directly calls MainWindow.SetButtonEnabled()
    /// - MainWindow.SetButtonEnabled() directly calls MainWindowViewModel.SetButtonEnabled() which does the thing a voila
    /// 
    /// Page ViewModels can also be notified of when the next page button has been pressed and stop progression if needed through a callback
    /// - MainWindow has a single-set Func<bool> property named NextPageCallback which is reset on every page load
    /// - This callback is called when the next page button is pressed
    /// - Page CodeBehind gets MainWindow and sets the callback to its own local function on page load
    /// - CodeBehind's local function then directly calls the ViewModel to do whatever it needs to do
    /// 
    /// TODO: theme selection

    public partial class MainWindow : WpfUiWindow, INavigationWindow
    {
        internal readonly MainWindowViewModel _viewModel = new();

        private Type _currentPage = typeof(WelcomePage);

        private List<Type> _pages = new() { typeof(WelcomePage), typeof(InstallPage), typeof(CompletionPage) };

        public Func<bool>? NextPageCallback;

        public NextAction CloseAction = NextAction.Terminate;

        public bool Finished => _currentPage == _pages.Last();

        public MainWindow()
        {
            _viewModel.CloseWindowRequest += (_, _) => CloseWindow();

            _viewModel.PageRequest += (_, type) =>
            {
                if (type == "next")
                    NextPage();
                else if (type == "back")
                    BackPage();
            };

            DataContext = _viewModel;
            InitializeComponent();

            App.Logger.WriteLine("MainWindow::MainWindow", "Initializing installer");

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

            var result = Frontend.ShowMessageBox("Are you sure you want to cancel the installation?", MessageBoxImage.Warning, MessageBoxButton.YesNo);

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
