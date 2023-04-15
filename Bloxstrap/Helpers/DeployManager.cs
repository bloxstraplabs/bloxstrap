using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Bloxstrap.Models;
using DiscordRPC;

namespace Bloxstrap.Helpers
{
    public class DeployManager
    {
        #region Properties
        public const string DefaultBaseUrl = "https://setup.rbxcdn.com";
        public const string DefaultChannel = "LIVE";
        
        public string BaseUrl { get; private set; } = DefaultBaseUrl;
        public string Channel { get; private set; } = DefaultChannel;

        // most commonly used/interesting channels
        public static readonly List<string> SelectableChannels = new()
        {
            "LIVE",
            "ZFlag",
            "ZNext",
            "ZCanary",
            "ZIntegration",
            "ZAvatarTeam",
            "ZSocialTeam"
        };
        #endregion

        public void SetChannel(string channel)
        {
            if (Channel == channel) 
                return;

            App.Logger.WriteLine($"[DeployManager::SetChannel] Set channel to {Channel}");

            Channel = channel;
            BaseUrl = channel == DefaultChannel ? DefaultBaseUrl : $"{DefaultBaseUrl}/channel/{channel.ToLower()}";
        }

        public async Task<ClientVersion> GetLastDeploy(bool timestamp = false)
        {
            App.Logger.WriteLine($"[DeployManager::GetLastDeploy] Getting deploy info for channel {Channel} (timestamp={timestamp})");

            HttpResponseMessage deployInfoResponse = await App.HttpClient.GetAsync($"https://clientsettings.roblox.com/v2/client-version/WindowsPlayer/channel/{Channel}");

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
    }
}
