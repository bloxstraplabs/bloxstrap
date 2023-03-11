using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Helpers;
using Bloxstrap.Models;
using System.Collections.ObjectModel;

namespace Bloxstrap.ViewModels
{
    public class IntegrationsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        public ICommand OpenReShadeFolderCommand => new RelayCommand(OpenReShadeFolder);
        public ICommand AddIntegrationCommand => new RelayCommand(AddIntegration);
        public ICommand DeleteIntegrationCommand => new RelayCommand(DeleteIntegration);

        public bool CanOpenReShadeFolder => App.Settings.Prop.UseReShade;

        private void OpenReShadeFolder()
        {
            Process.Start("explorer.exe", Path.Combine(Directories.Integrations, "ReShade"));
        }

        private void AddIntegration()
        {
            CustomIntegrations.Add(new CustomIntegration()
            {
                Name = "New Integration"
            });

            SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
            
            OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        private void DeleteIntegration()
        {
            if (SelectedCustomIntegration is null)
                return;

            CustomIntegrations.Remove(SelectedCustomIntegration);

            if (CustomIntegrations.Count > 0)
            {
                SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
                OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            }

            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        public bool DiscordActivityEnabled
        {
            get => App.Settings.Prop.UseDiscordRichPresence;
            set
            {
                App.Settings.Prop.UseDiscordRichPresence = value;

                if (!value)
                {
                    DiscordActivityJoinEnabled = value;
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                }
            }
        }

        public bool DiscordActivityJoinEnabled
        {
            get => !App.Settings.Prop.HideRPCButtons;
            set => App.Settings.Prop.HideRPCButtons = !value;
        }

        public bool ReShadeEnabled
        {
            get => App.Settings.Prop.UseReShade;
            set
            {
                App.Settings.Prop.UseReShade = value;
                ReShadePresetsEnabled = value;

                if (value)
					App.FastFlags.SetRenderingMode("Direct3D 11");

				OnPropertyChanged(nameof(ReShadePresetsEnabled));
            }
        }

        public bool ReShadePresetsEnabled
        {
            get => App.Settings.Prop.UseReShadeExtraviPresets;
            set => App.Settings.Prop.UseReShadeExtraviPresets = value;
        }

        public bool RbxFpsUnlockerEnabled
        {
            get => App.Settings.Prop.RFUEnabled;
            set
            {
                App.Settings.Prop.RFUEnabled = value;
                RbxFpsUnlockerAutocloseEnabled = value;
                OnPropertyChanged(nameof(RbxFpsUnlockerAutocloseEnabled));
            }
        }

        public bool RbxFpsUnlockerAutocloseEnabled
        {
            get => App.Settings.Prop.RFUAutoclose;
            set => App.Settings.Prop.RFUAutoclose = value;
        }

        public ObservableCollection<CustomIntegration> CustomIntegrations
        {
            get => App.Settings.Prop.CustomIntegrations; 
            set => App.Settings.Prop.CustomIntegrations = value;
        }

        public CustomIntegration? SelectedCustomIntegration { get; set; }
        public int SelectedCustomIntegrationIndex { get; set; }
        public bool IsCustomIntegrationSelected => SelectedCustomIntegration is not null;
    }
}
