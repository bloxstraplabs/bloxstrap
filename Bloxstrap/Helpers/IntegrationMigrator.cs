using System.IO;
using System.Windows;

namespace Bloxstrap.Helpers
{
    static class IntegrationMigrator
    {
        public static void Execute()
        {
            // v2.2.0 - remove rbxfpsunlocker
            string rbxfpsunlocker = Path.Combine(Directories.Integrations, "rbxfpsunlocker");

            if (Directory.Exists(rbxfpsunlocker))
                Directory.Delete(rbxfpsunlocker, true);

            // v2.2.0 - remove reshade
            string reshadeLocation = Path.Combine(Directories.Modifications, "dxgi.dll");

            if (File.Exists(reshadeLocation))
            {
				App.ShowMessageBox(
                    "As of April 18th, Roblox has started out rolling out the Byfron anticheat as well as 64-bit support. Because of this, ReShade will no longer work, and will be deactivated from now on.\n\n" +
                    $"Your ReShade configs and files will still be kept, which are all located in the {App.ProjectName} folder.", 
                    MessageBoxImage.Warning
                );

                File.Delete(reshadeLocation);

                if (App.FastFlags.GetValue(FastFlagManager.RenderingModes["Direct3D 11"]) == "True" && App.FastFlags.GetValue("FFlagHandleAltEnterFullscreenManually") != "False")
                    App.FastFlags.SetRenderingMode("Automatic");
			}
        }
    }
}
