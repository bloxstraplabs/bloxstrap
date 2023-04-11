using System.IO;

namespace Bloxstrap.Helpers
{
    static class IntegrationMigrator
    {
        public static void Execute()
        {
            App.FastFlags.Load();

            // v2.2.0 - remove rbxfpsunlocker
            string rbxfpsunlocker = Path.Combine(Directories.Integrations, "rbxfpsunlocker");

            if (Directory.Exists(rbxfpsunlocker))
                Directory.Delete(rbxfpsunlocker, true);
        }
    }
}
