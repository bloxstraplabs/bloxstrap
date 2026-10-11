namespace Bloxstrap
{
    public static class RobloxSingleton
    {
        private static readonly string[] _names = { "ROBLOX_singletonEvent", "ROBLOX_singletonMutex" };

        private static readonly List<Mutex> _handles = new();

        public static void Hold()
        {
            const string LOG_IDENT = "RobloxSingleton::Hold";

            if (_handles.Any())
                return;

            foreach (string name in _names)
            {
                try
                {
                    _handles.Add(new Mutex(false, name));
                    App.Logger.WriteLine(LOG_IDENT, $"Holding {name}");
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{name} is already owned by Roblox, multi-instance will not work until all clients are closed");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to hold {name}");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }

        public static void Release()
        {
            foreach (var handle in _handles)
                handle.Dispose();

            _handles.Clear();
        }
    }
}
