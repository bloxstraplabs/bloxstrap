namespace Bloxstrap
{
    // https://stackoverflow.com/a/53873141/11852173

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

            try
            {
                _filestream = File.Open(location, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            catch (IOException)
            {
                WriteLine(LOG_IDENT, "Failed to initialize because log file already exists");
                return;
            }
            

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

                    try
                    {
                       log.Delete();
                    }
                    catch (Exception ex)
                    {
                        WriteLine(LOG_IDENT, "Failed to delete log!");
                        WriteException(LOG_IDENT, ex);
                    }
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
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            WriteLine($"[{identifier}] {ex}");

            Thread.CurrentThread.CurrentUICulture = Locale.CurrentCulture;
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

                _ = _filestream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
