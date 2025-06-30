using Bloxstrap.Properties;
using System;
using System.Configuration;
using System.Windows.Automation;

namespace Bloxstrap.RobloxInterfaces
{
    public static class Deployment
    {
        public const string DefaultChannel = "production";
        
        private const string VersionStudioHash = "version-012732894899482c";


        public static string Channel = App.Settings.Prop.Channel;

        public static string BinaryType = "WindowsPlayer";

        public static bool IsDefaultChannel => Channel.Equals(DefaultChannel, StringComparison.OrdinalIgnoreCase) || Channel.Equals("live", StringComparison.OrdinalIgnoreCase);
        
        public static string BaseUrl { get; private set; } = null!;

        public static readonly List<HttpStatusCode?> BadChannelCodes = new()
        {
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound
        };

        private static readonly Dictionary<string, ClientVersion> ClientVersionCache = new();

        // a list of roblox deployment locations that we check for, in case one of them don't work
        // these are all weighted based on their priority, so that we pick the most optimal one that we can. 0 = highest
        private static readonly Dictionary<string, int> BaseUrls = new()
        {
            { "https://setup.rbxcdn.com", 0 },
            { "https://setup-aws.rbxcdn.com", 2 },
            { "https://setup-ak.rbxcdn.com", 2 },
            { "https://roblox-setup.cachefly.net", 2 },
            { "https://s3.amazonaws.com/setup.roblox.com", 4 }
        };

        private static async Task<string?> TestConnection(string url, int priority, CancellationToken token)
        {
            string LOG_IDENT = $"Deployment::TestConnection<{url}>";

            await Task.Delay(priority * 1000, token);

            App.Logger.WriteLine(LOG_IDENT, "Connecting...");

            try
            {
                var response = await App.HttpClient.GetAsync($"{url}/versionStudio", token);

                response.EnsureSuccessStatusCode();

                // versionStudio is the version hash for the last MFC studio to be deployed.
                // the response body should always be "version-012732894899482c".
                string content = await response.Content.ReadAsStringAsync(token);

                if (content != VersionStudioHash)
                    throw new InvalidHTTPResponseException($"versionStudio response does not match (expected \"{VersionStudioHash}\", got \"{content}\")");
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

        /// <summary>
        /// This function serves double duty as the setup mirror enumerator, and as our connectivity check.
        /// Returns null for success.
        /// </summary>
        /// <returns></returns>
        public static async Task<Exception?> InitializeConnectivity()
        {
            const string LOG_IDENT = "Deployment::InitializeConnectivity";

            var tokenSource = new CancellationTokenSource();

            var exceptions = new List<Exception>();
            var tasks = (from entry in BaseUrls select TestConnection(entry.Key, entry.Value, tokenSource.Token)).ToList();

            App.Logger.WriteLine(LOG_IDENT, "Testing connectivity...");

            while (tasks.Any() && String.IsNullOrEmpty(BaseUrl))
            {
                var finishedTask = await Task.WhenAny(tasks);

                tasks.Remove(finishedTask);

                if (finishedTask.IsFaulted)
                    exceptions.Add(finishedTask.Exception!.InnerException!);
                else if (!finishedTask.IsCanceled)
                    BaseUrl = finishedTask.Result;
            }

            // stop other running connectivity tests
            tokenSource.Cancel();

            if (string.IsNullOrEmpty(BaseUrl))
            {
                if (exceptions.Any())
                    return exceptions[0];

                // task cancellation exceptions don't get added to the list
                return new TaskCanceledException("All connection attempts timed out.");
            }

            App.Logger.WriteLine(LOG_IDENT, $"Got {BaseUrl} as the optimal base URL");

            return null;
        }

        public static string GetLocation(string resource)
        {
            string location = BaseUrl;

            if (!IsDefaultChannel)
            {
                string channelName;

                if (ApplicationSettings.GetSettings(nameof(ApplicationSettings.PCClientBootstrapper), Channel).Get<bool>("FFlagReplaceChannelNameForDownload"))
                    channelName = "common";
                else
                    channelName = Channel.ToLowerInvariant();

                location += $"/channel/{channelName}";
            }

            location += resource;

            return location;
        }

        public static async Task<ClientVersion> GetInfo(string? channel = null)
        {
            const string LOG_IDENT = "Deployment::GetInfo";

            if (String.IsNullOrEmpty(channel))
                channel = Channel;

            bool isDefaultChannel = String.Compare(channel, DefaultChannel, StringComparison.OrdinalIgnoreCase) == 0;

            App.Logger.WriteLine(LOG_IDENT, $"Getting deploy info for channel {channel}");

            string cacheKey = $"{channel}-{BinaryType}";

            ClientVersion clientVersion;

            if (ClientVersionCache.ContainsKey(cacheKey))
            {
                App.Logger.WriteLine(LOG_IDENT, "Deploy information is cached");
                clientVersion = ClientVersionCache[cacheKey];
            }
            else
            {
                string path = $"/v2/client-version/{BinaryType}";

                if (!isDefaultChannel)
                    path = $"/v2/client-version/{BinaryType}/channel/{channel}";

                try
                {
                    clientVersion = await Http.GetJson<ClientVersion>("https://clientsettingscdn.roblox.com" + path);
                }
                catch (HttpRequestException httpEx) 
                when (!isDefaultChannel && BadChannelCodes.Contains(httpEx.StatusCode))
                {
                    throw new InvalidChannelException(httpEx.StatusCode);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to contact clientsettingscdn! Falling back to clientsettings...");
                    App.Logger.WriteException(LOG_IDENT, ex);

                    try
                    {
                        clientVersion = await Http.GetJson<ClientVersion>("https://clientsettings.roblox.com" + path);
                    }
                    catch (HttpRequestException httpEx)
                    when (!isDefaultChannel && BadChannelCodes.Contains(httpEx.StatusCode))
                    {
                        throw new InvalidChannelException(httpEx.StatusCode);
                    }
                }

                // check if channel is behind LIVE


                if (!isDefaultChannel)
                {
                    var defaultClientVersion = await GetInfo(DefaultChannel);

                    if (Utilities.CompareVersions(clientVersion.Version, defaultClientVersion.Version) == VersionComparison.LessThan)
                        clientVersion.IsBehindDefaultChannel = true;
                }

                ClientVersionCache[cacheKey] = clientVersion;
            }

            return clientVersion;
        }
    }
}
