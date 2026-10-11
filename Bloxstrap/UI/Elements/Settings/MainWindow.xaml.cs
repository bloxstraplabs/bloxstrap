using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using Bloxstrap.UI.Elements.Controls;
using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings
{
    public partial class MainWindow : INavigationWindow
    {
        private Models.Persistable.WindowState _state => App.State.Prop.SettingsWindow;

        protected override bool AllowCustomBackground => true;

        private const int SizeVersion = 2;

        private readonly MainWindowViewModel _viewModel;

        private readonly Dictionary<string, (string PageTag, string Header)> _searchEntries = new();

        private string? _pendingSearchHeader;

        private static readonly Dictionary<string, string> _pageDescriptions = new()
        {
            ["integrations"] = Strings.Menu_Integrations_Description,
            ["bootstrapper"] = Strings.Menu_Behaviour_Description,
            ["playtime"] = Strings.Menu_Playtime_Description,
            ["quickplay"] = Strings.Menu_QuickPlay_Description,
            ["regionselector"] = Strings.Menu_RegionSelector_Description,
            ["mods"] = Strings.Menu_Mods_Description,
            ["appearance"] = Strings.Menu_Appearance_Description,
            ["fastflags"] = Strings.Menu_FastFlags_Description,
            ["roblox"] = Strings.Menu_Roblox_Description,
            ["gameprofiles"] = Strings.Menu_GameProfiles_Description,
            ["shortcuts"] = Strings.Menu_Shortcuts_Description,
            ["reistrap"] = Strings.Menu_Bloxstrap_Description,
            ["fastflageditor"] = Strings.Menu_FastFlagEditor_Description
        };

        public bool RestartRequested => _viewModel.RestartRequested;

        public MainWindow(bool showAlreadyRunningWarning)
        {
            _viewModel = new MainWindowViewModel();

            _viewModel.RequestSaveNoticeEvent += (_, _) => SettingsSavedSnackbar.Show();
            _viewModel.RequestCloseWindowEvent += (_, _) => Close();

            DataContext = _viewModel;

            InitializeComponent();

            App.Logger.WriteLine("MainWindow", "Initializing settings window");

            if (showAlreadyRunningWarning)
                ShowAlreadyRunningSnackbar();

            LoadState();
            BuildSearchIndex();

            if (App.Settings.Prop.DisableAnimations)
                RootNavigation.TransitionDuration = 0;
        }

        public void LoadState()
        {
            if (_state.Left > SystemParameters.VirtualScreenWidth)
                _state.Left = 0;

            if (_state.Top > SystemParameters.VirtualScreenHeight)
                _state.Top = 0;

            if (_state.SizeVersion == SizeVersion && _state.Width >= MinWidth)
                this.Width = _state.Width;

            if (_state.SizeVersion == SizeVersion && _state.Height >= MinHeight)
                this.Height = _state.Height;

            this.Width = Math.Min(this.Width, SystemParameters.WorkArea.Width);
            this.Height = Math.Min(this.Height, SystemParameters.WorkArea.Height);

            if (_state.Left > 0 && _state.Top > 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = _state.Left;
                this.Top = _state.Top;
            }
        }

        private async void ShowAlreadyRunningSnackbar()
        {
            await Task.Delay(500);
            AlreadyRunningSnackbar.Show();
        }

        private void RootNavigation_Navigated(INavigation sender, RoutedNavigationEventArgs e)
        {
            if (sender.Current is not NavigationItem item)
                return;

            PageIcon.Symbol = item.Icon;
            PageTitle.Text = item.Content?.ToString() ?? "";
            PageDescription.Text = _pageDescriptions.TryGetValue(item.PageTag, out var description) ? description : "";
            PageDescription.Visibility = String.IsNullOrEmpty(PageDescription.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        #region Search

        private void BuildSearchIndex()
        {
            var pageNames = RootNavigation.Items.OfType<NavigationItem>().ToDictionary(x => x.PageTag, x => x.Content?.ToString() ?? "");

            foreach (var (pageTag, stringKey) in SettingsSearch.Entries)
            {
                string? header = typeof(Strings).GetProperty(stringKey)?.GetValue(null) as string;

                if (String.IsNullOrEmpty(header))
                    continue;

                string display = pageNames.TryGetValue(pageTag, out var pageName) ? $"{header} ({pageName})" : header;

                _searchEntries.TryAdd(display, (pageTag, header));
            }

            SearchBox.ItemsSource = _searchEntries.Keys.ToList();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SearchBox.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Hidden;
            SearchBox.Focus();
        }

        private void CloseSearch()
        {
            SearchBox.Visibility = Visibility.Collapsed;
            SearchButton.Visibility = RootNavigation.IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is DependencyObject focus && SearchBox.IsAncestorOf(focus))
                return;

            if (!SearchBox.IsSuggestionListOpen)
                CloseSearch();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseSearch();
            }
            else if (e.Key == Key.Enter)
            {
                var match = SearchBox.FilteredItemsSource?.FirstOrDefault();

                if (match is not null)
                    GoToSearchEntry(match);
            }
        }

        private void SearchBox_SuggestionChosen(object sender, RoutedEventArgs e) => GoToSearchEntry(SearchBox.Text);

        private void GoToSearchEntry(string display)
        {
            if (!_searchEntries.TryGetValue(display, out var entry))
                return;

            CloseSearch();

            if (RootNavigation.Current?.PageTag == entry.PageTag)
            {
                ScrollToHeader(entry.Header);
                return;
            }

            _pendingSearchHeader = entry.Header;
            RootNavigation.Navigate(entry.PageTag);
        }

        private void RootFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_pendingSearchHeader is null)
                return;

            string header = _pendingSearchHeader;
            _pendingSearchHeader = null;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => ScrollToHeader(header));
        }

        private void ScrollToHeader(string header)
        {
            if (RootFrame.Content is not DependencyObject page)
                return;

            var target = FindHeaderElement(page, header);

            if (target is null)
                return;

            var scrollViewer = FindAncestor<ScrollViewer>(target);

            if (scrollViewer is null)
            {
                target.BringIntoView();
                return;
            }

            var offset = target.TranslatePoint(new Point(0, 0), scrollViewer).Y;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset - 8);
        }

        private static FrameworkElement? FindHeaderElement(DependencyObject parent, string header)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
            {
                if (child is OptionControl option && option.Header == header)
                    return option;

                if (child is TextBlock text && text.Text == header)
                    return text;

                var found = FindHeaderElement(child, header);

                if (found is not null)
                    return found;
            }

            return null;
        }

        private static T? FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            var current = System.Windows.Media.VisualTreeHelper.GetParent(element);

            while (current is not null && current is not T)
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);

            return current as T;
        }

        #endregion

        #region Launch menu

        private void LaunchMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = LaunchMenuButton.ContextMenu;

            TestModeMenuItem.IsChecked = _viewModel.TestModeEnabled;

            menu.PlacementTarget = LaunchMenuButton;
            menu.IsOpen = true;
        }

        private void TestModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TestModeEnabled = TestModeMenuItem.IsChecked;
            TestModeMenuItem.IsChecked = _viewModel.TestModeEnabled;
        }

        #endregion

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void WpfUiWindow_Closing(object sender, CancelEventArgs e)
        {
            if (App.FastFlags.Changed || App.PendingSettingTasks.Any())
            {
                var result = Frontend.ShowMessageBox(Strings.Menu_UnsavedChanges, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    _viewModel.CancelPendingClose();
                }
            }

            _state.SizeVersion = SizeVersion;
            _state.Width = this.Width;
            _state.Height = this.Height;

            _state.Top = this.Top;
            _state.Left = this.Left;

            App.State.Save();
        }

        private void WpfUiWindow_Closed(object sender, EventArgs e)
        {
            if (_viewModel.LaunchAfterClose != LaunchMode.None)
                LaunchHandler.LaunchRoblox(_viewModel.LaunchAfterClose);
            else if (App.LaunchSettings.TestModeFlag.Active && !_viewModel.RestartRequested)
                LaunchHandler.LaunchRoblox(LaunchMode.Player);
            else
                App.SoftTerminate();
        }
    }
}
