using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bloxstrap.Enums;
using Bloxstrap.Views;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.ViewModels
{
    public class BootstrapperViewModel
    {
        private readonly Page _page;

        public ICommand PreviewBootstrapperCommand => new RelayCommand(PreviewBootstrapper);

        private void PreviewBootstrapper() => App.Settings.Prop.BootstrapperStyle.Show();

        public BootstrapperViewModel(Page page)
        {
            _page = page;
        }

        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;
            set => App.Settings.Prop.CheckForUpdates = value;
        }

        public bool ChannelChangePromptingEnabled
        {
            get => App.Settings.Prop.PromptChannelChange;
            set => App.Settings.Prop.PromptChannelChange = value;
        }

        public bool MultiInstanceLaunchingEnabled
        {
            get => App.Settings.Prop.MultiInstanceLaunching;
            set => App.Settings.Prop.MultiInstanceLaunching = value;
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
            { "Vista (2009 - 2011)", BootstrapperStyle.VistaDialog },
            { "Legacy (2009 - 2011)", BootstrapperStyle.LegacyDialog2009 },
            { "Legacy (2011 - 2014)", BootstrapperStyle.LegacyDialog2011 },
            { "Progress (~2014)", BootstrapperStyle.ProgressDialog },
        };

        public string Dialog
        {
            get => Dialogs.FirstOrDefault(x => x.Value == App.Settings.Prop.BootstrapperStyle).Key;
            set => App.Settings.Prop.BootstrapperStyle = Dialogs[value];
        }

        public IReadOnlyDictionary<string, BootstrapperIcon> Icons { get; set; } = new Dictionary<string, BootstrapperIcon>()
        {
            { "Bloxstrap", BootstrapperIcon.IconBloxstrap },
            { "2009", BootstrapperIcon.Icon2009 },
            { "2011", BootstrapperIcon.Icon2011 },
            { "2015", BootstrapperIcon.IconEarly2015 },
            { "2016", BootstrapperIcon.IconLate2015 },
            { "2017", BootstrapperIcon.Icon2017 },
            { "2019", BootstrapperIcon.Icon2019 },
            { "2022", BootstrapperIcon.Icon2022 }
        };

        public string Icon
        {
            get => Icons.FirstOrDefault(x => x.Value == App.Settings.Prop.BootstrapperIcon).Key;
            set => App.Settings.Prop.BootstrapperIcon = Icons[value];
        }
    }
}
