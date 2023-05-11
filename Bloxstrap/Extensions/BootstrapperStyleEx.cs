using Bloxstrap.Dialogs;
using Bloxstrap.Enums;

namespace Bloxstrap.Extensions
{
    static class BootstrapperStyleEx
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
                BootstrapperStyle.ByfronDialog => new ByfronDialog(),
                _ => new FluentDialog()
            };
        }
    }
}
