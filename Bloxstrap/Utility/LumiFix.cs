using Bloxstrap.AppData;
using Bloxstrap.Models.Manifest;
using Bloxstrap.RobloxInterfaces;

namespace Bloxstrap.Utility
{
    internal sealed class LumiFixResult
    {
        public bool IsInstalled { get; init; }
        public int FilesChecked { get; init; }
        public int MissingFiles { get; init; }
        public int ChangedFiles { get; init; }
        public int UnreadableFiles { get; init; }
        public int SkippedModifiedFiles { get; init; }

        public int ProblemCount => MissingFiles + ChangedFiles + UnreadableFiles;
    }

    internal static class LumiFix
    {
        public static async Task<LumiFixResult> CheckPlayerInstallationAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            const string LOG_IDENT = "LumiFix::CheckPlayerInstallation";

            var playerData = new RobloxPlayerData();
            string versionGuid = playerData.DistributionState.VersionGuid;

            if (String.IsNullOrWhiteSpace(versionGuid))
                return new LumiFixResult { IsInstalled = false };

            if (!String.Equals(Path.GetFileName(versionGuid), versionGuid, StringComparison.Ordinal)
                || versionGuid.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidDataException("The saved Roblox version identifier is invalid.");
            }

            string versionDirectory = Path.GetFullPath(playerData.Directory);
            string versionsDirectory = Path.GetFullPath(Paths.Versions) + Path.DirectorySeparatorChar;

            if (!versionDirectory.StartsWith(versionsDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The saved Roblox version path is outside the installation folder.");

            if (String.IsNullOrEmpty(Deployment.BaseUrl))
            {
                Exception? connectionError = await Deployment.InitializeConnectivity();
                if (connectionError is not null)
                    throw new HttpRequestException("LumiFix could not connect to a Roblox download server.", connectionError);
            }

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(Strings.Menu_LumiFix_DownloadingManifest);

            FileManifest manifest = await FileManifest.Get(versionGuid);
            if (manifest.Count == 0)
                throw new InvalidDataException("Roblox returned an empty file manifest.");

            var modifiedFiles = new HashSet<string>(
                playerData.DistributionState.ModManifest.Select(NormalizeRelativePath),
                StringComparer.OrdinalIgnoreCase
            );

            App.Logger.WriteLine(LOG_IDENT, $"Checking {manifest.Count} files for Roblox {versionGuid}");

            return await Task.Run(() =>
            {
                int checkedFiles = 0;
                int missingFiles = 0;
                int changedFiles = 0;
                int unreadableFiles = 0;
                int skippedModifiedFiles = 0;

                foreach (ManifestFile file in manifest)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relativePath = NormalizeRelativePath(file.Name);
                    if (modifiedFiles.Contains(relativePath))
                    {
                        skippedModifiedFiles++;
                        continue;
                    }

                    string filePath = Path.GetFullPath(Path.Combine(versionDirectory, relativePath));
                    string rootPrefix = versionDirectory + Path.DirectorySeparatorChar;

                    if (!filePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Roblox returned an unsafe file path: {file.Name}");

                    checkedFiles++;

                    if (!File.Exists(filePath))
                    {
                        missingFiles++;
                    }
                    else
                    {
                        try
                        {
                            string actualSignature = MD5Hash.FromFile(filePath);
                            if (!String.Equals(actualSignature, file.Signature, StringComparison.OrdinalIgnoreCase))
                                changedFiles++;
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            unreadableFiles++;
                            App.Logger.WriteLine(LOG_IDENT, $"Could not inspect '{file.Name}': {ex.Message}");
                        }
                    }

                    if (checkedFiles % 25 == 0 || checkedFiles == manifest.Count - skippedModifiedFiles)
                    {
                        progress?.Report(String.Format(Strings.Menu_LumiFix_Progress, checkedFiles, manifest.Count - skippedModifiedFiles));
                    }
                }

                App.Logger.WriteLine(LOG_IDENT, $"Finished: checked={checkedFiles}, missing={missingFiles}, changed={changedFiles}, unreadable={unreadableFiles}, skippedMods={skippedModifiedFiles}");

                return new LumiFixResult
                {
                    IsInstalled = true,
                    FilesChecked = checkedFiles,
                    MissingFiles = missingFiles,
                    ChangedFiles = changedFiles,
                    UnreadableFiles = unreadableFiles,
                    SkippedModifiedFiles = skippedModifiedFiles
                };
            }, cancellationToken);
        }

        private static string NormalizeRelativePath(string path)
            => path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }
}
