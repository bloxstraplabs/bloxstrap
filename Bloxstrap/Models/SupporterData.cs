namespace Bloxstrap.Models
{
    public class SupporterData
    {
        [JsonPropertyName("columns")]
        public int Columns { get; set; }

        [JsonPropertyName("supporters")]
        public List<Supporter> Supporters { get; set; } = null!;
    }
}
