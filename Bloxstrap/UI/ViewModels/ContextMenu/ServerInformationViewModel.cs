using System.Windows;
using System.Windows.Input;
using Bloxstrap.Integrations;
using Bloxstrap.Models.Entities;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public string InstanceId => _activityWatcher.Data.JobId;

        public string PlaceId => _activityWatcher.Data.PlaceId == 0
            ? Strings.ContextMenu_ServerInformation_DetailsNotAvailable
            : _activityWatcher.Data.PlaceId.ToString();

        public string ExperienceName { get; private set; } = Strings.Common_Loading;

        public string CreatorName { get; private set; } = Strings.Common_Loading;

        public string ServerType => _activityWatcher.Data.ServerType.ToTranslatedString();

        public string ServerLocation { get; private set; } = Strings.Common_Loading;

        public Visibility ServerLocationVisibility => App.Settings.Prop.ShowServerDetails ? Visibility.Visible : Visibility.Collapsed;

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

        public ICommand CopySessionDetailsCommand => new RelayCommand(CopySessionDetails);

        public ServerInformationViewModel(Watcher watcher)
        {
            _activityWatcher = watcher.ActivityWatcher!;

            if (ServerLocationVisibility == Visibility.Visible)
                QueryServerLocation();

            QueryExperienceDetails();
        }

        private async void QueryExperienceDetails()
        {
            long universeId = _activityWatcher.Data.UniverseId;
            if (universeId == 0)
            {
                ExperienceName = Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
                CreatorName = Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
                OnPropertyChanged(nameof(ExperienceName));
                OnPropertyChanged(nameof(CreatorName));
                return;
            }

            try
            {
                UniverseDetails? details = _activityWatcher.Data.UniverseDetails ?? UniverseDetails.LoadFromCache(universeId);
                if (details is null)
                {
                    await UniverseDetails.FetchSingle(universeId);
                    details = UniverseDetails.LoadFromCache(universeId);
                }

                ExperienceName = details?.Data.Name ?? Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
                CreatorName = details?.Data.Creator.Name ?? Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("ServerInformationViewModel::QueryExperienceDetails", "Failed to get experience details");
                App.Logger.WriteException("ServerInformationViewModel::QueryExperienceDetails", ex);
                ExperienceName = Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
                CreatorName = Strings.ContextMenu_ServerInformation_DetailsNotAvailable;
            }

            OnPropertyChanged(nameof(ExperienceName));
            OnPropertyChanged(nameof(CreatorName));
        }

        public async void QueryServerLocation()
        {
            string? location = await _activityWatcher.Data.QueryServerLocation();

            if (String.IsNullOrEmpty(location))
                ServerLocation = Strings.Common_NotAvailable;
            else
                ServerLocation = location;

            OnPropertyChanged(nameof(ServerLocation));
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);

        private void CopySessionDetails()
        {
            var details = new List<string>
            {
                $"{Strings.ContextMenu_ServerInformation_Experience}: {ExperienceName}",
                $"{Strings.ContextMenu_ServerInformation_Creator}: {CreatorName}",
                $"{Strings.ContextMenu_ServerInformation_PlaceId}: {PlaceId}",
                $"{Strings.ContextMenu_ServerInformation_Type}: {ServerType}",
                $"{Strings.ContextMenu_ServerInformation_InstanceId}: {InstanceId}"
            };

            if (ServerLocationVisibility == Visibility.Visible && ServerLocation != Strings.Common_Loading)
                details.Add($"{Strings.ContextMenu_ServerInformation_Location}: {ServerLocation}");

            Clipboard.SetDataObject(String.Join(Environment.NewLine, details));
        }
    }
}
