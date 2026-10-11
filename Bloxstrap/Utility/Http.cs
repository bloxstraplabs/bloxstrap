namespace Bloxstrap.Utility
{
    internal static class Http
    {
        public static async Task<T> GetJson<T>(string url)
        {
            var request = await App.HttpClient.GetAsync(url);

            request.EnsureSuccessStatusCode();

            string json = await request.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}
