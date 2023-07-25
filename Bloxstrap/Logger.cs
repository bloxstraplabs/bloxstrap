namespace Bloxstrap
{
    // https://stackoverflow.com/a/53873141/11852173
    // TODO - this kind of sucks
    // the main problem is just that this doesn't finish writing log entries before exiting the program
    // this can be solved by making writetolog completely synchronous, but while it doesn't affect performance, its's not ideal
    // also, writing and flushing for every single line that's written may not be great

    public class Logger
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private FileStream? _filestream;

        public readonly List<string> Backlog = new();
        public bool Initialized = false;
        public string? FileLocation;

        public void Initialize(bool useTempDir = false)
        {
            const string LOG_IDENT = "Logger::Initialize";

            string directory = useTempDir ? Path.Combine(Paths.LocalAppData, "Temp") : Path.Combine(Paths.Base, "Logs");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            string filename = $"{App.ProjectName}_{timestamp}.log";
            string location = Path.Combine(directory, filename);

            WriteLine(LOG_IDENT, $"Initializing at {location}");

            if (Initialized)
            {
                WriteLine(LOG_IDENT, "Failed to initialize because logger is already initialized");
                return;
            }

            Directory.CreateDirectory(directory);

            if (File.Exists(location))
            {
                WriteLine(LOG_IDENT, "Failed to initialize because log file already exists");
                return;
            }

            _filestream = File.Open(location, FileMode.Create, FileAccess.Write, FileShare.Read);

            Initialized = true;

            if (Backlog.Count > 0)
                WriteToLog(string.Join("\r\n", Backlog));

            WriteLine(LOG_IDENT, "Finished initializing!");

            FileLocation = location;

            // clean up any logs older than a week
            if (Paths.Initialized && Directory.Exists(Paths.Logs))
            {
                foreach (FileInfo log in new DirectoryInfo(Paths.Logs).GetFiles())
                {
                    if (log.LastWriteTimeUtc.AddDays(7) > DateTime.UtcNow)
                        continue;

                    WriteLine(LOG_IDENT, $"Cleaning up old log file '{log.Name}'");
                    log.Delete();
                }
            }
        }

        private void WriteLine(string message)
        {
            string timestamp = DateTime.UtcNow.ToString("s") + "Z";
            string outcon = $"{timestamp} {message}";
            string outlog = outcon.Replace(Paths.UserProfile, "%UserProfile%");

            Debug.WriteLine(outcon);
            WriteToLog(outlog);
        }

        public void WriteLine(string identifier, string message) => WriteLine($"[{identifier}] {message}");

        public void WriteException(string identifier, Exception ex)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            WriteLine($"[{identifier}] {ex}");
        }

        private async void WriteToLog(string message)
        {
            if (!Initialized)
            {
                Backlog.Add(message);
                return;
            }

            try
            {
                await _semaphore.WaitAsync();
                await _filestream!.WriteAsync(Encoding.UTF8.GetBytes($"{message}\r\n"));
                await _filestream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
