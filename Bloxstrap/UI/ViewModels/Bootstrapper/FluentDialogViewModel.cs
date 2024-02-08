using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        public double FooterOpacity => Environment.OSVersion.Version.Build >= 22000 ? 0.4 : 1;

        [Obsolete("Do not use this! This is for the designer only.", true)]
        public FluentDialogViewModel() : this(null!)
        { }

        public FluentDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}
