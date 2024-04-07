﻿namespace Bloxstrap
{
    public static class RobloxDeployment
    {
        #region Properties
        public const string DefaultChannel = "LIVE";

        private static Dictionary<string, ClientVersion> ClientVersionCache = new();

        // a list of roblox deployment locations that we check for, in case one of them don't work
        private static List<string> BaseUrls = new()
        {
            "https://setup.limbrv.xyz",
            "https://setup.limbrv.xyz",
            "https://setup.limbrv.xyz",
            "https://setup.limbrv.xyz"
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
                            var response = App.HttpClient.GetAsync($"{attemptedUrl}/version").Result;

                            if (!response.IsSuccessStatusCode)
                                throw new HttpResponseException(response);

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
                        throw new Exception("Could not find an accessible Roblox deployment mirror, likely due to a bad internet connection. Please launch again.");
                }

                return _baseUrl;
            }
        }
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

        public static async Task<ClientVersion> GetInfo(string channel, bool extraInformation = false, string binaryType = "WindowsPlayer")
        {
            const string LOG_IDENT = "RobloxDeployment::GetInfo";

            App.Logger.WriteLine(LOG_IDENT, $"Getting deploy info for channel {channel} (extraInformation={extraInformation})");

            string cacheKey = $"{channel}-{binaryType}";
            ClientVersion clientVersion;

            if (ClientVersionCache.ContainsKey(cacheKey))
            {
                App.Logger.WriteLine(LOG_IDENT, "Deploy information is cached");
                clientVersion = ClientVersionCache[cacheKey];
            }
            else
            {
                string path = $"/v1/client-version/{binaryType}";
                HttpResponseMessage deployInfoResponse;

                try
                {
                    deployInfoResponse = await App.HttpClient.GetAsync("https://clientsettings.limbrv.xyz" + path);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to contact clientsettingscdn! Falling back to clientsettings...");
                    App.Logger.WriteException(LOG_IDENT, ex);

                    deployInfoResponse = await App.HttpClient.GetAsync("https://clientsettings.roblox.com" + path);
                }

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

                    throw new HttpResponseException(deployInfoResponse);
                }

                clientVersion = JsonSerializer.Deserialize<ClientVersion>(rawResponse)!;
            }

            // check if channel is behind LIVE
            if (channel != DefaultChannel)
            {
                var defaultClientVersion = await GetInfo(DefaultChannel);

                if (Utilities.CompareVersions(clientVersion.Version, defaultClientVersion.Version) == -1)
                    clientVersion.IsBehindDefaultChannel = true;
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
            }

            ClientVersionCache[cacheKey] = clientVersion;

            return clientVersion;
        }
    }
}
