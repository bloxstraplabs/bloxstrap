using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bloxstrap.Dialogs;
using Bloxstrap.Enums;
using Bloxstrap.Helpers.Extensions;
using Bloxstrap.Views;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Mvvm.Contracts;

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
