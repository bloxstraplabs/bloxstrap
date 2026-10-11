using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class GameProfilesViewModel : NotifyPropertyChangedViewModel
    {
        public record Option(string Value, string Label);

        public ObservableCollection<GameProfile> Profiles => App.Settings.Prop.GameProfiles;

        public Visibility EmptyVisibility => Profiles.Any() ? Visibility.Collapsed : Visibility.Visible;

        public IReadOnlyList<Option> FastFlagProfileOptions { get; }

        public IReadOnlyList<Option> QualityOptions { get; }

        public IEnumerable<ProfileToggle> CrosshairOptions { get; } = Enum.GetValues<ProfileToggle>();

        public string NewGameInput { get; set; } = "";

        public ICommand AddProfileCommand => new AsyncRelayCommand(AddProfile);

        public ICommand DeleteProfileCommand => new RelayCommand<GameProfile>(DeleteProfile);

        public GameProfilesViewModel()
        {
            var flagProfiles = Directory.Exists(Paths.FastFlagProfiles)
                ? Directory.GetFiles(Paths.FastFlagProfiles, "*.json").Select(x => Path.GetFileNameWithoutExtension(x)!).OrderBy(x => x)
                : Enumerable.Empty<string>();

            FastFlagProfileOptions = flagProfiles.Select(x => new Option(x, x)).Prepend(new Option("", Strings.Menu_GameProfiles_DontChange)).ToList();

            QualityOptions = Enumerable.Range(1, 10).Select(x => new Option(x.ToString(), x.ToString()))
                .Prepend(new Option("0", Strings.Common_Automatic))
                .Prepend(new Option("", Strings.Menu_GameProfiles_DontChange))
                .ToList();
        }

        private async Task AddProfile()
        {
            const string LOG_IDENT = "GameProfilesViewModel::AddProfile";

            long? placeId = GameShortcuts.ParsePlaceId(NewGameInput);

            if (placeId is null)
            {
                Frontend.ShowMessageBox(Strings.Menu_Shortcuts_Game_Invalid, MessageBoxImage.Error);
                return;
            }

            try
            {
                var profile = await GameProfiles.Create(placeId.Value);

                if (Profiles.Any(x => x.UniverseId == profile.UniverseId))
                {
                    Frontend.ShowMessageBox(String.Format(Strings.Menu_GameProfiles_AlreadyExists, profile.Name), MessageBoxImage.Information);
                    return;
                }

                Profiles.Add(profile);

                NewGameInput = "";
                OnPropertyChanged(nameof(NewGameInput));
                OnPropertyChanged(nameof(EmptyVisibility));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(String.Format(Strings.Menu_Shortcuts_Game_Failed, ex.Message), MessageBoxImage.Error);
            }
        }

        private void DeleteProfile(GameProfile? profile)
        {
            if (profile is null)
                return;

            Profiles.Remove(profile);
            OnPropertyChanged(nameof(EmptyVisibility));
        }
    }
}
