using System.Windows.Forms;

using Bloxstrap.Dialogs;

namespace Bloxstrap.Enums
{
    public enum BootstrapperStyle
    {
        VistaDialog,
        LegacyDialog2009,
        LegacyDialog2011,
        ProgressDialog,
        FluentDialog
    }

    public static class BootstrapperStyleEx
    {
        public static IBootstrapperDialog GetNew(this BootstrapperStyle bootstrapperStyle)
        {
            return bootstrapperStyle switch
            {
                BootstrapperStyle.VistaDialog => new VistaDialog(),
                BootstrapperStyle.LegacyDialog2009 => new LegacyDialog2009(),
                BootstrapperStyle.LegacyDialog2011 => new LegacyDialog2011(),
                BootstrapperStyle.ProgressDialog => new ProgressDialog(),
                BootstrapperStyle.FluentDialog => new FluentDialog(),
                _ => new FluentDialog()
            };
        }
    }
}
