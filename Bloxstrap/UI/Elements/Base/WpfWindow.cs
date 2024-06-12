using System.Windows;
using System.Windows.Interop;

namespace Bloxstrap.UI.Elements.Base
{
    public class WpfWindow : Window
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.SoftwareRenderingEnabled)
            {
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

                if (hwndSource != null)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            base.OnSourceInitialized(e);
        }
    }
}
