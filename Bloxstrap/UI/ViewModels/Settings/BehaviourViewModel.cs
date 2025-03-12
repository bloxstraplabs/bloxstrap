using Bloxstrap.AppData;
using Bloxstrap.RobloxInterfaces;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {

        public BehaviourViewModel()
        {
            
        }

        public bool MultiInstances
        {
            get => App.Settings.Prop.MultiInstanceLaunching;
            set => App.Settings.Prop.MultiInstanceLaunching = value;
        }

        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool ForceRobloxLanguage
        {
            get => App.Settings.Prop.ForceRobloxLanguage;
            set => App.Settings.Prop.ForceRobloxLanguage = value;
        }
    }
}
