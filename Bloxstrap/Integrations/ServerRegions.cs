namespace Bloxstrap.Integrations
{
    public class ServerRegion
    {
        public string Country { get; init; } = "";

        public string CountryName { get; init; } = "";

        public string City { get; init; } = "";

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        public string Key => $"{Country}|{City}";

        public string DisplayName => String.IsNullOrEmpty(CountryName) ? City : $"{City}, {CountryName}";

        public override string ToString() => DisplayName;
    }

    public class RegionServer
    {
        public string ServerId { get; init; } = "";

        public string City { get; init; } = "";

        public string Country { get; init; } = "";

        public DateTime? FirstSeen { get; init; }
    }

    public static class ServerRegions
    {
        private class DatacenterEntry
        {
            [JsonPropertyName("location")]
            public DatacenterLocation? Location { get; set; }

            [JsonPropertyName("inactive")]
            public bool Inactive { get; set; }
        }

        private class DatacenterLocation
        {
            [JsonPropertyName("city")]
            public string City { get; set; } = "";

            [JsonPropertyName("country")]
            public string Country { get; set; } = "";

            [JsonPropertyName("country_name")]
            public string CountryName { get; set; } = "";

            [JsonPropertyName("latLong")]
            public List<string>? LatLong { get; set; }
        }

        private class ServersResponse
        {
            [JsonPropertyName("next_cursor")]
            public int? NextCursor { get; set; }

            [JsonPropertyName("servers")]
            public List<ServerEntry>? Servers { get; set; }
        }

        private class ServerEntry
        {
            [JsonPropertyName("server_id")]
            public string? ServerId { get; set; }

            [JsonPropertyName("city")]
            public string City { get; set; } = "";

            [JsonPropertyName("country")]
            public string Country { get; set; } = "";

            [JsonPropertyName("first_seen")]
            public DateTime? FirstSeen { get; set; }
        }

        private class IpInfoResponse
        {
            [JsonPropertyName("loc")]
            public string? Loc { get; set; }
        }

        private const string DatacentersUrl = "https://apis.rovalra.com/v1/datacenters/list";

        private const string ServersUrl = "https://apis.rovalra.com/v1/servers/region";

        private static List<ServerRegion>? _regions;

        public static async Task<List<ServerRegion>> GetRegions()
        {
            if (_regions is not null)
                return _regions;

            var datacenters = await Http.GetJson<List<DatacenterEntry>>(DatacentersUrl) ?? new();

            _regions = datacenters
                .Where(x => !x.Inactive && x.Location is not null && !String.IsNullOrEmpty(x.Location.City))
                .Select(x => new ServerRegion
                {
                    Country = x.Location!.Country,
                    CountryName = x.Location.CountryName,
                    City = x.Location.City,
                    Latitude = ParseCoordinate(x.Location.LatLong, 0),
                    Longitude = ParseCoordinate(x.Location.LatLong, 1)
                })
                .GroupBy(x => x.Key)
                .Select(x => x.First())
                .OrderBy(x => x.CountryName)
                .ThenBy(x => x.City)
                .ToList();

            return _regions;
        }

        private static double ParseCoordinate(List<string>? values, int index) =>
            values is not null && values.Count > index && double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : 0;

        public static async Task<(List<RegionServer> Servers, int? NextCursor)> GetServers(long placeId, ServerRegion region, int? cursor = null)
        {
            string url = $"{ServersUrl}?place_id={placeId}&region={region.Country}&city={Uri.EscapeDataString(region.City)}";

            if (cursor is not null)
                url += $"&cursor={cursor}";

            var response = await Http.GetJson<ServersResponse>(url);

            var servers = response?.Servers?
                .Where(x => !String.IsNullOrEmpty(x.ServerId))
                .Select(x => new RegionServer { ServerId = x.ServerId!, City = x.City, Country = x.Country, FirstSeen = x.FirstSeen })
                .ToList() ?? new();

            return (servers, response?.NextCursor);
        }

        public static async Task<List<ServerRegion>> GetNearestRegions()
        {
            var regions = await GetRegions();

            try
            {
                var ipinfo = await Http.GetJson<IpInfoResponse>("https://ipinfo.io/json");
                string[] location = ipinfo?.Loc?.Split(',') ?? Array.Empty<string>();

                if (location.Length == 2
                    && double.TryParse(location[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat)
                    && double.TryParse(location[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                    return regions.OrderBy(x => Distance(lat, lon, x.Latitude, x.Longitude)).ToList();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerRegions::GetNearestRegions", ex);
            }

            return regions;
        }

        private static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            static double Radians(double degrees) => degrees * Math.PI / 180;

            double dLat = Radians(lat2 - lat1);
            double dLon = Radians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public static async Task<string?> FindServer(long placeId)
        {
            const string LOG_IDENT = "ServerRegions::FindServer";

            var candidates = new List<ServerRegion>();
            var regions = await GetRegions();

            var preferred = regions.FirstOrDefault(x => x.Key == App.Settings.Prop.PreferredRegion);

            if (preferred is not null)
                candidates.Add(preferred);

            candidates.AddRange((await GetNearestRegions()).Where(x => x != preferred).Take(4));

            foreach (var region in candidates)
            {
                var (servers, _) = await GetServers(placeId, region);

                App.Logger.WriteLine(LOG_IDENT, $"{servers.Count} servers in {region.DisplayName}");

                if (!servers.Any())
                    continue;

                var server = App.Settings.Prop.RandomizeMatchmaking ? servers[Random.Shared.Next(servers.Count)] : servers[0];

                App.Logger.WriteLine(LOG_IDENT, $"Picked {server.ServerId} in {region.DisplayName}");

                return server.ServerId;
            }

            return null;
        }
    }
}
