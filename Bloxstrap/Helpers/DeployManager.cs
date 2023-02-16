using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Bloxstrap.Models;

namespace Bloxstrap.Helpers
{
    public class DeployManager
    {
        #region Properties
        public const string DefaultBaseUrl = "https://setup.rbxcdn.com";
        public static string BaseUrl { get; private set; } = DefaultBaseUrl;

        public const string DefaultChannel = "LIVE";
        public static string Channel { set => BaseUrl = BuildBaseUrl(value); }

        // basically any channel that has had a deploy within the past month with a windowsplayer build
        public static readonly List<string> ChannelsAbstracted = new List<string>()
        {
            "LIVE",
            "ZNext",
            "ZCanary",
            "ZIntegration"
        };

        // why not?
        public static readonly List<string> ChannelsAll = new List<string>()
        {
            "LIVE",
            "ZAvatarTeam",
            "ZAvatarRelease",
            "ZCanary",
            "ZCanary1",
            "ZCanary2",
            "ZCanary3",
            "ZCanaryApps",
            "ZFlag",
            "ZIntegration",
            "ZIntegration1",
            "ZLive",
            "ZLive1",
            "ZNext",
            "ZSocialTeam",
            "ZStudioInt1",
            "ZStudioInt2"
        };
        #endregion

        private static string BuildBaseUrl(string channel) => channel == DefaultChannel ? DefaultBaseUrl : $"{DefaultBaseUrl}/channel/{channel.ToLower()}";

        public static async Task<ClientVersion> GetLastDeploy(string channel, bool timestamp = false)
        {
            App.Logger.WriteLine($"[DeployManager::GetLastDeploy] Getting deploy info for channel {channel} (timestamp={timestamp})");

            HttpResponseMessage deployInfoResponse = await App.HttpClient.GetAsync($"https://clientsettings.roblox.com/v2/client-version/WindowsPlayer/channel/{channel}");

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

                throw new Exception($"Could not get latest deploy for channel {channel}! (HTTP {deployInfoResponse.StatusCode})");
            }

            App.Logger.WriteLine($"[DeployManager::GetLastDeploy] Got JSON: {rawResponse}");

            ClientVersion clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawResponse)!;

            // for preferences
            if (timestamp)
            {
                App.Logger.WriteLine("[DeployManager::GetLastDeploy] Getting timestamp...");

                string channelUrl = BuildBaseUrl(channel);
                string manifestUrl = $"{channelUrl}/{clientVersion.VersionGuid}-rbxPkgManifest.txt";

                // get an approximate deploy time from rbxpkgmanifest's last modified date
                HttpResponseMessage pkgResponse = await App.HttpClient.GetAsync(manifestUrl);

                if (pkgResponse.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    App.Logger.WriteLine($"[DeployManager::GetLastDeploy] {manifestUrl} - Last-Modified: {lastModified}");
                    clientVersion.Timestamp = DateTime.Parse(lastModified);
                }
            }

            return clientVersion;
        }
    }
}
