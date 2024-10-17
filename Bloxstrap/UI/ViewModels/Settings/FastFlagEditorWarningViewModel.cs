using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Contracts;

using Bloxstrap.UI.Elements.Settings.Pages;

namespace Bloxstrap.UI.ViewModels.Settings
{
    internal class FastFlagEditorWarningViewModel : NotifyPropertyChangedViewModel
    {
        private Page _page;

        public string ContinueButtonText { get; set; } = "";

        public bool CanContinue { get; set; } = false;

        public ICommand GoBackCommand => new RelayCommand(GoBack);

        public ICommand ContinueCommand => new RelayCommand(Continue);

        public FastFlagEditorWarningViewModel(Page page)
        {
            _page = page;
            DoCountdown();
        }

        private void DoCountdown()
        {
            ContinueButtonText = Strings.Menu_FastFlagEditor_Warning_Continue;
            OnPropertyChanged(nameof(ContinueButtonText));

            CanContinue = true;
            OnPropertyChanged(nameof(CanContinue));

            App.State.Prop.ShowFFlagEditorWarning = false;
            App.State.Save();
        }

        private void Continue()
        {
            if (!CanContinue)
                return;

            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(FastFlagEditorPage));
        }

        private void GoBack()
        {
            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(FastFlagsPage));
        }
    }
}