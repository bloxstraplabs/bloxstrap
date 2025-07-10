using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bloxstrap.UI.ViewModels.Dialogs
{
    internal class AddCustomThemeViewModel : NotifyPropertyChangedViewModel
    {
        public static CustomThemeTemplate[] Templates => Enum.GetValues<CustomThemeTemplate>();

        public CustomThemeTemplate Template { get; set; } = CustomThemeTemplate.Simple;

        public string Name { get; set; } = "";

        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                    OnPropertyChanged(nameof(FilePathVisibility));
                }
            }
        }
        public Visibility FilePathVisibility => string.IsNullOrEmpty(FilePath) ? Visibility.Collapsed : Visibility.Visible;

        public int SelectedTab { get; set; } = 0;

        private string _nameError = "";
        public string NameError
        {
            get => _nameError;
            set
            {
                if (_nameError != value)
                {
                    _nameError = value;
                    OnPropertyChanged(nameof(NameError));
                    OnPropertyChanged(nameof(NameErrorVisibility));
                }
            }
        }
        public Visibility NameErrorVisibility => string.IsNullOrEmpty(NameError) ? Visibility.Collapsed : Visibility.Visible;

        private string _fileError = "";
        public string FileError
        {
            get => _fileError;
            set
            {
                if (_fileError != value)
                {
                    _fileError = value;
                    OnPropertyChanged(nameof(FileError));
                    OnPropertyChanged(nameof(FileErrorVisibility));
                }
            }
        }
        public Visibility FileErrorVisibility => string.IsNullOrEmpty(FileError) ? Visibility.Collapsed : Visibility.Visible;
    }
}
