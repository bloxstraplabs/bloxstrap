using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Models.SettingTasks;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class FileModViewModel : NotifyPropertyChangedViewModel
    {
        private readonly string _filter;

        private readonly byte[] _header;

        public FileModTask Task { get; }

        public ICommand ManageCommand => new RelayCommand(Manage);

        public Visibility ChooseVisibility => String.IsNullOrEmpty(Task.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RemoveVisibility => String.IsNullOrEmpty(Task.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public FileModViewModel(FileModTask task, string filter, byte[] header)
        {
            Task = task;
            _filter = filter;
            _header = header;
        }

        private void Manage()
        {
            if (!String.IsNullOrEmpty(Task.NewState))
            {
                Task.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog { Filter = _filter };

                if (dialog.ShowDialog() != true)
                    return;

                using (var stream = File.OpenRead(dialog.FileName))
                {
                    byte[] header = new byte[_header.Length];

                    if (stream.Read(header, 0, header.Length) != header.Length || !header.SequenceEqual(_header))
                    {
                        Frontend.ShowMessageBox(Strings.Menu_Mods_CustomFiles_Invalid, MessageBoxImage.Error);
                        return;
                    }
                }

                Task.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseVisibility));
            OnPropertyChanged(nameof(RemoveVisibility));
        }
    }
}
