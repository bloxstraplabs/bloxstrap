using System.Windows;

namespace Bloxstrap.UI.ViewModels.About
{
    public class AboutViewModel : NotifyPropertyChangedViewModel
    {
        private SupporterData? _supporterData;
        
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";

        public Visibility BuildInformationVisibility => App.IsProductionBuild ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => App.IsActionBuild ? Visibility.Visible : Visibility.Collapsed;

        public List<Supporter> Supporters => _supporterData?.Supporters ?? Enumerable.Empty<Supporter>().ToList();

        public int SupporterColumns => _supporterData?.Columns ?? 0;

        public GenericTriState SupportersLoadedState { get; set; } = GenericTriState.Unknown;

        public string SupportersLoadError { get; set; } = "";

        public AboutViewModel()
        {
            // this will cause momentary freezes only when ran under the debugger
            LoadSupporterData();
        }

        public async void LoadSupporterData()
        {
            const string LOG_IDENT = "AboutViewModel::LoadSupporterData";

            try
            {
                _supporterData = await Http.GetJson<SupporterData>("https://raw.githubusercontent.com/bloxstraplabs/config/main/supporters.json");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not load supporter data");
                App.Logger.WriteException(LOG_IDENT, ex);

                SupportersLoadedState = GenericTriState.Failed;
                SupportersLoadError = ex.Message;

                OnPropertyChanged(nameof(SupportersLoadError));
            }

            if (_supporterData is not null)
            {
                SupportersLoadedState = GenericTriState.Successful;
                
                OnPropertyChanged(nameof(Supporters));
                OnPropertyChanged(nameof(SupporterColumns));
            }

            OnPropertyChanged(nameof(SupportersLoadedState));
        }
    }
}
