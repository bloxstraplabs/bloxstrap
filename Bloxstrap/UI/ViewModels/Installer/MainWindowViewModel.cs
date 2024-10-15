using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Installer
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        public string NextButtonText { get; private set; } = Strings.Common_Navigation_Next;

        public bool BackButtonEnabled { get; private set; } = false;

        public bool NextButtonEnabled { get; private set; } = false;

        public int ButtonWidth { get; } = Locale.CurrentCulture.Name.StartsWith("bg") ? 112 : 96;

        public ICommand BackPageCommand => new RelayCommand(BackPage);
        
        public ICommand NextPageCommand => new RelayCommand(NextPage);
        
        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

        public event EventHandler<string>? PageRequest;

        public event EventHandler? CloseWindowRequest;

        public void SetButtonEnabled(string type, bool state)
        {
            if (type == "next")
            {
                NextButtonEnabled = state;
                OnPropertyChanged(nameof(NextButtonEnabled));
            }
            else if (type == "back")
            {
                BackButtonEnabled = state;
                OnPropertyChanged(nameof(BackButtonEnabled));
            }
        }

        public void SetNextButtonText(string text)
        {
            NextButtonText = text;
            OnPropertyChanged(nameof(NextButtonText));
        }

        private void BackPage() => PageRequest?.Invoke(this, "back");

        private void NextPage() => PageRequest?.Invoke(this, "next");

        private void CloseWindow() => CloseWindowRequest?.Invoke(this, new EventArgs());
    }
}
