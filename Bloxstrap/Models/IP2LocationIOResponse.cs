namespace Bloxstrap.Models
{
	public class IP2LocationIOResponse
	{
		[JsonPropertyName("city_name")]
		public string City { get; set; } = null!;

		[JsonPropertyName("country_code")]
		public string Country { get; set; } = null!;

		[JsonPropertyName("region_name")]
		public string Region { get; set; } = null!;
	}
}
