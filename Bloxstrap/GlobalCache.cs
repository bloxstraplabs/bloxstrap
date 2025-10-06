namespace Bloxstrap
{
    public static class GlobalCache
    {
        public static readonly Dictionary<string, string?> ServerLocation = new();

        public static readonly Dictionary<string, DateTime?> ServerTime = new();
    }
}
