using Bloxstrap.Exceptions;

namespace Bloxstrap
{
    public static class RobloxDeployment
    {
        #region Properties
        public const string DefaultChannel = "LIVE";

        private static Dictionary<string, ClientVersion> ClientVersionCache = new();

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
                const string LOG_IDENT = "DeployManager::DefaultBaseUrl.Set";

                if (string.IsNullOrEmpty(_baseUrl))
                {
                    // check for a working accessible deployment domain
                    foreach (string attemptedUrl in BaseUrls)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Testing connection to '{attemptedUrl}'...");

                        try
                        {
                            App.HttpClient.GetAsync($"{attemptedUrl}/version").Wait();
                            App.Logger.WriteLine(LOG_IDENT, "Connection successful!");
                            _baseUrl = attemptedUrl;
                            break;
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteLine(LOG_IDENT, "Connection failed!");
                            App.Logger.WriteException(LOG_IDENT, ex);
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

            if (channel.ToLowerInvariant() != DefaultChannel.ToLowerInvariant())
                location += $"/channel/{channel.ToLowerInvariant()}";

            location += resource;

            return location;
        }

        public static async Task<ClientVersion> GetInfo(string channel, bool extraInformation = false)
        {
            const string LOG_IDENT = "RobloxDeployment::GetInfo";

            App.Logger.WriteLine(LOG_IDENT, $"Getting deploy info for channel {channel} (extraInformation={extraInformation})");

            ClientVersion clientVersion;

            if (ClientVersionCache.ContainsKey(channel))
            {
                App.Logger.WriteLine(LOG_IDENT, "Deploy information is cached");
                clientVersion = ClientVersionCache[channel];
            }
            else
            {
                HttpResponseMessage deployInfoResponse = await App.HttpClient.GetAsync($"https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer/channel/{channel}");

                string rawResponse = await deployInfoResponse.Content.ReadAsStringAsync();

                if (!deployInfoResponse.IsSuccessStatusCode)
                {
                    // 400 = Invalid binaryType.
                    // 404 = Could not find version details for binaryType.
                    // 500 = Error while fetching version information.
                    // either way, we throw

                    App.Logger.WriteLine(LOG_IDENT,
                        "Failed to fetch deploy info!\r\n" +
                        $"\tStatus code: {deployInfoResponse.StatusCode}\r\n" +
                        $"\tResponse: {rawResponse}"
                    );

                    throw new HttpResponseUnsuccessfulException(deployInfoResponse);
                }

                clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawResponse)!;
            }
            

            // for preferences
            if (extraInformation && clientVersion.Timestamp is null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Getting extra information...");

                string manifestUrl = GetLocation($"/{clientVersion.VersionGuid}-rbxPkgManifest.txt", channel);

                // get an approximate deploy time from rbxpkgmanifest's last modified date
                HttpResponseMessage pkgResponse = await App.HttpClient.GetAsync(manifestUrl);

                if (pkgResponse.Content.Headers.TryGetValues("last-modified", out var values))
                {
                    string lastModified = values.First();
                    App.Logger.WriteLine(LOG_IDENT, $"{manifestUrl} - Last-Modified: {lastModified}");
                    clientVersion.Timestamp = DateTime.Parse(lastModified).ToLocalTime();
                }

                // check if channel is behind LIVE
                if (channel != DefaultChannel)
                {
                    var defaultClientVersion = await GetInfo(DefaultChannel);

                    if (Utilities.CompareVersions(clientVersion.Version, defaultClientVersion.Version) == -1)
                        clientVersion.IsBehindDefaultChannel = true;
                }
            }

            ClientVersionCache[channel] = clientVersion;

            return clientVersion;
        }
    }
}
