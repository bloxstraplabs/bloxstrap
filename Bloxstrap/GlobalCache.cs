namespace Bloxstrap
{
    public static class GlobalCache
    {
        public static readonly Dictionary<string, Task> PendingTasks = new();
        
        public static readonly Dictionary<string, string> ServerLocation = new();
    }
}
