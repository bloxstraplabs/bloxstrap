namespace Bloxstrap.Models.APIs.Config
{
    public class SupporterGroup
    {
        [JsonPropertyName("columns")]
        public int Columns { get; set; } = 0;

        [JsonPropertyName("supporters")]
        public List<Supporter> Supporters { get; set; } = Enumerable.Empty<Supporter>().ToList();
    }
}
