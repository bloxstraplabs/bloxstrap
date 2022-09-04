using Microsoft.Win32;
using Bloxstrap.Dialogs.BootstrapperStyles;

namespace Bloxstrap.Enums
{
    public enum BootstrapperStyle
    {
        VistaDialog,
        LegacyDialog2009,
        LegacyDialog2011,
        ProgressDialog,
        ProgressDialogDark,
        SystemTheme,
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

                case BootstrapperStyle.ProgressDialogDark:
                    dialog = new ProgressDialogDark(bootstrapper);
                    break;

                case BootstrapperStyle.SystemTheme:
                    bool darkMode = false;
                    using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    {
                        var value = key?.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            darkMode = (int)value <= 0;
                        }
                    }

                    dialog = !darkMode ? new ProgressDialog(bootstrapper) : new ProgressDialogDark(bootstrapper);
                    break;
            }

            if (bootstrapper is null)
            {
                dialog.ShowDialog();
            }
            else
            {
                Application.Run(dialog);
            }
        }
    }
}
