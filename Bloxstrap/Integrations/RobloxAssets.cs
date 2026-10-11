using System.Security.Cryptography;

namespace Bloxstrap.Integrations
{
    public static class RobloxAssets
    {
        private class AssetDeliveryResponse
        {
            [JsonPropertyName("locations")]
            public List<AssetLocation>? Locations { get; set; }

            [JsonPropertyName("assetTypeId")]
            public int AssetTypeId { get; set; }
        }

        private class AssetLocation
        {
            [JsonPropertyName("location")]
            public string? Location { get; set; }
        }

        private const string AssetDeliveryUrl = "https://assetdelivery.roblox.com/v2/assetId/";

        private static readonly HttpClient _client = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, UseCookies = false });

        private static string? _cookie;

        public static string CacheFolder => Path.Combine(Paths.Temp, "GameImages");

        private static string? ReadCookie()
        {
            const string LOG_IDENT = "RobloxAssets::ReadCookie";

            if (_cookie is not null)
                return _cookie;

            try
            {
                string path = Path.Combine(Paths.LocalAppData, "Roblox", "LocalStorage", "RobloxCookies.dat");
                var file = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));

                if (file is null || !file.TryGetValue("CookiesData", out string? encoded))
                    return null;

                byte[] decrypted = ProtectedData.Unprotect(Convert.FromBase64String(encoded), null, DataProtectionScope.CurrentUser);
                var match = Regex.Match(Encoding.UTF8.GetString(decrypted), @"\.ROBLOSECURITY\t([^;]+)");

                if (match.Success)
                    _cookie = match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not read the Roblox login");
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            return _cookie;
        }

        public static async Task<string> GetImage(ulong assetId)
        {
            Directory.CreateDirectory(CacheFolder);

            string path = Path.Combine(CacheFolder, $"{assetId}.png");

            if (File.Exists(path))
                return path;

            string cookie = ReadCookie() ?? throw new InvalidOperationException("Not signed in to Roblox");

            var request = new HttpRequestMessage(HttpMethod.Get, AssetDeliveryUrl + assetId);
            request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var asset = JsonSerializer.Deserialize<AssetDeliveryResponse>(await response.Content.ReadAsStringAsync());

            if (asset?.AssetTypeId != 1)
                throw new InvalidOperationException($"Asset {assetId} is not an image");

            string location = asset.Locations?.FirstOrDefault()?.Location ?? throw new InvalidOperationException($"Asset {assetId} has no download location");

            await File.WriteAllBytesAsync(path, await _client.GetByteArrayAsync(location));

            return path;
        }

        public static void ClearCache()
        {
            try
            {
                if (Directory.Exists(CacheFolder))
                    Directory.Delete(CacheFolder, true);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("RobloxAssets::ClearCache", ex);
            }
        }
    }
}
