using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using Bloxstrap.UI.Elements.About;
using System.Windows;

namespace Bloxstrap.UI.ViewModels.Installer
{
    internal class LaunchMenuViewModel : NotifyPropertyChangedViewModel
    {
        public int MaxNews => 2;

        public Visibility NewsPanelVisible { get; set; }

        public Visibility NewsVisible { get; set; } = Visibility.Hidden;

        public List<NewsItem> _newsfeed { get; set; } = new();

        public event EventHandler<NextAction>? CloseWindowRequest;

        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public List<NewsItem> Newsfeed
        {
            get => _newsfeed;
            set => _newsfeed = value.Take(MaxNews).ToList();
        }

        public LaunchMenuViewModel(bool newsfeedVisible)
        {
            NewsPanelVisible = newsfeedVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void RaiseNewsLoaded()
        {
            OnPropertyChanged(nameof(Newsfeed));
            ShowNews();
        }

        private async void ShowNews()
        {
            // this is pretty gross
            // essentially when WPF loads an image it likes to render
            // it supermassively for a split second so we have this brief
            // delay + fade in to try and mitigate that from visibly showing
            await Task.Delay(250);
            NewsVisible = Visibility.Visible;
            OnPropertyChanged(nameof(NewsVisible));
        }

        public ICommand LaunchSettingsCommand => new RelayCommand(() => CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings));

        public ICommand LaunchRobloxCommand => new RelayCommand(() => CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox));

        public ICommand LaunchRobloxStudioCommand => new RelayCommand(() => CloseWindowRequest?.Invoke(this, NextAction.LaunchRobloxStudio));

        public ICommand LaunchAboutCommand => new RelayCommand(() => new MainWindow().ShowDialog());

        public ICommand LaunchGroupCommand => new RelayCommand(() => Utilities.ShellExecute(StaticContent.RobloxGroupURL));

        public ICommand LaunchHelpCommand => new RelayCommand(() => Utilities.ShellExecute(StaticContent.HelpURL));

        public ICommand LaunchNewsCommand => new RelayCommand(() => Utilities.ShellExecute(StaticContent.NewsURL));
    }
}
