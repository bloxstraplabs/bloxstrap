using System.ComponentModel;

namespace Bloxstrap
{
    public class RobloxFastFlags
    {
        private string _applicationName;
        private string _channelName;

        private bool _initialised = false;
        private Dictionary<string, string>? _flags;

        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private RobloxFastFlags(string applicationName, string channelName)
        {
            _applicationName = applicationName;
            _channelName = channelName;
        }

        private async Task Fetch()
        {
            if (_initialised)
                return;

            await semaphoreSlim.WaitAsync();
            try
            {
                if (_initialised)
                    return;

                string logIndent = $"RobloxFastFlags::Fetch.{_applicationName}.{_channelName}";
                App.Logger.WriteLine(logIndent, "Fetching fast flags");

                string path = $"/v2/settings/application/{_applicationName}";
                if (_channelName != RobloxDeployment.DefaultChannel.ToLowerInvariant())
                    path += $"/bucket/{_channelName}";

                HttpResponseMessage response;

                try
                {
                    response = await App.HttpClient.GetAsync("https://clientsettingscdn.roblox.com" + path);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(logIndent, "Failed to contact clientsettingscdn! Falling back to clientsettings...");
                    App.Logger.WriteException(logIndent, ex);

                    response = await App.HttpClient.GetAsync("https://clientsettings.roblox.com" + path);
                }

                string rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    App.Logger.WriteLine(logIndent,
                        "Failed to fetch client settings!\r\n" +
                        $"\tStatus code: {response.StatusCode}\r\n" +
                        $"\tResponse: {rawResponse}"
                    );

                    throw new HttpResponseException(response);
                }

                var clientSettings = JsonSerializer.Deserialize<ClientFlagSettings>(rawResponse);

                if (clientSettings == null)
                    throw new Exception("Deserialised client settings is null!");

                if (clientSettings.ApplicationSettings == null)
                    throw new Exception("Deserialised application settings is null!");

                _flags = clientSettings.ApplicationSettings;
                _initialised = true;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<T?> GetAsync<T>(string name)
        {
            await Fetch();

            if (!_flags!.ContainsKey(name))
                return default;

            string value = _flags[name];

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                    return default;

                return (T?)converter.ConvertFromString(value);
            }
            catch (NotSupportedException) // boohoo
            {
                return default;
            }
        }

        public T? Get<T>(string name)
        {
            return GetAsync<T>(name).Result;
        }

        // _cache[applicationName][channelName]
        private static Dictionary<string, Dictionary<string, RobloxFastFlags>> _cache = new();

        public static RobloxFastFlags PCDesktopClient { get; } = GetSettings("PCDesktopClient");
        public static RobloxFastFlags PCClientBootstrapper { get; } = GetSettings("PCClientBootstrapper");

        public static RobloxFastFlags GetSettings(string applicationName, string? channelName = null, bool shouldCache = true)
        {
            string channelNameLower;
            if (!string.IsNullOrEmpty(channelName))
                channelNameLower = channelName.ToLowerInvariant();
            else
                channelNameLower = App.Settings.Prop.Channel.ToLowerInvariant();

            lock (_cache)
            {
                if (_cache.ContainsKey(applicationName) && _cache[applicationName].ContainsKey(channelNameLower))
                    return _cache[applicationName][channelNameLower];

                var flags = new RobloxFastFlags(applicationName, channelNameLower);

                if (shouldCache)
                {
                    if (!_cache.ContainsKey(applicationName))
                        _cache[applicationName] = new();

                    _cache[applicationName][channelNameLower] = flags;
                }

                return flags;
            }
        }
    }
}
