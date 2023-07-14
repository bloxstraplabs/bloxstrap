using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Enums;
using Bloxstrap.Extensions;
using Bloxstrap.UI.Menu;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class AppearanceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            using var dialog = new OpenFileDialog();
            dialog.Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CustomIconLocation = dialog.FileName;
                OnPropertyChanged(nameof(CustomIconLocation));
            }
        }

        public AppearanceViewModel(Page page)
        {
            _page = page;
        }

        public IReadOnlyDictionary<string, Theme> Themes { get; set; } = new Dictionary<string, Theme>()
        {
            { "System Default", Enums.Theme.Default },
            { "Light", Enums.Theme.Light },
            { "Dark", Enums.Theme.Dark },
        };

        public string Theme
        {
            get => Themes.FirstOrDefault(x => x.Value == App.Settings.Prop.Theme).Key;
            set
            {
                App.Settings.Prop.Theme = Themes[value];
                ((MainWindow)Window.GetWindow(_page)!).SetTheme();
            }
        }

        public IReadOnlyDictionary<string, BootstrapperStyle> Dialogs { get; set; } = new Dictionary<string, BootstrapperStyle>()
        {
            { "Fluent", BootstrapperStyle.FluentDialog },
            { "Progress (~2014)", BootstrapperStyle.ProgressDialog },
            { "Legacy (2011 - 2014)", BootstrapperStyle.LegacyDialog2011 },
            { "Legacy (2008 - 2011)", BootstrapperStyle.LegacyDialog2008 },
            { "Vista (2008 - 2011)", BootstrapperStyle.VistaDialog },
            { "Fake Byfron (2023)", BootstrapperStyle.ByfronDialog },
        };

        public string Dialog
        {
            get => Dialogs.FirstOrDefault(x => x.Value == App.Settings.Prop.BootstrapperStyle).Key;
            set => App.Settings.Prop.BootstrapperStyle = Dialogs[value];
        }

        public IReadOnlyDictionary<string, BootstrapperIcon> Icons { get; set; } = new Dictionary<string, BootstrapperIcon>()
        {
            { "Bloxstrap", BootstrapperIcon.IconBloxstrap },
            { "2022", BootstrapperIcon.Icon2022 },
            { "2019", BootstrapperIcon.Icon2019 },
            { "2017", BootstrapperIcon.Icon2017 },
            { "Late 2015", BootstrapperIcon.IconLate2015 },
            { "Early 2015", BootstrapperIcon.IconEarly2015 },
            { "2011", BootstrapperIcon.Icon2011 },
            { "2008", BootstrapperIcon.Icon2008 },
            { "Custom", BootstrapperIcon.IconCustom },
        };

        public string Icon
        {
            get => Icons.FirstOrDefault(x => x.Value == App.Settings.Prop.BootstrapperIcon).Key;
            set
            {
                App.Settings.Prop.BootstrapperIcon = Icons[value];
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
