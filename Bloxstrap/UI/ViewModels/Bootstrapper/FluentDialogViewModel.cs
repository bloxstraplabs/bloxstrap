using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;

        public SolidColorBrush BackgroundColourBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        public string VersionText { get; init; }
        public string ChannelText { get; init; }
        public FluentDialogViewModel(IBootstrapperDialog dialog, bool aero) : base(dialog)
        {
            const int alpha = 128;

            WindowBackdropType = aero ? BackgroundType.Aero : BackgroundType.Mica;

            //string RealVersion = String.IsNullOrEmpty(Utilities.GetRobloxVersion(App.Bootstrapper?.IsStudioLaunch ?? false)) ? "None" : Utilities.GetRobloxVersion(App.Bootstrapper?.IsStudioLaunch ?? false);

            VersionText = String.Format(Strings.Common_Channel + ":", "0.0.0.0");
            ChannelText = String.Format(Strings.Common_Version + ":", "production");

            if (aero)
            {
                BackgroundColourBrush = App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Light ?
                    new SolidColorBrush(Color.FromArgb(alpha, 225, 225, 225)) :
                    new SolidColorBrush(Color.FromArgb(alpha, 30, 30, 30));
            }
        }
    }
}
