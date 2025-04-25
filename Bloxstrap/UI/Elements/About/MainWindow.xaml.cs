using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Appearance;

namespace Bloxstrap.UI.Elements.About
{
    public partial class MainWindow : INavigationWindow
    {
        public ElementTheme CurrentTheme { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            // Load theme preference (default to dark if not set)
            bool darkMode = App.Settings.Theme != "Light";
            ToggleTheme(darkMode);

            App.Logger.WriteLine("MainWindow", "Initializing about window");

            if (Locale.CurrentCulture.Name.StartsWith("tr"))
                TranslatorsText.FontSize = 9;
        }

        public void ToggleTheme(bool darkMode)
        {
            try
            {
                Theme.Apply(darkMode ? ThemeType.Dark : ThemeType.Light);
                CurrentTheme = darkMode ? ElementTheme.Dark : ElementTheme.Light;
                
                if (RootFrame?.Content is FrameworkElement content)
                {
                    content.RequestedTheme = CurrentTheme;
                }
                
                App.Settings.Theme = darkMode ? "Dark" : "Light";
                App.Settings.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteError("MainWindow", $"Theme toggle failed: {ex.Message}");
            }
        }

        #region INavigationWindow methods
        public Frame GetFrame() => RootFrame;
        public INavigation GetNavigation() => RootNavigation;
        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);
        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;
        public void ShowWindow() => Show();
        public void CloseWindow() => Close();
        #endregion
    }
}
