using System.ComponentModel;

namespace Bloxstrap.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WebEnvironment
    {
        [Description("prod")]
        Production,

        [Description("stage")]
        Staging,

        [Description("int")]
        Integration,

        [Description("matt")]
        Matt,

        [Description("local")]
        Local
    }
}
