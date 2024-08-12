﻿namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        private string _oldPlayerVersionGuid = "";
        private string _oldStudioVersionGuid = "";

        public IEnumerable<PriorityClasses> PriorityClassesList { get; } = Enum.GetValues(typeof(PriorityClasses)).Cast<PriorityClasses>();

        public PriorityClasses ChoosePriorityClass
        {
            get => App.Settings.Prop.ChoosePriorityClass;
            set => App.Settings.Prop.ChoosePriorityClass = value;
        }

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

        public bool ForceRobloxReinstallation
        {
            // wouldnt it be better to check old version guids?
            // what about fresh installs?
            get => String.IsNullOrEmpty(App.State.Prop.PlayerVersionGuid) && String.IsNullOrEmpty(App.State.Prop.StudioVersionGuid);
            set
            {
                if (value)
                {
                    _oldPlayerVersionGuid = App.State.Prop.PlayerVersionGuid;
                    _oldStudioVersionGuid = App.State.Prop.StudioVersionGuid;
                    App.State.Prop.PlayerVersionGuid = "";
                    App.State.Prop.StudioVersionGuid = "";
                }
                else
                {
                    App.State.Prop.PlayerVersionGuid = _oldPlayerVersionGuid;
                    App.State.Prop.StudioVersionGuid = _oldStudioVersionGuid;
                }
            }
        }
    }
}
