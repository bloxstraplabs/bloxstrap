using System.Text.Json.Serialization;

namespace Bloxstrap.Models
{
    public class GithubCommits
    {
        [JsonPropertyName("assets")]
        public List<GithubCommit>? Commits { get; set; }
    }

    public class GithubCommit
    {
        [JsonPropertyName("commit")]
        public GithubCommitData Commit { get; set; } = null!;
    }

    public class GithubCommitData
    {
        [JsonPropertyName("author")]
        public GithubCommitAuthor Author { get; set; } = null!;
    }

    public class GithubCommitAuthor
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = null!;
    }
}
