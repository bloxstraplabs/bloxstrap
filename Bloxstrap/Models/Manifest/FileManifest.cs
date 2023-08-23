namespace Bloxstrap.Models.Manifest
{
    public class FileManifest : List<ManifestFile>
    {
        private FileManifest(string data)
        {
            using StringReader reader = new StringReader(data);

            while (true)
            {
                string? fileName = reader.ReadLine();
                string? signature = reader.ReadLine();

                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(signature))
                    break;

                Add(new ManifestFile
                {
                    Name = fileName,
                    Signature = signature
                });
            }
        }

        public static async Task<FileManifest> Get(string versionGuid)
        {
            string pkgManifestUrl = RobloxDeployment.GetLocation($"/{versionGuid}-rbxManifest.txt");
            var pkgManifestData = await App.HttpClient.GetStringAsync(pkgManifestUrl);

            return new FileManifest(pkgManifestData);
        }
    }
}
