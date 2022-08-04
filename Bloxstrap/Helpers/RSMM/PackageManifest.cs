// https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager/blob/main/ProjectSrc/Bootstrapper/PackageManifest.cs


namespace Bloxstrap.Helpers.RSMM
{
    internal class PackageManifest : List<Package>
    {
        public string RawData { get; private set; }

        private PackageManifest(string data)
        {
            using (var reader = new StringReader(data))
            {
                string version = reader.ReadLine();

                if (version != "v0")
                {
                    string errorMsg = $"Unexpected package manifest version: {version} (expected v0!)\n" +
                                       "Please contact MaximumADHD if you see this error.";

                    throw new NotSupportedException(errorMsg);
                }

                bool eof = false;

                var readLine = new Func<string>(() =>
                {
                    string line = reader.ReadLine();

                    if (line == null)
                        eof = true;

                    return line;
                });

                while (!eof)
                {
                    string fileName = readLine();
                    string signature = readLine();

                    string rawPackedSize = readLine();
                    string rawSize = readLine();

                    if (eof)
                        break;

                    if (!int.TryParse(rawPackedSize, out int packedSize))
                        break;

                    if (!int.TryParse(rawSize, out int size))
                        break;

                    if (fileName == "RobloxPlayerLauncher.exe")
                        break;

                    var package = new Package()
                    {
                        Name = fileName,
                        Signature = signature,
                        PackedSize = packedSize,
                        Size = size
                    };

                    Add(package);
                }
            }

            RawData = data;
        }

        public static async Task<PackageManifest> Get(string versionGuid)
        {
            string pkgManifestUrl = $"{Program.BaseUrlSetup}/{versionGuid}-rbxPkgManifest.txt";
            string pkgManifestData;

            using (HttpClient http = new())
            {
                var getData = http.GetStringAsync(pkgManifestUrl);
                pkgManifestData = await getData.ConfigureAwait(false);
            }

            return new PackageManifest(pkgManifestData);
        }
    }
}
