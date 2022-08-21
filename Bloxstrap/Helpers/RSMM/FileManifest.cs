// https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager/blob/main/ProjectSrc/Bootstrapper/FileManifest.cs

using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;

namespace Bloxstrap.Helpers.RSMM
{
    [Serializable]
    internal class FileManifest : Dictionary<string, string>
    {
        public string RawData { get; set; }

        protected FileManifest(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        private FileManifest(string data, bool remapExtraContent = false)
        {
            using (var reader = new StringReader(data))
            {
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
                    string path = readLine();
                    string signature = readLine();

                    if (eof)
                        break;
                    else if (remapExtraContent && path.StartsWith("ExtraContent", Program.StringFormat))
                        path = path.Replace("ExtraContent", "content");

                    Add(path, signature);
                }
            }

            RawData = data;
        }

        public static async Task<FileManifest> Get(string versionGuid, bool remapExtraContent = false)
        {
            string fileManifestUrl = $"{Program.BaseUrlSetup}/{versionGuid}-rbxManifest.txt";
            string fileManifestData;

            using (HttpClient http = new())
            {
                var download = http.GetStringAsync(fileManifestUrl);
                fileManifestData = await download.ConfigureAwait(false);
            }

            return new FileManifest(fileManifestData, remapExtraContent);
        }
    }
}
