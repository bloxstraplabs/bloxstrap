namespace Bloxstrap.Utility
{
    public static class AsyncHelpers
    {
        public static void ExceptionHandler(Task task, object? state)
        {
            const string LOG_IDENT = "AsyncHelpers::ExceptionHandler";

            if (task.Exception is null)
                return;

            if (state is null)
                App.Logger.WriteLine(LOG_IDENT, "An exception occurred while running the task");
            else
                App.Logger.WriteLine(LOG_IDENT, $"An exception occurred while running the task '{state}'");
            
            App.FinalizeExceptionHandling(task.Exception);
        }
    }
}
