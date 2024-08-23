namespace Bloxstrap
{
    public static class RobloxDeployment
    {
        public const string DefaultChannel = "production";

        public static string BaseUrl { get; private set; } = null!;

        private static readonly Dictionary<string, ClientVersion> ClientVersionCache = new();

        // a list of roblox deployment locations that we check for, in case one of them don't work
        // these are all weighted based on their priority, so that we pick the most optimal one that we can. 0 = highest
        private static readonly Dictionary<string, int> BaseUrls = new()
        {
            { "https://setup.rbxcdn.com", 0 },
            { "https://setup-ak.rbxcdn.com", 2 },
            { "https://roblox-setup.cachefly.net", 2 },
            { "https://s3.amazonaws.com/setup.roblox.com", 4 }
        };

        private static async Task<string?> TestConnection(string url, int priority, CancellationToken token)
        {
            string LOG_IDENT = $"RobloxDeployment::TestConnection.{url}";

            await Task.Delay(priority * 1000, token);

            App.Logger.WriteLine(LOG_IDENT, "Connecting...");

            try
            {
                var response = await App.HttpClient.GetAsync($"{url}/version", token);
                
                if (!response.IsSuccessStatusCode)
                    throw new HttpResponseException(response);
            }
            catch (TaskCanceledException)
            {
                App.Logger.WriteLine(LOG_IDENT, "Connectivity test cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                throw;
            }

            return url;
        }

        public static async Task<Exception?> InitializeConnectivity()
        {
            const string LOG_IDENT = "RobloxDeployment::InitializeConnectivity";

            // this function serves double duty as the setup mirror enumerator, and as our connectivity check
            // since we're basically asking four different urls for the exact same thing, if all four fail, then it has to be a user-side problem

            // this should be checked for in the installer and in the bootstrapper

            // returns null for success

            if (!String.IsNullOrEmpty(BaseUrl))
                return null;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            var exceptions = new List<Exception>();
            var tasks = (from entry in BaseUrls select TestConnection(entry.Key, entry.Value, token)).ToList();

            App.Logger.WriteLine(LOG_IDENT, "Testing connectivity...");

            while (tasks.Any())
            {
                var finishedTask = await Task.WhenAny(tasks);

                if (finishedTask.IsFaulted)
                {
                    tasks.Remove(finishedTask);
                    exceptions.Add(finishedTask.Exception!.InnerException!);
                    continue;
                }

                BaseUrl = await finishedTask;
                break;
            }

            // stop other running connectivity tests
            tokenSource.Cancel();

            if (String.IsNullOrEmpty(BaseUrl))
                return exceptions[0];

            App.Logger.WriteLine(LOG_IDENT, $"Got {BaseUrl} as the optimal base URL");

            return null;
        }

        public static string GetLocation(string resource, string channel = DefaultChannel)
        {
            string location = BaseUrl;

            if (String.Compare(channel, DefaultChannel, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                string channelName;

                if (RobloxFastFlags.GetSettings(nameof(RobloxFastFlags.PCClientBootstrapper), channel).Get<bool>("FFlagReplaceChannelNameForDownload"))
                    channelName = "common";
                else
                    channelName = channel.ToLowerInvariant();

                location += $"/channel/{channelName}";
            }

            location += resource;

            return location;
        }

        public static async Task<ClientVersion> GetInfo(string channel, string binaryType = "WindowsPlayer")
        {
            const string LOG_IDENT = "RobloxDeployment::GetInfo";

            App.Logger.WriteLine(LOG_IDENT, $"Getting deploy info for channel {channel}");

            string cacheKey = $"{channel}-{binaryType}";
            ClientVersion clientVersion;

            if (ClientVersionCache.ContainsKey(cacheKey))
            {
                App.Logger.WriteLine(LOG_IDENT, "Deploy information is cached");
                clientVersion = ClientVersionCache[cacheKey];
            }
            else
            {
                string path = $"/v2/client-version/{binaryType}";

                if (String.Compare(channel, DefaultChannel, StringComparison.InvariantCultureIgnoreCase) != 0)
                    path = $"/v2/client-version/{binaryType}/channel/{channel}";

                HttpResponseMessage deployInfoResponse;

                try
                {
                    deployInfoResponse = await App.HttpClient.GetAsync("https://clientsettingscdn.roblox.com" + path);
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

                if (Utilities.CompareVersions(clientVersion.Version, defaultClientVersion.Version) == VersionComparison.LessThan)
                    clientVersion.IsBehindDefaultChannel = true;
            }

            ClientVersionCache[cacheKey] = clientVersion;

            return clientVersion;
        }
    }
}
