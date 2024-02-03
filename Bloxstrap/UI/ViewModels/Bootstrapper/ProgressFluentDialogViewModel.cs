using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class ProgressFluentDialogViewModel : BootstrapperDialogViewModel
    {
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;

        [Obsolete("Do not use this! This is for the designer only.", true)]
        public ProgressFluentDialogViewModel() : base()
        { }

        public ProgressFluentDialogViewModel(IBootstrapperDialog dialog, bool aero) : base(dialog)
        {
            WindowBackdropType = aero ? BackgroundType.Aero : BackgroundType.Mica;
        }
    }
}
