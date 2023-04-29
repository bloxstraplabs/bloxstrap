using System.Collections.Generic;
using System.Linq;

using Bloxstrap.Enums;

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

        public bool MultiInstanceLaunchingEnabled
        {
            get => App.Settings.Prop.MultiInstanceLaunching;
            set => App.Settings.Prop.MultiInstanceLaunching = value;
        }

        // todo - move to enum attributes?
        public IReadOnlyDictionary<string, ChannelChangeMode> ChannelChangeModes => new Dictionary<string, ChannelChangeMode>
        {
            { "Change automatically", ChannelChangeMode.Automatic },
            { "Always prompt", ChannelChangeMode.Prompt },
            { "Never change", ChannelChangeMode.Ignore },
        };

        public string SelectedChannelChangeMode
        {
            get => ChannelChangeModes.FirstOrDefault(x => x.Value == App.Settings.Prop.ChannelChangeMode).Key;
            set => App.Settings.Prop.ChannelChangeMode = ChannelChangeModes[value];
        }
    }
}
