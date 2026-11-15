using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Bloxstrap.Models;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public sealed class ExperienceLibraryViewModel : NotifyPropertyChangedViewModel
    {
        private string _experienceUrl = String.Empty;
        private string _experienceName = String.Empty;
        private string _status = String.Empty;
        private ExperienceEntry? _selectedExperience;

        public ObservableCollection<ExperienceEntry> Experiences { get; } = new();

        public string ExperienceUrl
        {
            get => _experienceUrl;
            set { if (_experienceUrl == value) return; _experienceUrl = value; OnPropertyChanged(nameof(ExperienceUrl)); }
        }

        public string ExperienceName
        {
            get => _experienceName;
            set { if (_experienceName == value) return; _experienceName = value; OnPropertyChanged(nameof(ExperienceName)); }
        }

        public string Status
        {
            get => _status;
            private set { if (_status == value) return; _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public ExperienceEntry? SelectedExperience
        {
            get => _selectedExperience;
            set
            {
                if (_selectedExperience == value) return;
                _selectedExperience = value;
                OnPropertyChanged(nameof(SelectedExperience));
                OnPropertyChanged(nameof(IsExperienceSelected));
                OnPropertyChanged(nameof(FavoriteActionText));
            }
        }

        public bool IsExperienceSelected => SelectedExperience is not null;
        public string FavoriteActionText => SelectedExperience?.IsFavorite == true
            ? LumiFeatureStrings.Menu_Experiences_UnmarkFavorite
            : LumiFeatureStrings.Menu_Experiences_MarkFavorite;
        public ICommand AddExperienceCommand => new RelayCommand(AddExperience);
        public ICommand OpenExperienceCommand => new RelayCommand(OpenExperience);
        public ICommand ToggleFavoriteCommand => new RelayCommand(ToggleFavorite);
        public ICommand RemoveExperienceCommand => new RelayCommand(RemoveExperience);

        public ExperienceLibraryViewModel() => Refresh();

        private void Refresh(long? selectPlaceId = null)
        {
            Experiences.Clear();
            foreach (ExperienceEntry entry in ExperienceLibraryManager.LoadAll())
                Experiences.Add(entry);

            SelectedExperience = selectPlaceId is null
                ? null
                : Experiences.FirstOrDefault(entry => entry.PlaceId == selectPlaceId.Value);
        }

        private void AddExperience()
        {
            if (!ExperienceLibraryManager.TryParsePlaceId(ExperienceUrl, out long placeId))
            {
                Status = LumiFeatureStrings.Menu_Experiences_InvalidUrl;
                return;
            }

            try
            {
                ExperienceLibraryManager.AddOrUpdate(placeId, ExperienceName);
                ExperienceUrl = String.Empty;
                ExperienceName = String.Empty;
                Status = LumiFeatureStrings.Menu_Experiences_Added;
                Refresh(placeId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ExperienceLibraryViewModel::AddExperience", ex);
                Status = String.Format(LumiFeatureStrings.Menu_Experiences_SaveFailed, ex.Message);
            }
        }

        private void OpenExperience()
        {
            if (SelectedExperience is null)
                return;

            try
            {
                Process.Start(new ProcessStartInfo($"roblox://experiences/start?placeId={SelectedExperience.PlaceId}")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ExperienceLibraryViewModel::OpenExperience", ex);
                Status = String.Format(LumiFeatureStrings.Menu_Experiences_OpenFailed, ex.Message);
            }
        }

        private void ToggleFavorite()
        {
            if (SelectedExperience is null)
                return;

            long placeId = SelectedExperience.PlaceId;
            try
            {
                ExperienceLibraryManager.ToggleFavorite(placeId);
                Refresh(placeId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ExperienceLibraryViewModel::ToggleFavorite", ex);
                Status = String.Format(LumiFeatureStrings.Menu_Experiences_SaveFailed, ex.Message);
            }
        }

        private void RemoveExperience()
        {
            if (SelectedExperience is null)
                return;

            long placeId = SelectedExperience.PlaceId;
            var result = Frontend.ShowMessageBox(
                String.Format(LumiFeatureStrings.Menu_Experiences_RemovePrompt, SelectedExperience.Name),
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                ExperienceLibraryManager.Remove(placeId);
                Status = String.Empty;
                Refresh();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ExperienceLibraryViewModel::RemoveExperience", ex);
                Status = String.Format(LumiFeatureStrings.Menu_Experiences_SaveFailed, ex.Message);
            }
        }
    }
}
