using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);

        public EventHandler? RequestSaveNoticeEvent;

        private void SaveSettings()
        {
            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();

            foreach (var task in App.PendingSettingTasks)
                task.Value.Execute();

            App.PendingSettingTasks.Clear();

            RequestSaveNoticeEvent?.Invoke(this, new EventArgs());
        }
    }
}
