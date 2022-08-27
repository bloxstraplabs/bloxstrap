using System.Globalization;
using System.IO;
using System.Net.Http;

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
            "ZIntegration",
            "ZLive",
            "ZNext",
            "ZPublic",
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

        public static async Task<VersionDeploy> GetLastDeploy(string channel)
        {
            string baseUrl = BuildBaseUrl(channel);
            string lastDeploy = "";

            using (HttpClient client = new())
            {
                string deployHistory = await client.GetStringAsync($"{baseUrl}/DeployHistory.txt");

                using (StringReader reader = new(deployHistory))
                {
                    string? line;

                    while ((line = await reader.ReadLineAsync()) is not null)
                    {
                        if (line.Contains("WindowsPlayer"))
                            lastDeploy = line;
                    }
                }
            }

            if (String.IsNullOrEmpty(lastDeploy))
                throw new Exception($"Could not get latest deploy for channel {channel}");

            // here's to hoping roblox doesn't change their deployment entry format
            // (last time they did so was may 2021 so we should be fine?)
            // example entry: 'New WindowsPlayer version-29fb7cdd06e84001 at 8/23/2022 2:07:27 PM, file version: 0, 542, 100, 5420251, git hash: b98d6b2bea36fa2161f48cca979fb620bb0c24fd ...'

            // there's a proper way, and then there's the lazy way
            // this here is the lazy way but it should just work™

            lastDeploy = lastDeploy[18..]; // 'version-29fb7cdd06e84001 at 8/23/2022 2:07:27 PM, file version: 0, 542, 100, 5420251, git hash: b98d6b2bea36fa2161f48cca979fb620bb0c24fd ...'
            string versionGuid = lastDeploy[..lastDeploy.IndexOf(" at")]; // 'version-29fb7cdd06e84001'
            
            lastDeploy = lastDeploy[(versionGuid.Length + 4)..]; // '8/23/2022 2:07:27 PM, file version: 0, 542, 100, 5420251, git hash: b98d6b2bea36fa2161f48cca979fb620bb0c24fd ...'
            string strTimestamp = lastDeploy[..lastDeploy.IndexOf(", file")]; // '8/23/2022 2:07:27 PM'
            
            lastDeploy = lastDeploy[(strTimestamp.Length + 16)..]; // '0, 542, 100, 5420251, git hash: b98d6b2bea36fa2161f48cca979fb620bb0c24fd ...'
            string fileVersion = "";

            if (lastDeploy.Contains("git hash"))
            {
                // ~may 2021 entry: ends like 'file version: 0, 542, 100, 5420251, git hash: b98d6b2bea36fa2161f48cca979fb620bb0c24fd ...'
                fileVersion = lastDeploy[..lastDeploy.IndexOf(", git")]; // '0, 542, 100, 5420251'
            }
            else
            {
                // pre-may 2021 entry: ends like 'file version: 0, 448, 0, 411122...'
                fileVersion = lastDeploy[..lastDeploy.IndexOf("...")]; // '0, 448, 0, 411122'
            }

            // deployment timestamps are UTC-5
            strTimestamp += " -05";
            DateTime dtTimestamp = DateTime.ParseExact(strTimestamp, "M/d/yyyy h:mm:ss tt zz", Program.CultureFormat).ToLocalTime();

            // convert to traditional version format
            fileVersion = fileVersion.Replace(" ", "").Replace(',', '.');

            return new VersionDeploy 
            { 
                VersionGuid = versionGuid, 
                Timestamp = dtTimestamp, 
                FileVersion = fileVersion 
            };
        }
    }
}
