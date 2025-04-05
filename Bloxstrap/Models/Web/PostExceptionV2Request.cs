namespace Bloxstrap.Models.Web
{
    internal class PostExceptionV2Request
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("log")]
        public string Log { get; set; } = "";
    }
}
