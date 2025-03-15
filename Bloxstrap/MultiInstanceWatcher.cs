namespace Bloxstrap
{
    internal static class MultiInstanceWatcher
    {
        private static int GetOpenProcessesCount()
        {
            const string LOG_IDENT = "MultiInstanceWatcher::GetOpenProcessesCount";

            try
            {
                // prevent any possible race conditions by checking for bloxstrap processes too
                int count = Process.GetProcesses().Count(x => x.ProcessName is "RobloxPlayerBeta" or "Bloxstrap");
                count -= 1; // ignore the current process
                return count;
            }
            catch (Exception ex)
            {
                // everything process related can error at any time
                App.Logger.WriteException(LOG_IDENT, ex);
                return -1;
            }
        }

        private static void FireInitialisedEvent()
        {
            using EventWaitHandle initEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "Bloxstrap-MultiInstanceWatcherInitialisationFinished");
            initEventHandle.Set();
        }

        public static void Run()
        {
            const string LOG_IDENT = "MultiInstanceWatcher::Run";

            // try to get the mutex
            bool acquiredMutex;
            using Mutex mutex = new Mutex(false, "ROBLOX_singletonMutex");
            try
            {
                acquiredMutex = mutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                acquiredMutex = true;
            }

            if (!acquiredMutex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Client singleton mutex is already acquired");
                FireInitialisedEvent();
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Acquired mutex!");
            FireInitialisedEvent();

            // watch for alive processes
            int count;
            do
            {
                Thread.Sleep(5000);
                count = GetOpenProcessesCount();
            }
            while (count == -1 || count > 0); // redo if -1 (one of the Process apis failed)

            App.Logger.WriteLine(LOG_IDENT, "All Roblox related processes have closed, exiting!");
        }
    }
}
