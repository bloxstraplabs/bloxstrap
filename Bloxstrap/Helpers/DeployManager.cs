using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

using Bloxstrap.Models;

namespace Bloxstrap.Helpers
{
    // TODO - make this functional and into a helper instead of a singleton, this really doesn't need to be OOP

    public class DeployManager
    {
        #region Properties
        public const string DefaultChannel = "LIVE";

        public string Channel = DefaultChannel;

        // a list of roblox delpoyment locations that we check for, in case one of them don't work
        private List<string> BaseUrls = new()
        {
            "https://setup.rbxcdn.com",
            "https://setup-ak.rbxcdn.com",
            "https://s3.amazonaws.com/setup.roblox.com"
        };

        private string? _baseUrl = null;

        public string BaseUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_baseUrl))
                {
                    // check for a working accessible deployment domain
                    foreach (string attemptedUrl in BaseUrls)
                    {
                        App.Logger.WriteLine($"[DeployManager::DefaultBaseUrl.Set] Testing connection to '{attemptedUrl}'...");

                        try
                        {
                            App.HttpClient.GetAsync($"{attemptedUrl}/version").Wait();
                            App.Logger.WriteLine($"[DeployManager::DefaultBaseUrl.Set] Connection successful!");
                            _baseUrl = attemptedUrl;
                            break;
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteLine($"[DeployManager::DefaultBaseUrl.Set] Connection failed!");
                            App.Logger.WriteLine($"[DeployManager::DefaultBaseUrl.Set] {ex}");
                            continue;
                        }
                    }

                    if (String.IsNullOrEmpty(_baseUrl))
                        throw new Exception("Unable to find an accessible Roblox deploy mirror!");
                }

                if (Channel == DefaultChannel)
                    return _baseUrl; 
                else
                    return $"{_baseUrl}/channel/{Channel.ToLower()}";
            }
        }

        // most commonly used/interesting channels
        public static readonly List<string> SelectableChannels = new()
        {
            "LIVE",
            "ZWinPlayer64",
            "ZFlag",
            "ZNext",
            "ZCanary",
            "ZIntegration",
            "ZAvatarTeam",
            "ZSocialTeam"
        };
        #endregion

        public async Task<ClientVersion> GetLastDeploy(bool timestamp = false)
        {
            App.Logger.WriteLine($"[DeployManager::GetLastDeploy] Getting deploy info for channel {Channel} (timestamp={timestamp})");

            HttpResponseMessage deployInfoResponse = await App.HttpClient.GetAsync($"https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer/channel/{Channel}").ConfigureAwait(false);

            string rawResponse = await deployInfoResponse.Content.ReadAsStringAsync();

            if (!deployInfoResponse.IsSuccessStatusCode)
            {
                // 400 = Invalid binaryType.
                // 404 = Could not find version details for binaryType.
                // 500 = Error while fetching version information.
                // either way, we throw
                
                App.Logger.WriteLine(
                    "[DeployManager::GetLastDeploy] Failed to fetch deploy info!\r\n"+ 
                    $"\tStatus code: {deployInfoResponse.StatusCode}\r\n"+ 
                    $"\tResponse: {rawResponse}"
                );

                throw new Exception($"Could not get latest deploy for channel {Channel}! (HTTP {deployInfoResponse.StatusCode})");
            }

            App.Logger.WriteLine($"[DeployManager::GetLastDeploy] Got JSON: {rawResponse}");

            ClientVersion clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawResponse)!;

            // for preferences
            if (timestamp)
            {
                App.Logger.WriteLine("[DeployManager::GetLastDeploy] Getting timestamp...");

                string manifestUrl = $"{BaseUrl}/{clientVersion.VersionGuid}-rbxPkgManifest.txt";

                // get an approximate deploy time from rbxpkgmanifest's last modified date
                HttpResponseMessage pkgResponse = await App.HttpClient.GetAsync(manifestUrl);

                if (pkgResponse.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    App.Logger.WriteLine($"[DeployManager::GetLastDeploy] {manifestUrl} - Last-Modified: {lastModified}");
                    clientVersion.Timestamp = DateTime.Parse(lastModified).ToLocalTime();
                }
            }

            return clientVersion;
        }

        public async Task CheckReleaseChannel()
        {
            App.Logger.WriteLine($"[DeployManager::CheckReleaseChannel] Checking current Roblox release channel ({App.Settings.Prop.Channel})...");

            if (App.Settings.Prop.Channel.ToLower() == DefaultChannel.ToLower())
            {
                App.Logger.WriteLine($"[DeployManager::CheckReleaseChannel] Channel is already {DefaultChannel}");
                return;
            }

            ClientVersion versionInfo = await App.DeployManager.GetLastDeploy().ConfigureAwait(false);

            if (App.Settings.Prop.UseReShade)
            {
                string manifest = await App.HttpClient.GetStringAsync($"{App.DeployManager.BaseUrl}/{versionInfo.VersionGuid}-rbxManifest.txt");

                if (manifest.Contains("RobloxPlayerBeta.dll"))
                {
                    MessageBoxResult result = !App.Settings.Prop.PromptChannelChange ? MessageBoxResult.Yes : App.ShowMessageBox(
                        $"You currently have ReShade enabled, however your current preferred channel ({App.Settings.Prop.Channel}) does not support ReShade. Would you like to switch to {DefaultChannel}? ",
                        MessageBoxImage.Question,
                        MessageBoxButton.YesNo
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        SwitchToDefault();
                        return;
                    }
                }
            }

            // this SUCKS
            ClientVersion defaultChannelInfo = await new DeployManager().GetLastDeploy().ConfigureAwait(false);
            int defaultChannelVersion = Int32.Parse(defaultChannelInfo.Version.Split('.')[1]);
            int currentChannelVersion = Int32.Parse(versionInfo.Version.Split('.')[1]);

            if (currentChannelVersion < defaultChannelVersion)
            {
                MessageBoxResult result = !App.Settings.Prop.PromptChannelChange ? MessageBoxResult.Yes : App.ShowMessageBox(
                    $"Your current preferred channel ({App.Settings.Prop.Channel}) appears to no longer be receiving updates. Would you like to switch to {DefaultChannel}? ",
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.Yes)
                {
                    SwitchToDefault();
                    return;
                }
            }
        }

        public static void SwitchToDefault()
        {
            if (App.Settings.Prop.Channel.ToLower() == DefaultChannel.ToLower())
                return;

            App.DeployManager.Channel = App.Settings.Prop.Channel = DefaultChannel;
            App.Logger.WriteLine($"[DeployManager::CheckReleaseChannel] Changed Roblox release channel from {App.Settings.Prop.Channel} to {DefaultChannel}");
        }
    }
}
