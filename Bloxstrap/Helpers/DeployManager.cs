using System.Net.Http;
using System.Text.Json;
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

        private static string BuildBaseUrl(string channel)
        {
            if (channel == DefaultChannel)
                return DefaultBaseUrl;
            else
                return $"{DefaultBaseUrl}/channel/{channel.ToLower()}";
        }

        public static async Task<ClientVersion> GetLastDeploy(string channel, bool timestamp = false)
        {
            HttpResponseMessage deployInfoResponse = await Program.HttpClient.GetAsync($"https://clientsettings.roblox.com/v2/client-version/WindowsPlayer/channel/{channel}");

            if (!deployInfoResponse.IsSuccessStatusCode)
            {
                // 400 = Invalid binaryType.
                // 404 = Could not find version details for binaryType.
                // 500 = Error while fetching version information.
                // either way, we throw
                throw new Exception($"Could not get latest deploy for channel {channel}");
            }

            string rawJson = await deployInfoResponse.Content.ReadAsStringAsync();
            ClientVersion clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawJson)!;

            // for preferences
            if (timestamp)
            {
                string channelUrl = BuildBaseUrl(channel);

                // get an approximate deploy time from rbxpkgmanifest's last modified date
                HttpResponseMessage pkgResponse = await Program.HttpClient.GetAsync($"{channelUrl}/{clientVersion.VersionGuid}-rbxPkgManifest.txt");
                if (pkgResponse.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    clientVersion.Timestamp = DateTime.Parse(lastModified);
                }
            }

            return clientVersion;
        }
    }
}
