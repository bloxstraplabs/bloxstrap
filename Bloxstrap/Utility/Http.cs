namespace Bloxstrap.Utility
{
    internal static class Http
    {
        public static async Task<T?> GetJson<T>(string url)
        {
            string json = await App.HttpClient.GetStringAsync(url);

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"[Http::GetJson<{typeof(T).Name}>] Failed to deserialize JSON for {url}!");
                App.Logger.WriteLine($"[Http::GetJson<{typeof(T).Name}>] {ex}");
                return default;
            }
        }
    }
}
