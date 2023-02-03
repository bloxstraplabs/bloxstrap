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
            Form dialog;

            switch (bootstrapperStyle)
            {
                case BootstrapperStyle.VistaDialog:
                    dialog = new VistaDialog(bootstrapper);
                    break;

                case BootstrapperStyle.LegacyDialog2009:
                    dialog = new LegacyDialog2009(bootstrapper);
                    break;

                case BootstrapperStyle.LegacyDialog2011:
                    dialog = new LegacyDialog2011(bootstrapper);
                    break;

                case BootstrapperStyle.ProgressDialog:
                default:
                    dialog = new ProgressDialog(bootstrapper);
                    break;
            }

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
