using Bloxstrap.AppData;
using System;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        //TODO: FIX THE VERSION TEXT!
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;
        public SolidColorBrush BackgroundColourBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        public string VersionText { get; init; } = "None";
        public string ChannelText { get; init; } = "production";
        public FluentDialogViewModel(IBootstrapperDialog dialog, bool aero, string version, string channel) : base(dialog)
        {
            const int alpha = 128;

            WindowBackdropType = aero ? BackgroundType.Aero : BackgroundType.Mica;

            if (aero)
            {
                BackgroundColourBrush = App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Light ?
                    new SolidColorBrush(Color.FromArgb(alpha, 225, 225, 225)) :
                    new SolidColorBrush(Color.FromArgb(alpha, 30, 30, 30));
            }

            VersionText = $"{Strings.Common_Version}: {version}";
            ChannelText = $"{Strings.Common_Channel}: {channel}";
        }
    }
}
