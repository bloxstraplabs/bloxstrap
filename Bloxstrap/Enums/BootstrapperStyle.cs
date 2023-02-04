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
    }

    public static class BootstrapperStyleEx
    {
        public static void Show(this BootstrapperStyle bootstrapperStyle, Bootstrapper? bootstrapper = null)
        {
            Form dialog = bootstrapperStyle switch
            {
                BootstrapperStyle.VistaDialog => new VistaDialog(bootstrapper),
                BootstrapperStyle.LegacyDialog2009 => new LegacyDialog2009(bootstrapper),
                BootstrapperStyle.LegacyDialog2011 => new LegacyDialog2011(bootstrapper),
                BootstrapperStyle.ProgressDialog => new ProgressDialog(bootstrapper),
                _ => new ProgressDialog(bootstrapper)
            };

            if (bootstrapper is null)
            {
                dialog.ShowDialog();
            }
            else
            {
                if (App.IsQuiet)
                {
                    dialog.Opacity = 0;
                    dialog.ShowInTaskbar = false;
                }

                Application.Run(dialog);
            }
        }
    }
}
