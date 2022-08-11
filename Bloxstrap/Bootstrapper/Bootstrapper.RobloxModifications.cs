using System.IO.Compression;

using Bloxstrap.Helpers;

namespace Bloxstrap
{
    partial class Bootstrapper
    {
        private async Task ModifyDeathSound()
        {
            string fileContentName = "ouch.ogg";
            string fileContentLocation = "content\\sounds\\ouch.ogg";
            string fileLocation = Path.Combine(VersionFolder, fileContentLocation);

            string officialDeathSoundHash = VersionFileManifest[fileContentLocation];
            string currentDeathSoundHash = Utilities.CalculateMD5(fileLocation);

            if (Program.Settings.UseOldDeathSound && currentDeathSoundHash == officialDeathSoundHash)
            {
                // let's get the old one!

                var response = await Client.GetAsync($"{Program.BaseUrlApplication}/mods/{fileContentLocation}");

                if (File.Exists(fileLocation))
                    File.Delete(fileLocation);

                using (var fileStream = new FileStream(fileLocation, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            else if (!Program.Settings.UseOldDeathSound && currentDeathSoundHash != officialDeathSoundHash)
            {
                // who's lame enough to ever do this?
                // well, we need to re-extract the one that's in the content-sounds.zip package

                var package = VersionPackageManifest.Find(x => x.Name == "content-sounds.zip");

                if (package is null)
                    return;

                DownloadPackage(package);

                string packageLocation = Path.Combine(DownloadsFolder, package.Signature);
                string packageFolder = Path.Combine(VersionFolder, PackageDirectories[package.Name]);

                using (ZipArchive archive = ZipFile.OpenRead(packageLocation))
                {
                    ZipArchiveEntry? entry = archive.Entries.Where(x => x.FullName == fileContentName).FirstOrDefault();

                    if (entry is null)
                        return;

                    if (File.Exists(fileLocation))
                        File.Delete(fileLocation);

                    entry.ExtractToFile(fileLocation);
                }
            }
        }
    }
}
