using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        public double FooterOpacity => Environment.OSVersion.Version.Build >= 22000 ? 0.4 : 1;

        public FluentDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}
