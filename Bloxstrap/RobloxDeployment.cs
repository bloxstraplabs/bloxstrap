using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Bloxstrap.Models;

namespace Bloxstrap
{
    public static class RobloxDeployment
    {
        #region Properties
        public const string DefaultChannel = "LIVE";

        // a list of roblox delpoyment locations that we check for, in case one of them don't work
        private static List<string> BaseUrls = new()
        {
            "https://setup.rbxcdn.com",
            "https://setup-ak.rbxcdn.com",
            "https://s3.amazonaws.com/setup.roblox.com"
        };

        private static string? _baseUrl = null;

        public static string BaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_baseUrl))
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

                    if (string.IsNullOrEmpty(_baseUrl))
                        throw new Exception("Unable to find an accessible Roblox deploy mirror!");
                }

                return _baseUrl;
            }
        }

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

        public static string GetLocation(string resource, string? channel = null)
        {
            if (string.IsNullOrEmpty(channel))
                channel = App.Settings.Prop.Channel;

            string location = BaseUrl;

            if (channel.ToLower() != DefaultChannel.ToLower())
                location += $"/channel/{channel.ToLower()}";

            location += resource;

            return location;
        }

        public static async Task<ClientVersion> GetInfo(string channel, bool timestamp = false)
        {
            App.Logger.WriteLine($"[RobloxDeployment::GetInfo] Getting deploy info for channel {channel} (timestamp={timestamp})");

            HttpResponseMessage deployInfoResponse = await App.HttpClient.GetAsync($"https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer/channel/{channel}");

            string rawResponse = await deployInfoResponse.Content.ReadAsStringAsync();

            if (!deployInfoResponse.IsSuccessStatusCode)
            {
                // 400 = Invalid binaryType.
                // 404 = Could not find version details for binaryType.
                // 500 = Error while fetching version information.
                // either way, we throw

                App.Logger.WriteLine(
                    "[RobloxDeployment::GetInfo] Failed to fetch deploy info!\r\n" +
                    $"\tStatus code: {deployInfoResponse.StatusCode}\r\n" +
                    $"\tResponse: {rawResponse}"
                );

                throw new Exception($"Could not get latest deploy for channel {channel}! (HTTP {deployInfoResponse.StatusCode})");
            }

            App.Logger.WriteLine($"[RobloxDeployment::GetInfo] Got JSON: {rawResponse}");

            ClientVersion clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawResponse)!;

            // for preferences
            if (timestamp)
            {
                App.Logger.WriteLine("[RobloxDeployment::GetInfo] Getting timestamp...");

                string manifestUrl = GetLocation($"/{clientVersion.VersionGuid}-rbxPkgManifest.txt", channel);

                // get an approximate deploy time from rbxpkgmanifest's last modified date
                HttpResponseMessage pkgResponse = await App.HttpClient.GetAsync(manifestUrl);

                if (pkgResponse.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    App.Logger.WriteLine($"[RobloxDeployment::GetInfo] {manifestUrl} - Last-Modified: {lastModified}");
                    clientVersion.Timestamp = DateTime.Parse(lastModified).ToLocalTime();
                }
            }

            return clientVersion;
        }
    }
}
