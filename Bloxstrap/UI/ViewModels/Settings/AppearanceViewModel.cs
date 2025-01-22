using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Bloxstrap.UI.Elements.Settings;
using Bloxstrap.UI.Elements.Editor;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class AppearanceViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public ICommand PreviewBootstrapperCommand => new RelayCommand(PreviewBootstrapper);
        public ICommand BrowseCustomIconLocationCommand => new RelayCommand(BrowseCustomIconLocation);

        public ICommand AddCustomThemeCommand => new RelayCommand(AddCustomTheme);
        public ICommand DeleteCustomThemeCommand => new RelayCommand(DeleteCustomTheme);
        public ICommand RenameCustomThemeCommand => new RelayCommand(RenameCustomTheme);
        public ICommand EditCustomThemeCommand => new RelayCommand(EditCustomTheme);

        private void PreviewBootstrapper()
        {
            IBootstrapperDialog dialog = App.Settings.Prop.BootstrapperStyle.GetNew();

            if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ByfronDialog)
                dialog.Message = Strings.Bootstrapper_StylePreview_ImageCancel;
            else
                dialog.Message = Strings.Bootstrapper_StylePreview_TextCancel;

            dialog.CancelEnabled = true;
            dialog.ShowBootstrapper();
        }

        private void BrowseCustomIconLocation()
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.Menu_IconFiles}|*.ico"
            };

            if (dialog.ShowDialog() != true)
                return;

            CustomIconLocation = dialog.FileName;
            OnPropertyChanged(nameof(CustomIconLocation));
        }

        public AppearanceViewModel(Page page)
        {
            _page = page;

            foreach (var entry in BootstrapperIconEx.Selections)
                Icons.Add(new BootstrapperIconEntry { IconType = entry });

            PopulateCustomThemes();
        }

        public IEnumerable<Theme> Themes { get; } = Enum.GetValues(typeof(Theme)).Cast<Theme>();

        public Theme Theme
        {
            get => App.Settings.Prop.Theme;
            set
            {
                App.Settings.Prop.Theme = value;
                ((MainWindow)Window.GetWindow(_page)!).ApplyTheme();
            }
        }

        public static List<string> Languages => Locale.GetLanguages();

        public string SelectedLanguage 
        { 
            get => Locale.SupportedLocales[App.Settings.Prop.Locale]; 
            set => App.Settings.Prop.Locale = Locale.GetIdentifierFromName(value);
        }

        public IEnumerable<BootstrapperStyle> Dialogs { get; } = BootstrapperStyleEx.Selections;

        public BootstrapperStyle Dialog
        {
            get => App.Settings.Prop.BootstrapperStyle;
            set
            {
                App.Settings.Prop.BootstrapperStyle = value;
                OnPropertyChanged(nameof(CustomThemesExpanded)); // TODO: only fire when needed
            }
        }

        public bool CustomThemesExpanded => App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.CustomDialog;

        public ObservableCollection<BootstrapperIconEntry> Icons { get; set; } = new();

        public BootstrapperIcon Icon
        {
            get => App.Settings.Prop.BootstrapperIcon;
            set => App.Settings.Prop.BootstrapperIcon = value; 
        }

        public string Title
        {
            get => App.Settings.Prop.BootstrapperTitle;
            set => App.Settings.Prop.BootstrapperTitle = value;
        }

        public string CustomIconLocation
        {
            get => App.Settings.Prop.BootstrapperIconCustomLocation;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (App.Settings.Prop.BootstrapperIcon == BootstrapperIcon.IconCustom)
                        App.Settings.Prop.BootstrapperIcon = BootstrapperIcon.IconBloxstrap;
                }
                else
                {
                    App.Settings.Prop.BootstrapperIcon = BootstrapperIcon.IconCustom;
                }

                App.Settings.Prop.BootstrapperIconCustomLocation = value;

                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(Icons));
            }
        }

        private string CreateCustomThemeName()
        {
            int count = Directory.GetDirectories(Paths.CustomThemes).Count();

            string name = $"Custom Theme {count + 1}";

            // TODO: this sucks
            if (Directory.Exists(Path.Combine(Paths.CustomThemes, name))) // DUCK
                name += " " + Random.Shared.Next(1, 100000).ToString(); // easy

            return name;
        }

        private void CreateCustomThemeStructure(string name)
        {
            string dir = Path.Combine(Paths.CustomThemes, name);
            Directory.CreateDirectory(dir);

            string themeFilePath = Path.Combine(dir, "Theme.xml");

            string templateContent = Encoding.UTF8.GetString(Resource.Get("CustomBootstrapperTemplate.xml").Result);

            File.WriteAllText(themeFilePath, templateContent);
        }

        private void DeleteCustomThemeStructure(string name)
        {
            string dir = Path.Combine(Paths.CustomThemes, name);
            Directory.Delete(dir, true);
        }

        private void RenameCustomThemeStructure(string oldName, string newName)
        {
            string oldDir = Path.Combine(Paths.CustomThemes, oldName);
            string newDir = Path.Combine(Paths.CustomThemes, newName);
            Directory.Move(oldDir, newDir);
        }

        private void AddCustomTheme()
        {
            string name = CreateCustomThemeName();

            try
            {
                CreateCustomThemeStructure(name);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AppearanceViewModel::AddCustomTheme", ex);
                Frontend.ShowMessageBox($"Failed to create custom theme: {ex.Message}", MessageBoxImage.Error);
                return;
            }

            CustomThemes.Add(name);
            SelectedCustomThemeIndex = CustomThemes.Count - 1;

            OnPropertyChanged(nameof(SelectedCustomThemeIndex));
            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        private void DeleteCustomTheme()
        {
            if (SelectedCustomTheme is null)
                return;

            try
            {
                DeleteCustomThemeStructure(SelectedCustomTheme);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AppearanceViewModel::DeleteCustomTheme", ex);
                Frontend.ShowMessageBox($"Failed to delete custom theme {SelectedCustomTheme}: {ex.Message}", MessageBoxImage.Error);
                return;
            }

            CustomThemes.Remove(SelectedCustomTheme);

            if (CustomThemes.Any())
            {
                SelectedCustomThemeIndex = CustomThemes.Count - 1;
                OnPropertyChanged(nameof(SelectedCustomThemeIndex));
            }

            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        private void RenameCustomTheme()
        {
            if (SelectedCustomTheme is null)
                return;

            if (SelectedCustomTheme == SelectedCustomThemeName)
                return;

            try
            {
                RenameCustomThemeStructure(SelectedCustomTheme, SelectedCustomThemeName);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AppearanceViewModel::RenameCustomTheme", ex);
                Frontend.ShowMessageBox($"Failed to rename custom theme {SelectedCustomTheme}: {ex.Message}", MessageBoxImage.Error);
                return;
            }

            int idx = CustomThemes.IndexOf(SelectedCustomTheme);
            CustomThemes[idx] = SelectedCustomThemeName;

            SelectedCustomThemeIndex = idx;
            OnPropertyChanged(nameof(SelectedCustomThemeIndex));
        }

        private void EditCustomTheme()
        {
            if (SelectedCustomTheme is null)
                return;

            new BootstrapperEditorWindow(SelectedCustomTheme).ShowDialog();
        }

        private void PopulateCustomThemes()
        {
            string? selected = App.Settings.Prop.SelectedCustomTheme;

            Directory.CreateDirectory(Paths.CustomThemes);

            foreach (string directory in Directory.GetDirectories(Paths.CustomThemes))
            {
                if (!File.Exists(Path.Combine(directory, "Theme.xml")))
                    continue; // missing the main theme file, ignore

                string name = Path.GetFileName(directory);
                CustomThemes.Add(name);
            }

            if (selected != null)
            {
                int idx = CustomThemes.IndexOf(selected);

                if (idx != -1)
                {
                    SelectedCustomThemeIndex = idx;
                    OnPropertyChanged(nameof(SelectedCustomThemeIndex));
                }
                else
                {
                    SelectedCustomTheme = null;
                }
            }
        }

        public string? SelectedCustomTheme
        {
            get => App.Settings.Prop.SelectedCustomTheme;
            set => App.Settings.Prop.SelectedCustomTheme = value;
        }

        public string SelectedCustomThemeName { get; set; } = "";

        public int SelectedCustomThemeIndex { get; set; }

        public ObservableCollection<string> CustomThemes { get; set; } = new();
        public bool IsCustomThemeSelected => SelectedCustomTheme is not null;
    }
}
