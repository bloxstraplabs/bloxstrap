using Bloxstrap.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Integrations
{
    internal class IntegrationCleaner
    {
        private static void RemoveReshade()
        {
            App.Logger.WriteLine("[IntegrationCleaner::RemoveReshade] Removing ReShade...");

            File.Delete(Path.Combine(Directories.Modifications, "dxgi.dll"));
            File.Delete(Path.Combine(Directories.Modifications, "ReShade.ini"));

            string reshadeBaseFolder = Path.Combine(Directories.Integrations, "ReShade");
            if (Directory.Exists(reshadeBaseFolder))
            {
                Directory.Delete(reshadeBaseFolder, true);

                // reshade also forced dx11
                // so lets change that back!!!
                if (App.FastFlags.GetValue(FastFlagManager.RenderingModes["Direct3D 11"]) != null)
                    App.FastFlags.SetRenderingMode("Automatic");
            }
        }

        private static void KillRunningRbxFpsUnlocker()
        {
            Process[] processes = Process.GetProcessesByName("rbxfpsunlocker");

            if (processes.Length == 0)
                return;

            App.Logger.WriteLine("[IntegrationCleaner::KillRunningRbxFpsUnlocker] Closing currently running rbxfpsunlocker processes...");

            try
            {
                foreach (Process process in processes)
                {
                    if (process.MainModule?.FileName is null)
                        continue;

                    if (!process.MainModule.FileName.Contains(Directories.Base))
                        continue;

                    process.Kill();
                    process.Close();
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"[IntegrationCleaner::KillRunningRbxFpsUnlocker] Could not close rbxfpsunlocker process! {ex}");
            }
        }

        private static void RemoveRbxFpsUnlocker()
        {
            KillRunningRbxFpsUnlocker();

            string rfuBaseFolder = Path.Combine(Directories.Integrations, "rbxfpsunlocker");
            if (Directory.Exists(rfuBaseFolder))
                Directory.Delete(rfuBaseFolder, true);
        }

        public static void RemoveDeprecated()
        {
            App.Logger.WriteLine("[IntegrationCleaner::RemoveDeprecated] Removing all deprecated integrations...");

            RemoveRbxFpsUnlocker();
            RemoveReshade();

            App.Logger.WriteLine("[IntegrationCleaner::RemoveDeprecated] Deprecated integrations cleanup finished!");
        }
    }
}
