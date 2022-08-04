using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

using Microsoft.Win32;

using Bloxstrap.Enums;
using Bloxstrap.Dialogs.BootstrapperStyles;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap
{
    public class Bootstrapper
    {
        private string? LaunchCommandLine;

        private string VersionGuid;
        private PackageManifest VersionPackageManifest;
        private FileManifest VersionFileManifest;
        private string VersionFolder;

        private readonly string DownloadsFolder;
        private readonly bool FreshInstall;

        private int ProgressIncrement;
        private bool CancelFired = false;

        private static readonly HttpClient Client = new();

        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        // TODO - there ideally should be a less static way to do this that's not hardcoded?
        private static readonly IReadOnlyDictionary<string, string> PackageDirectories = new Dictionary<string, string>()
        {
            { "RobloxApp.zip",                 @"" },
            { "shaders.zip",                   @"shaders\" },
            { "ssl.zip",                       @"ssl\" },

            { "content-avatar.zip",            @"content\avatar\" },
            { "content-configs.zip",           @"content\configs\" },
            { "content-fonts.zip",             @"content\fonts\" },
            { "content-sky.zip",               @"content\sky\" },
            { "content-sounds.zip",            @"content\sounds\" },
            { "content-textures2.zip",         @"content\textures\" },
            { "content-models.zip",            @"content\models\" },

            { "content-textures3.zip",         @"PlatformContent\pc\textures\" },
            { "content-terrain.zip",           @"PlatformContent\pc\terrain\" },
            { "content-platform-fonts.zip",    @"PlatformContent\pc\fonts\" },

            { "extracontent-luapackages.zip",  @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip",       @"ExtraContent\models\" },
            { "extracontent-textures.zip",     @"ExtraContent\textures\" },
            { "extracontent-places.zip",       @"ExtraContent\places\" },
        };

        private static readonly string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<Settings>\n" +
            "	<ContentFolder>content</ContentFolder>\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>\n";

        public event EventHandler PromptShutdownEvent;
        public event ChangeEventHandler<string> ShowSuccessEvent;
        public event ChangeEventHandler<string> MessageChanged;
        public event ChangeEventHandler<int> ProgressBarValueChanged;
        public event ChangeEventHandler<ProgressBarStyle> ProgressBarStyleChanged;
        public event ChangeEventHandler<bool> CancelEnabledChanged;

        private string _message;
        private int _progress = 0;
        private ProgressBarStyle _progressStyle = ProgressBarStyle.Marquee;
        private bool _cancelEnabled = false;

        public string Message
        {
            get => _message;

            private set
            {
                if (_message == value)
                    return;

                MessageChanged.Invoke(this, new ChangeEventArgs<string>(value));

                _message = value;
            }
        }

        public int Progress
        {
            get => _progress;

            private set
            {
                if (_progress == value)
                    return;

                ProgressBarValueChanged.Invoke(this, new ChangeEventArgs<int>(value));

                _progress = value;
            }
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _progressStyle;

            private set
            {
                if (_progressStyle == value)
                    return;

                ProgressBarStyleChanged.Invoke(this, new ChangeEventArgs<ProgressBarStyle>(value));

                _progressStyle = value;
            }
        }

        public bool CancelEnabled
        {
            get => _cancelEnabled;

            private set
            {
                if (_cancelEnabled == value)
                    return;

                CancelEnabledChanged.Invoke(this, new ChangeEventArgs<bool>(value));

                _cancelEnabled = value;
            }
        }

        public Bootstrapper(BootstrapperStyle bootstrapperStyle, string? launchCommandLine = null)
        {
            Debug.WriteLine("Initializing bootstrapper");

            FreshInstall = String.IsNullOrEmpty(Program.Settings.VersionGuid);
            LaunchCommandLine = launchCommandLine;
            DownloadsFolder = Path.Combine(Program.BaseDirectory, "Downloads");
            Client.Timeout = TimeSpan.FromMinutes(10);

            switch (bootstrapperStyle)
            {
                case BootstrapperStyle.TaskDialog:
                    new TaskDialogStyle(this);
                    break;

                case BootstrapperStyle.LegacyDialog:
                    Application.Run(new LegacyDialogStyle(this));
                    break;

                case BootstrapperStyle.ProgressDialog:
                    Application.Run(new ProgressDialogStyle(this));
                    break;
            }
        }

        public async Task Run()
        {
            if (LaunchCommandLine == "-uninstall")
            {
                Uninstall();
                return;
            }

            await CheckLatestVersion();

            if (!Directory.Exists(VersionFolder) || Program.Settings.VersionGuid != VersionGuid)
            {
                Debug.WriteLineIf(!Directory.Exists(VersionFolder), $"Installing latest version (!Directory.Exists({VersionFolder}))");
                Debug.WriteLineIf(Program.Settings.VersionGuid != VersionGuid, $"Installing latest version ({Program.Settings.VersionGuid} != {VersionGuid})");

                await InstallLatestVersion();
            }

            // yes, doing this for every start is stupid, but the death sound mod is dynamically toggleable after all
            ApplyModifications();

            if (Program.IsFirstRun)
                Program.SettingsManager.ShouldSave = true;

            if (Program.IsFirstRun || FreshInstall)
                Register();

             CheckInstall();

            await StartRoblox();
        }

        private void CheckIfRunning()
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            if (processes.Length > 0)
                PromptShutdown();

            try
            {
                // try/catch just in case process was closed before prompt was answered

                foreach (Process process in processes)
                {
                    process.CloseMainWindow();
                    process.Close();
                }
            }
            catch (Exception) { }
        }

        private async Task StartRoblox()
        {
            string startEventName = Program.ProjectName.Replace(" ", "") + "StartEvent";

            Message = "Starting Roblox...";

            // launch time isn't really required for all launches, but it's usually just safest to do this
            LaunchCommandLine += " --launchtime=" + DateTimeOffset.Now.ToUnixTimeSeconds() + " -startEvent " + startEventName;
            Debug.WriteLine($"Starting game client with command line '{LaunchCommandLine}'");

            using (SystemEvent startEvent = new(startEventName))
            {
                Process.Start(Path.Combine(VersionFolder, "RobloxPlayerBeta.exe"), LaunchCommandLine);

                Debug.WriteLine($"Waiting for {startEventName} event to be fired...");
                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (startEventFired)
                {
                    Debug.WriteLine($"{startEventName} event fired! Exiting in 5 seconds...");
                    await Task.Delay(5000);

                    Program.Exit();
                }
            }
        }

        // Bootstrapper Installing

        public static void Register()
        {
            RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{Program.ProjectName}");

            // new install location selected, delete old one
            string? oldInstallLocation = (string?)applicationKey.GetValue("OldInstallLocation");
            if (!String.IsNullOrEmpty(oldInstallLocation) && oldInstallLocation != Program.BaseDirectory)
            {
                try
                {
                    if (Directory.Exists(oldInstallLocation))
                        Directory.Delete(oldInstallLocation, true);
                }
                catch (Exception) { }

                applicationKey.DeleteValue("OldInstallLocation");
            }
            
            applicationKey.SetValue("InstallLocation", Program.BaseDirectory);
            applicationKey.Close();

            // set uninstall key
            RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");
            uninstallKey.SetValue("DisplayIcon", $"{Program.FilePath},0");
            uninstallKey.SetValue("DisplayName", Program.ProjectName);
            uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            uninstallKey.SetValue("InstallLocation", Program.BaseDirectory);
            // uninstallKey.SetValue("NoModify", 1);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", Program.ProjectName);
            uninstallKey.SetValue("ModifyPath", $"\"{Program.FilePath}\" -preferences");
            uninstallKey.SetValue("UninstallString", $"\"{Program.FilePath}\" -uninstall");
            uninstallKey.Close();
        }

        public static void CheckInstall()
        {
            // check if launch uri is set to our bootstrapper
            // this doesn't go under register, so we check every launch
            // just in case the stock bootstrapper changes it back

            Protocol.Register("roblox", "Roblox", Program.FilePath);
            Protocol.Register("roblox-player", "Roblox", Program.FilePath);

            // in case the user is reinstalling
            if (File.Exists(Program.FilePath) && Program.IsFirstRun)
                File.Delete(Program.FilePath);

            // check to make sure bootstrapper is in the install folder
            if (!File.Exists(Program.FilePath) && Environment.ProcessPath is not null)
                File.Copy(Environment.ProcessPath, Program.FilePath);
        }

        private void Uninstall()
        {
            CheckIfRunning();

            // lots of try/catches here... lol

            Message = $"Uninstalling {Program.ProjectName}...";

            Program.SettingsManager.ShouldSave = false;

            // check if stock bootstrapper is still installed
            RegistryKey? bootstrapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\roblox-player");
            if (bootstrapperKey is null)
            {
                Protocol.Unregister("roblox");
                Protocol.Unregister("roblox-player");
            }
            else
            {
                // revert launch uri handler to stock bootstrapper

                string bootstrapperLocation = (string?)bootstrapperKey.GetValue("InstallLocation") + "RobloxPlayerLauncher.exe";

                Protocol.Register("roblox", "Roblox", bootstrapperLocation);
                Protocol.Register("roblox-player", "Roblox", bootstrapperLocation);
            }

            try
            {
                // delete application key
                Registry.CurrentUser.DeleteSubKey($@"Software\{Program.ProjectName}");
            }
            catch (Exception) { }

            try
            {
                // delete installation folder
                // (should delete everything except bloxstrap itself)
                Directory.Delete(Program.BaseDirectory, true);
            }
            catch (Exception) { }

            try
            {
                // delete uninstall key
                Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Program.ProjectName}");
            }
            catch (Exception) { }

            ShowSuccess($"{Program.ProjectName} has been uninstalled");
            Program.Exit();
        }

        // Roblox Installing

        private async Task CheckLatestVersion()
        {
            Message = "Connecting to Roblox...";

            Debug.WriteLine($"Checking latest version...");
            VersionGuid = await Client.GetStringAsync($"{Program.BaseUrlSetup}/version");
            VersionFolder = Path.Combine(Program.BaseDirectory, "Versions", VersionGuid);
            Debug.WriteLine($"Latest version is {VersionGuid}");

            Debug.WriteLine("Getting package manifest...");
            VersionPackageManifest = await PackageManifest.Get(VersionGuid);

            Debug.WriteLine("Getting file manifest...");
            VersionFileManifest = await FileManifest.Get(VersionGuid);
        }

        private async Task InstallLatestVersion()
        {
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

            ProgressIncrement = (int)Math.Floor((decimal) 1 / VersionPackageManifest.Count * 100);
            Debug.WriteLine($"Progress Increment is {ProgressIncrement}");

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
                Directory.Delete(Path.Combine(Program.BaseDirectory, "Versions", Program.Settings.VersionGuid));
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

            string fileContentName = "ouch.ogg";
            string fileContentLocation = "content\\sounds\\ouch.ogg";
            string fileLocation = Path.Combine(VersionFolder, fileContentLocation);

            string officialDeathSoundHash = VersionFileManifest[fileContentLocation];
            string currentDeathSoundHash = CalculateMD5(fileLocation);

            if (Program.Settings.UseOldDeathSound && currentDeathSoundHash == officialDeathSoundHash)
            {
                // let's get the old one!

                Debug.WriteLine($"Fetching old death sound...");

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

                Debug.WriteLine("Fetching current death sound...");

                var package = VersionPackageManifest.Find(x => x.Name == "content-sounds.zip");

                if (package is null)
                {
                    Debug.WriteLine("Failed to find content-sounds.zip package! Aborting...");
                    return;
                }

                DownloadPackage(package);

                string packageLocation = Path.Combine(DownloadsFolder, package.Signature);
                string packageFolder = Path.Combine(VersionFolder, PackageDirectories[package.Name]);

                using (ZipArchive archive = ZipFile.OpenRead(packageLocation))
                {
                    ZipArchiveEntry? entry = archive.Entries.Where(x => x.FullName == fileContentName).FirstOrDefault();

                    if (entry is null)
                    {
                        Debug.WriteLine("Failed to find file entry in content-sounds.zip! Aborting...");
                        return;
                    }

                    if (File.Exists(fileLocation))
                        File.Delete(fileLocation);

                    entry.ExtractToFile(fileLocation);
                }
            }
        }

        private async void DownloadPackage(Package package)
        {
            string packageUrl = $"{Program.BaseUrlSetup}/{VersionGuid}-{package.Name}";
            string packageLocation = Path.Combine(DownloadsFolder, package.Signature);
            string robloxPackageLocation = Path.Combine(Program.LocalAppData, "Roblox", "Downloads", package.Signature);

            if (File.Exists(packageLocation))
            {
                FileInfo file = new(packageLocation);

                string calculatedMD5 = CalculateMD5(packageLocation);
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

        // Dialog Events

        public void CancelButtonClicked()
        {
            CancelFired = true;

            try
            {
                if (Program.IsFirstRun)
                    Directory.Delete(Program.BaseDirectory, true);
                else if (Directory.Exists(VersionFolder))
                    Directory.Delete(VersionFolder, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cleanup install!\n\n{ex}");
            }
 
            Program.Exit();
        }

        private void ShowSuccess(string message)
        {
            ShowSuccessEvent.Invoke(this, new ChangeEventArgs<string>(message));
        }

        private void PromptShutdown()
        {
            PromptShutdownEvent.Invoke(this, new EventArgs());
        }

        // Utilities

        private static string CalculateMD5(string filename)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
