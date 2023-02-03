using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Contracts;
using Bloxstrap.Views.Pages;
using Bloxstrap.Helpers;
using System.Diagnostics;

namespace Bloxstrap.ViewModels
{
    public class IntegrationsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        private readonly Page _page;

        public ICommand OpenReShadeFolderCommand => new RelayCommand(OpenReShadeFolder);
        public ICommand ShowReShadeHelpCommand => new RelayCommand(ShowReShadeHelp);

        public bool CanOpenReShadeFolder => !App.IsFirstRun;

        public IntegrationsViewModel(Page page)
        {
            _page = page;
        }

        private void OpenReShadeFolder()
        {
            Process.Start("explorer.exe", Directories.ReShade);
        }

        private void ShowReShadeHelp()
        {
            ((INavigationWindow)Window.GetWindow(_page)!).Navigate(typeof(ReShadeHelpPage));
        }

        public bool DiscordActivityEnabled
        {
            get => App.Settings.UseDiscordRichPresence;
            set
            {
                App.Settings.UseDiscordRichPresence = value;

                if (!value)
                {
                    DiscordActivityJoinEnabled = value;
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                }
            }
        }

        public bool DiscordActivityJoinEnabled
        {
            get => !App.Settings.HideRPCButtons;
            set => App.Settings.HideRPCButtons = !value;
        }

        public bool ReShadeEnabled
        {
            get => App.Settings.UseReShade;
            set
            {
                App.Settings.UseReShade = value;
                ReShadePresetsEnabled = value;
                OnPropertyChanged(nameof(ReShadePresetsEnabled));
            }
        }

        public bool ReShadePresetsEnabled
        {
            get => App.Settings.UseReShadeExtraviPresets;
            set => App.Settings.UseReShadeExtraviPresets = value;
        }

        public bool RbxFpsUnlockerEnabled
        {
            get => App.Settings.RFUEnabled;
            set
            {
                App.Settings.RFUEnabled = value;
                RbxFpsUnlockerAutocloseEnabled = value;
                OnPropertyChanged(nameof(RbxFpsUnlockerAutocloseEnabled));
            }
        }

        public bool RbxFpsUnlockerAutocloseEnabled
        {
            get => App.Settings.RFUAutoclose;
            set => App.Settings.RFUAutoclose = value;
        }
    }
}
