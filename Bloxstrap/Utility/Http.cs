namespace Bloxstrap.Utility
{
    internal static class Http
    {
        public static async Task<T?> GetJson<T>(string url)
        {
            string LOG_IDENT = $"Http::GetJson<{typeof(T).Name}>";

            string json = await App.HttpClient.GetStringAsync(url);

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to deserialize JSON for {url}!");
                App.Logger.WriteException(LOG_IDENT, ex);
                return default;
            }
        }
    }
}
