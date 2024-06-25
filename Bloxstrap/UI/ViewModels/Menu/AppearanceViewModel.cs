using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Bloxstrap.UI.Elements.Menu;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class AppearanceViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public ICommand PreviewBootstrapperCommand => new RelayCommand(PreviewBootstrapper);
        public ICommand BrowseCustomIconLocationCommand => new RelayCommand(BrowseCustomIconLocation);

        private void PreviewBootstrapper()
        {
            IBootstrapperDialog dialog = App.Settings.Prop.BootstrapperStyle.GetNew();

            if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ByfronDialog)
                dialog.Message = Resources.Strings.Bootstrapper_StylePreview_ImageCancel;
            else
                dialog.Message = Resources.Strings.Bootstrapper_StylePreview_TextCancel;

            dialog.CancelEnabled = true;
            dialog.ShowBootstrapper();
        }

        private void BrowseCustomIconLocation()
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"{Resources.Strings.Menu_IconFiles}|*.ico|{Resources.Strings.Menu_AllFiles}|*.*"
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
            set => App.Settings.Prop.BootstrapperStyle = value;
        }

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
    }
}
