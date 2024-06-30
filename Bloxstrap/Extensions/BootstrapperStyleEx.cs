namespace Bloxstrap.Extensions
{
    static class BootstrapperStyleEx
    {
        public static IBootstrapperDialog GetNew(this BootstrapperStyle bootstrapperStyle) => Frontend.GetBootstrapperDialog(bootstrapperStyle);

        public static IReadOnlyCollection<BootstrapperStyle> Selections => new BootstrapperStyle[]
        {
            BootstrapperStyle.FluentDialog,
            BootstrapperStyle.FluentAeroDialog,
            BootstrapperStyle.ClassicFluentDialog,
            BootstrapperStyle.ByfronDialog,
            BootstrapperStyle.ProgressDialog,
            BootstrapperStyle.LegacyDialog2011,
            BootstrapperStyle.LegacyDialog2008,
            BootstrapperStyle.VistaDialog,
            BootstrapperStyle.Yeezus,
            BootstrapperStyle.Terminal
        };
    }
}
