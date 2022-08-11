using System.Diagnostics;
using System.IO.Compression;

using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap
{
    partial class Bootstrapper
    {
        private async Task CheckLatestVersion()
        {
            if (Program.BaseDirectory is null)
                return;

            Message = "Connecting to Roblox...";

            VersionGuid = await Client.GetStringAsync($"{Program.BaseUrlSetup}/version");
            VersionFolder = Path.Combine(Program.BaseDirectory, "Versions", VersionGuid);
            VersionPackageManifest = await PackageManifest.Get(VersionGuid);
            VersionFileManifest = await FileManifest.Get(VersionGuid);
        }

        private async Task InstallLatestVersion()
        {
            if (Program.BaseDirectory is null)
                return;

            CheckIfRunning();

            if (FreshInstall)
                Message = "Installing Roblox...";
            else
                Message = "Upgrading Roblox...";

            Directory.CreateDirectory(Program.BaseDirectory);

            CancelEnabled = true;

            // i believe the original bootstrapper bases the progress bar off zip
            // extraction progress, but here i'm doing package download progress

            ProgressStyle = ProgressBarStyle.Continuous;

            ProgressIncrement = (int)Math.Floor((decimal)1 / VersionPackageManifest.Count * 100);

            Directory.CreateDirectory(Path.Combine(Program.BaseDirectory, "Downloads"));

            foreach (Package package in VersionPackageManifest)
            {
                // no await, download all the packages at once
                DownloadPackage(package);
            }

            do
            {
                // wait for download to finish (and also round off the progress bar if needed)

                if (Progress == ProgressIncrement * VersionPackageManifest.Count)
                    Progress = 100;

                await Task.Delay(1000);
            }
            while (Progress != 100);

            ProgressStyle = ProgressBarStyle.Marquee;

            Debug.WriteLine("Finished downloading");

            Directory.CreateDirectory(Path.Combine(Program.BaseDirectory, "Versions"));

            foreach (Package package in VersionPackageManifest)
            {
                // extract all the packages at once (shouldn't be too heavy on cpu?)
                ExtractPackage(package);
            }

            Debug.WriteLine("Finished extracting packages");

            Message = "Configuring Roblox...";

            string appSettingsLocation = Path.Combine(VersionFolder, "AppSettings.xml");
            await File.WriteAllTextAsync(appSettingsLocation, AppSettings);

            if (!FreshInstall)
            {
                // let's take this opportunity to delete any packages we don't need anymore
                foreach (string filename in Directory.GetFiles(DownloadsFolder))
                {
                    if (!VersionPackageManifest.Exists(package => filename.Contains(package.Signature)))
                        File.Delete(filename);
                }

                // and also to delete our old version folder
                Directory.Delete(Path.Combine(Program.BaseDirectory, "Versions", Program.Settings.VersionGuid), true);
            }

            CancelEnabled = false;

            Program.Settings.VersionGuid = VersionGuid;
        }

        private async void ApplyModifications()
        {
            // i guess we can just assume that if the hash does not match the manifest, then it's a mod
            // probably not the best way to do this? don't think file corruption is that much of a worry here

            // TODO - i'm thinking i could have a manifest on my website like rbxManifest.txt
            // for integrity checking and to quickly fix/alter stuff (like ouch.ogg being renamed)
            // but that probably wouldn't be great to check on every run in case my webserver ever goes down
            // interesting idea nonetheless, might add it sometime

            // TODO - i'm hoping i can take this idea of content mods much further
            // for stuff like easily installing (community-created?) texture/shader/audio mods
            // but for now, let's just keep it at this

            await ModifyDeathSound();
        }

        private async void DownloadPackage(Package package)
        {
            string packageUrl = $"{Program.BaseUrlSetup}/{VersionGuid}-{package.Name}";
            string packageLocation = Path.Combine(DownloadsFolder, package.Signature);
            string robloxPackageLocation = Path.Combine(Program.LocalAppData, "Roblox", "Downloads", package.Signature);

            if (File.Exists(packageLocation))
            {
                FileInfo file = new(packageLocation);

                string calculatedMD5 = Utilities.CalculateMD5(packageLocation);
                if (calculatedMD5 != package.Signature)
                {
                    Debug.WriteLine($"{package.Name} is corrupted ({calculatedMD5} != {package.Signature})! Deleting and re-downloading...");
                    file.Delete();
                }
                else
                {
                    Debug.WriteLine($"{package.Name} is already downloaded, skipping...");
                    Progress += ProgressIncrement;
                    return;
                }
            }
            else if (File.Exists(robloxPackageLocation))
            {
                // let's cheat! if the stock bootstrapper already previously downloaded the file,
                // then we can just copy the one from there

                Debug.WriteLine($"Found existing version of {package.Name} ({robloxPackageLocation})! Copying to Downloads folder...");
                File.Copy(robloxPackageLocation, packageLocation);
                Progress += ProgressIncrement;
                return;
            }

            if (!File.Exists(packageLocation))
            {
                Debug.WriteLine($"Downloading {package.Name}...");

                var response = await Client.GetAsync(packageUrl);

                if (CancelFired)
                    return;

                using (var fileStream = new FileStream(packageLocation, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                Debug.WriteLine($"Finished downloading {package.Name}!");
                Progress += ProgressIncrement;
            }
        }

        private void ExtractPackage(Package package)
        {
            if (CancelFired)
                return;

            string packageLocation = Path.Combine(DownloadsFolder, package.Signature);
            string packageFolder = Path.Combine(VersionFolder, PackageDirectories[package.Name]);
            string extractPath;

            Debug.WriteLine($"Extracting {package.Name} to {packageFolder}...");

            using (ZipArchive archive = ZipFile.OpenRead(packageLocation))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (CancelFired)
                        return;

                    if (entry.FullName.EndsWith(@"\"))
                        continue;

                    extractPath = Path.Combine(packageFolder, entry.FullName);

                    Debug.WriteLine($"[{package.Name}] Writing {extractPath}...");

                    Directory.CreateDirectory(Path.GetDirectoryName(extractPath));

                    if (File.Exists(extractPath))
                        File.Delete(extractPath);

                    entry.ExtractToFile(extractPath);
                }
            }
        }
    }
}
