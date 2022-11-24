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
            "ZAvatarTeam",
            "ZCanary",
            //"ZFeatureHarmony", last updated 9/20, shouldn't be here anymore
            "ZFlag",
            "ZIntegration",
            "ZLive",
            "ZNext",
            //"ZPublic",
            "ZSocialTeam"
        };

        // why not?
        public static readonly List<string> ChannelsAll = new List<string>()
        {
            "LIVE",
            "Ganesh",
            "ZAvatarTeam",
            "ZBugFixBoost-Mutex-Revert",
            "ZBugFixCLI-54676-Test",
            "ZBugFixCLI-55214-Master",
            "ZCanary",
            "ZCanary1",
            "ZCanary2",
            "ZCanaryApps",
            "ZClientIntegration",
            "ZClientWatcher",
            "ZFeatureBaseline",
            "ZFeatureBoost_Removal_Test_In_Prod",
            "ZFeatureFMOD-20115",
            "ZFeatureFMOD-Recording-Test",
            "ZFeatureHarmony",
            "ZFeatureHSR2CDNPlayTest",
            "ZFeatureHSR2CDNPlayTest2",
            "ZFeatureInstance-Parent-Weak-Ptr",
            "ZFeatureInstance-Parent-Weak-Ptr-2",
            "ZFeatureLTCG1",
            "ZFeatureLuaIInline1",
            "ZFeatureQt5.15",
            "ZFeatureRail",
            "ZFeatureRetchecksV2",
            "ZFeatureSubsystemAtomic",
            "ZFeatureSubsystemHttpClient",
            "ZFeatureTelemLife",
            "ZFeatureUse-New-RapidJson-In-Flag-Loading",
            "ZFlag",
            "ZIntegration",
            "ZIntegration1",
            "ZLang",
            "ZLive",
            "ZLive1",
            "ZLoom",
            "ZNext",
            "ZProject512-Boost-Remove-Mutex-1",
            "ZProject516-Boost-Remove-Mutex-Network",
            "ZPublic",
            "ZQtitanStudio",
            "ZQTitanStudioRelease",
            "ZReleaseVS2019",
            "ZSocialTeam",
            "ZStIntegration",
            "ZStudioInt1",
            "ZStudioInt2",
            "ZStudioInt3",
            "ZStudioInt4",
            "ZStudioInt5",
            "ZStudioInt6",
            "ZStudioInt7",
            "ZStudioInt8",
            "ZTesting",
            "ZVS2019"
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

                HttpResponseMessage pkgMessage = await Program.HttpClient.GetAsync($"{channelUrl}/{clientVersion.VersionGuid}-rbxPkgManifest.txt");
                if (pkgMessage.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    clientVersion.Timestamp = DateTime.Parse(lastModified);
                }
            }

            return clientVersion;
        }
    }
}
