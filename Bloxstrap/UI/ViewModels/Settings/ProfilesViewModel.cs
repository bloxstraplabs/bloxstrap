using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Models;
using Bloxstrap.UI.Elements.Settings;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class ProfilesViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;
        private string _profileName = String.Empty;
        private ConfigurationProfile? _selectedProfile;

        public ObservableCollection<ConfigurationProfile> Profiles { get; } = new();

        public string ProfileName
        {
            get => _profileName;
            set
            {
                if (_profileName == value)
                    return;

                _profileName = value;
                OnPropertyChanged(nameof(ProfileName));
            }
        }

        public ConfigurationProfile? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile == value)
                    return;

                _selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
                OnPropertyChanged(nameof(IsProfileSelected));
            }
        }

        public bool IsProfileSelected => SelectedProfile is not null;

        public ICommand SaveProfileCommand => new RelayCommand(SaveProfile);
        public ICommand ImportProfileCommand => new RelayCommand(ImportProfile);
        public ICommand ExportProfileCommand => new RelayCommand(ExportProfile);
        public ICommand ApplyProfileCommand => new RelayCommand(ApplyProfile);
        public ICommand DeleteProfileCommand => new RelayCommand(DeleteProfile);

        public ProfilesViewModel(Page page)
        {
            _page = page;
            RefreshProfiles();
        }

        private void RefreshProfiles()
        {
            Profiles.Clear();

            foreach (var profile in ConfigurationProfileManager.LoadAll())
                Profiles.Add(profile);
        }

        private void SaveProfile()
        {
            if (String.IsNullOrWhiteSpace(ProfileName))
            {
                Frontend.ShowMessageBox(Strings.Menu_Profiles_NameRequired, MessageBoxImage.Information);
                return;
            }

            if (Profiles.Any(x => String.Equals(x.Name, ProfileName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                Frontend.ShowMessageBox(Strings.Menu_Profiles_DuplicateName, MessageBoxImage.Information);
                return;
            }

            try
            {
                var profile = ConfigurationProfileManager.SaveCurrent(ProfileName);
                Profiles.Insert(0, profile);
                SelectedProfile = profile;
                ProfileName = String.Empty;
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_Saved, profile.Name), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ProfilesViewModel::SaveProfile", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_SaveFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void ApplyProfile()
        {
            if (SelectedProfile is null)
                return;

            try
            {
                string name = SelectedProfile.Name;
                var preview = ConfigurationProfileManager.GetApplyPreview(SelectedProfile);
                string message = String.Format(
                    Strings.Menu_Profiles_ApplyPreview,
                    name,
                    preview.SettingsChanged,
                    preview.FlagsAdded,
                    preview.FlagsUpdated,
                    preview.FlagsRemoved
                );

                var result = Frontend.ShowMessageBox(message, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;

                ConfigurationProfileManager.Apply(SelectedProfile);

                if (Window.GetWindow(_page) is MainWindow mainWindow)
                    mainWindow.ApplyTheme();

                Frontend.ShowMessageBox(
                    String.Format(Strings.Menu_Profiles_Applied, name),
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ProfilesViewModel::ApplyProfile", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_ApplyFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void ImportProfile()
        {
            var dialog = new OpenFileDialog
            {
                Title = Strings.Menu_Profiles_Import,
                Filter = Strings.Menu_Profiles_ImportFilter,
                DefaultExt = ".lumiprofile",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog(Window.GetWindow(_page)) != true)
                return;

            try
            {
                ConfigurationProfile profile = ConfigurationProfileManager.ReadPortable(dialog.FileName);
                var result = Frontend.ShowMessageBox(
                    String.Format(Strings.Menu_Profiles_ImportPreview, profile.Name, profile.FastFlags.Count),
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );

                if (result != MessageBoxResult.Yes)
                    return;

                profile = ConfigurationProfileManager.ImportPortable(profile);
                Profiles.Insert(0, profile);
                SelectedProfile = profile;

                Frontend.ShowMessageBox(
                    String.Format(Strings.Menu_Profiles_Imported, profile.Name),
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ProfilesViewModel::ImportProfile", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_ImportFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void ExportProfile()
        {
            if (SelectedProfile is null)
                return;

            var profile = SelectedProfile;
            var dialog = new SaveFileDialog
            {
                Title = Strings.Menu_Profiles_Export,
                Filter = Strings.Menu_Profiles_ExportFilter,
                DefaultExt = ".lumiprofile",
                AddExtension = true,
                OverwritePrompt = true,
                FileName = $"{GetSafeFileName(profile.Name)}.lumiprofile"
            };

            if (dialog.ShowDialog(Window.GetWindow(_page)) != true)
                return;

            try
            {
                ConfigurationProfileManager.ExportPortable(profile, dialog.FileName);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_Exported, profile.Name), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ProfilesViewModel::ExportProfile", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_ExportFailed, ex.Message), MessageBoxImage.Error);
            }
        }

        private static string GetSafeFileName(string name)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            string safeName = new(name.Select(character => invalidCharacters.Contains(character) ? '_' : character).ToArray());
            return String.IsNullOrWhiteSpace(safeName) ? "LumiStrap-profile" : safeName;
        }

        private void DeleteProfile()
        {
            if (SelectedProfile is null)
                return;

            var profile = SelectedProfile;
            var result = Frontend.ShowMessageBox(
                String.Format(Strings.Menu_Profiles_DeletePrompt, profile.Name),
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                ConfigurationProfileManager.Delete(profile);
                Profiles.Remove(profile);
                SelectedProfile = null;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ProfilesViewModel::DeleteProfile", ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Profiles_DeleteFailed, ex.Message), MessageBoxImage.Error);
            }
        }
    }
}
