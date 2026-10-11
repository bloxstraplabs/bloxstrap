namespace Bloxstrap
{
    public enum RejoinCategory
    {
        Idle,
        ConnectionLost,
        ServerShutdown,
        TeleportFailure
    }

    public class RejoinHistory
    {
        public List<DateTime> Attempts { get; set; } = new();
    }

    public static class RejoinRules
    {
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(10);

        public static RejoinCategory? GetCategory(int reason) => reason switch
        {
            278 => RejoinCategory.Idle,
            1 or 266 or 277 or 279 or 290 or 291 or 292 or 295 => RejoinCategory.ConnectionLost,
            274 or 275 or 276 or 296 => RejoinCategory.ServerShutdown,
            >= 769 and <= 774 => RejoinCategory.TeleportFailure,
            _ => null
        };

        public static bool IsEnabled(RejoinCategory category) => category switch
        {
            RejoinCategory.Idle => App.Settings.Prop.RejoinOnIdle,
            RejoinCategory.ConnectionLost => App.Settings.Prop.RejoinOnConnectionLost,
            RejoinCategory.ServerShutdown => App.Settings.Prop.RejoinOnShutdown,
            RejoinCategory.TeleportFailure => App.Settings.Prop.RejoinOnTeleportFailure,
            _ => false
        };

        public static bool TryRecordAttempt()
        {
            var manager = new JsonManager<RejoinHistory>();
            manager.Load(false);

            var recent = manager.Prop.Attempts.Where(x => DateTime.Now - x < AttemptWindow).ToList();

            if (recent.Count >= Math.Max(App.Settings.Prop.RejoinMaxAttempts, 1))
                return false;

            recent.Add(DateTime.Now);
            manager.Prop.Attempts = recent;
            manager.Save();

            return true;
        }
    }
}
