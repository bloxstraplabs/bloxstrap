using System.Windows;

namespace Bloxstrap.UI.ViewModels.About
{
    public class SupportersViewModel : NotifyPropertyChangedViewModel
    {
        public SupporterData? SupporterData { get; private set; }
        
        public GenericTriState LoadedState { get; set; } = GenericTriState.Unknown;

        public string LoadError { get; set; } = "";

        public int Columns { get; set; } = 3;

        public SizeChangedEventHandler? WindowResizeEvent;

        public SupportersViewModel()
        {
            WindowResizeEvent += OnWindowResize;

            // this will cause momentary freezes only when ran under the debugger
            LoadSupporterData();
        }

        private void OnWindowResize(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
                return;

            int newCols = (int)Math.Floor(e.NewSize.Width / 200);

            if (Columns == newCols)
                return;
             
            Columns = newCols;
            OnPropertyChanged(nameof(Columns));
        }

        public async void LoadSupporterData()
        {
            const string LOG_IDENT = "AboutViewModel::LoadSupporterData";

            try
            {
                SupporterData = await Http.GetJson<SupporterData>("https://raw.githubusercontent.com/bloxstraplabs/config/main/supporters.json");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not load supporter data");
                App.Logger.WriteException(LOG_IDENT, ex);

                LoadedState = GenericTriState.Failed;
                LoadError = ex.Message;

                OnPropertyChanged(nameof(LoadError));
            }

            if (SupporterData is not null)
            {
                LoadedState = GenericTriState.Successful;
                
                OnPropertyChanged(nameof(SupporterData));
            }

            OnPropertyChanged(nameof(LoadedState));
        }
    }
}
