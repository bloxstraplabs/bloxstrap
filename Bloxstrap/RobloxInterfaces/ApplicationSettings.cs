using System.ComponentModel;

namespace Bloxstrap.RobloxInterfaces
{
    // i am 100% sure there is a much, MUCH better way to handle this
    // matt wrote this so this is effectively a black box to me right now
    // i'll likely refactor this at some point
    public class ApplicationSettings
    {
        private string _applicationName;
        private string _channelName;

        private bool _initialised = false;
        private Dictionary<string, string>? _flags;

        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private ApplicationSettings(string applicationName, string channelName)
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

                string logIndent = $"ApplicationSettings::Fetch.{_applicationName}.{_channelName}";
                App.Logger.WriteLine(logIndent, "Fetching fast flags");

                string path = $"/v2/settings/application/{_applicationName}";
                if (_channelName != Deployment.DefaultChannel.ToLowerInvariant())
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

                response.EnsureSuccessStatusCode();

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
        private static Dictionary<string, Dictionary<string, ApplicationSettings>> _cache = new();

        public static ApplicationSettings PCDesktopClient => GetSettings("PCDesktopClient");

        public static ApplicationSettings PCClientBootstrapper => GetSettings("PCClientBootstrapper");

        public static ApplicationSettings GetSettings(string applicationName, string channelName = Deployment.DefaultChannel, bool shouldCache = true)
        {
            channelName = channelName.ToLowerInvariant();

            lock (_cache)
            {
                if (_cache.ContainsKey(applicationName) && _cache[applicationName].ContainsKey(channelName))
                    return _cache[applicationName][channelName];

                var flags = new ApplicationSettings(applicationName, channelName);

                if (shouldCache)
                {
                    if (!_cache.ContainsKey(applicationName))
                        _cache[applicationName] = new();

                    _cache[applicationName][channelName] = flags;
                }

                return flags;
            }
        }
    }
}
