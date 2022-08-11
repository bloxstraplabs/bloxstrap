using System.Diagnostics;

using Bloxstrap.Enums;
using Bloxstrap.Dialogs.BootstrapperStyles;
using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap
{
    public partial class Bootstrapper
    {
        public Bootstrapper()
        {
            if (Program.BaseDirectory is null)
                return;

            FreshInstall = String.IsNullOrEmpty(Program.Settings.VersionGuid);
            DownloadsFolder = Path.Combine(Program.BaseDirectory, "Downloads");
            Client.Timeout = TimeSpan.FromMinutes(10);
        }

        public void Initialize(BootstrapperStyle bootstrapperStyle, string? launchCommandLine = null)
        {
            LaunchCommandLine = launchCommandLine;

            switch (bootstrapperStyle)
            {
                case BootstrapperStyle.VistaDialog:
                    new VistaDialog(this);
                    break;

                case BootstrapperStyle.LegacyDialog:
                    Application.Run(new LegacyDialog(this));
                    break;

                case BootstrapperStyle.ProgressDialog:
                    Application.Run(new ProgressDialog(this));
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

            Program.Exit();
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

            using (SystemEvent startEvent = new(startEventName))
            {
                Process gameClient = Process.Start(Path.Combine(VersionFolder, "RobloxPlayerBeta.exe"), LaunchCommandLine);

                bool startEventFired = await startEvent.WaitForEvent();

                startEvent.Close();

                if (!startEventFired)
                    return;

                // event fired, wait for 6 seconds then close
                await Task.Delay(6000);

                // now we move onto handling rich presence
                // except beta app launch since we have to rely strictly on website launch
                if (!Program.Settings.UseDiscordRichPresence || LaunchCommandLine.Contains("--app"))
                    return;

                // probably not the most ideal way to do this
                string? placeId = Utilities.GetKeyValue(LaunchCommandLine, "placeId=", '&');
                
                if (placeId is null)
                    return;

                // keep bloxstrap open to handle rich presence
                using (DiscordRichPresence richPresence = new())
                {
                    bool presenceSet = await richPresence.SetPresence(placeId);

                    if (!presenceSet)
                        return;

                    CloseDialog();
                    await gameClient.WaitForExitAsync();
                }
            }
        }

        public void CancelButtonClicked()
        {
            if (Program.BaseDirectory is null)
                return;

            CancelFired = true;

            try
            {
                if (Program.IsFirstRun)
                    Directory.Delete(Program.BaseDirectory, true);
                else if (Directory.Exists(VersionFolder))
                    Directory.Delete(VersionFolder, true);
            }
            catch (Exception) { }
 
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

        private void CloseDialog()
        {
            CloseDialogEvent.Invoke(this, new EventArgs());
        }
    }
}
