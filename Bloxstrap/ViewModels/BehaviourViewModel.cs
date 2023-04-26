namespace Bloxstrap.ViewModels
{
    public class BehaviourViewModel
    {
        public bool CreateDesktopIcon
        {
            get => App.Settings.Prop.CreateDesktopIcon;
            set => App.Settings.Prop.CreateDesktopIcon = value;
        }

        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;
            set => App.Settings.Prop.CheckForUpdates = value;
        }

        public bool ChannelChangePromptingEnabled
        {
            get => App.Settings.Prop.PromptChannelChange;
            set => App.Settings.Prop.PromptChannelChange = value;
        }

        public bool MultiInstanceLaunchingEnabled
        {
            get => App.Settings.Prop.MultiInstanceLaunching;
            set => App.Settings.Prop.MultiInstanceLaunching = value;
        }
    }
}
