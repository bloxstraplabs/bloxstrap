using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                dialog.Message = "Style preview - Click the X button at the top right to close";
            else
                dialog.Message = "Style preview - Click Cancel to close";

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
        }

        public IReadOnlyCollection<Theme> Themes { get; set; } = new Theme[]
        {
            Theme.Default,
            Theme.Light,
            Theme.Dark
        };

        public Theme Theme
        {
            get => App.Settings.Prop.Theme;
            set
            {
                App.Settings.Prop.Theme = value;
                ((MainWindow)Window.GetWindow(_page)!).ApplyTheme();
            }
        }

        public IReadOnlyCollection<BootstrapperStyle> Dialogs { get; set; } = new BootstrapperStyle[]
        {
            BootstrapperStyle.FluentDialog,
            BootstrapperStyle.ProgressDialog,
            BootstrapperStyle.LegacyDialog2011,
            BootstrapperStyle.LegacyDialog2008,
            BootstrapperStyle.VistaDialog,
            BootstrapperStyle.ByfronDialog
        };

        public BootstrapperStyle Dialog
        {
            get => App.Settings.Prop.BootstrapperStyle;
            set => App.Settings.Prop.BootstrapperStyle = value;
        }

        public IReadOnlyCollection<BootstrapperIcon> Icons { get; set; } = new BootstrapperIcon[]
        {
            BootstrapperIcon.IconBloxstrap,
            BootstrapperIcon.Icon2022,
            BootstrapperIcon.Icon2019,
            BootstrapperIcon.Icon2017,
            BootstrapperIcon.IconLate2015,
            BootstrapperIcon.IconEarly2015,
            BootstrapperIcon.Icon2011,
            BootstrapperIcon.Icon2008,
            BootstrapperIcon.IconCustom
        };

        public BootstrapperIcon Icon
        {
            get => App.Settings.Prop.BootstrapperIcon;
            set
            {
                App.Settings.Prop.BootstrapperIcon = value;
                OnPropertyChanged(nameof(IconPreviewSource));
            }
        }

        public ImageSource IconPreviewSource => App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();

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
                App.Settings.Prop.BootstrapperIcon = BootstrapperIcon.IconCustom;
                App.Settings.Prop.BootstrapperIconCustomLocation = value;

                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(IconPreviewSource));
            }
        }
    }
}
