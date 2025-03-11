using Bloxstrap.UI.Elements.Base;
using Bloxstrap.UI.ViewModels.Dialogs;
using Microsoft.Win32;
using System.IO.Compression;
using System.Windows;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for AddCustomThemeDialog.xaml
    /// </summary>
    public partial class AddCustomThemeDialog : WpfUiWindow
    {
        private const int CreateNewTabId = 0;
        private const int ImportTabId = 1;

        private readonly AddCustomThemeViewModel _viewModel;

        public bool Created { get; private set; } = false;
        public string ThemeName { get; private set; } = "";
        public bool OpenEditor { get; private set; } = false;

        public AddCustomThemeDialog()
        {
            _viewModel = new AddCustomThemeViewModel();
            _viewModel.Name = GenerateRandomName();

            DataContext = _viewModel;

            InitializeComponent();
        }

        private static string GetThemePath(string name)
        {
            return Path.Combine(Paths.CustomThemes, name, "Theme.xml");
        }

        private static string GenerateRandomName()
        {
            int count = Directory.GetDirectories(Paths.CustomThemes).Count();

            string name = $"Custom Theme {count + 1}";

            // TODO: this sucks
            if (File.Exists(GetThemePath(name)))
                name += " " + Random.Shared.Next(1, 100000).ToString(); // easy

            return name;
        }

        private static string GetUniqueName(string name)
        {
            const int maxTries = 100;

            if (!File.Exists(GetThemePath(name)))
                return name;

            for (int i = 1; i <= maxTries; i++)
            {
                string newName = $"{name}_{i}";
                if (!File.Exists(GetThemePath(newName)))
                    return newName;
            }

            // last resort
            return $"{name}_{Random.Shared.Next(maxTries+1, 1_000_000)}";
        }

        private static void CreateCustomTheme(string name, CustomThemeTemplate template)
        {
            string dir = Path.Combine(Paths.CustomThemes, name);

            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);

            string themeFilePath = Path.Combine(dir, "Theme.xml");

            string templateContent = Encoding.UTF8.GetString(Resource.Get(template.GetFileName()).Result);

            File.WriteAllText(themeFilePath, templateContent);
        }

        private bool ValidateCreateNew()
        {
            const string LOG_IDENT = "AddCustomThemeDialog::ValidateCreateNew";

            if (string.IsNullOrEmpty(_viewModel.Name))
            {
                _viewModel.NameError = "Name cannot be empty";
                return false;
            }

            var validationResult = PathValidator.IsFileNameValid(_viewModel.Name);

            if (validationResult != PathValidator.ValidationResult.Ok)
            {
                switch (validationResult)
                {
                    case PathValidator.ValidationResult.IllegalCharacter:
                        _viewModel.NameError = "Name contains illegal characters";
                        break;
                    case PathValidator.ValidationResult.ReservedFileName:
                        _viewModel.NameError = "Name cannot be used";
                        break;
                    default:
                        App.Logger.WriteLine(LOG_IDENT, $"Got unhandled PathValidator::ValidationResult {validationResult}");
                        Debug.Assert(false);

                        _viewModel.NameError = "Unknown error";
                        break;
                }

                return false;
            }

            // better to check for the file instead of the directory so broken themes can be overwritten
            string path = Path.Combine(Paths.CustomThemes, _viewModel.Name, "Theme.xml");
            if (File.Exists(path))
            {
                _viewModel.NameError = "Name is already in use";
                return false;
            }    

            return true;
        }

        private bool ValidateImport()
        {
            const string LOG_IDENT = "AddCustomThemeDialog::ValidateImport";

            if (!_viewModel.FilePath.EndsWith(".zip"))
            {
                _viewModel.FileError = "File must be a ZIP";
                return false;
            }

            try
            {
                using var zipFile = ZipFile.OpenRead(_viewModel.FilePath);
                var entries = zipFile.Entries;

                bool foundThemeFile = false;

                foreach (var entry in entries)
                {
                    if (entry.FullName == "Theme.xml")
                    {
                        foundThemeFile = true;
                        break;
                    }
                }

                if (!foundThemeFile)
                {
                    _viewModel.FileError = "Theme file could not be found in the ZIP file";
                    return false;
                }

                return true;
            }
            catch (InvalidDataException ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Got invalid data");
                App.Logger.WriteException(LOG_IDENT, ex);

                _viewModel.FileError = "Invalid or corrupted ZIP file";
                return false;
            }
        }

        private void CreateNew()
        {
            if (!ValidateCreateNew())
                return;

            CreateCustomTheme(_viewModel.Name, _viewModel.Template);

            Created = true;
            ThemeName = _viewModel.Name;
            OpenEditor = true;

            Close();
        }

        private void Import()
        {
            if (!ValidateImport())
                return;

            string fileName = Path.GetFileNameWithoutExtension(_viewModel.FilePath);
            string name = GetUniqueName(fileName);

            string directory = Path.Combine(Paths.CustomThemes, name);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);

            var fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
            fastZip.ExtractZip(_viewModel.FilePath, directory, null);

            Created = true;
            ThemeName = name;
            OpenEditor = false;

            Close();
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedTab == CreateNewTabId)
                CreateNew();
            else
                Import();
        }

        private void OnImportButtonClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            _viewModel.FilePath = dialog.FileName;
        }
    }
}
