using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

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
            string directory = useTempDir ? Path.Combine(Directories.LocalAppData, "Temp") : Path.Combine(Directories.Base, "Logs");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            string filename = $"{App.ProjectName}_{timestamp}.log";
            string location = Path.Combine(directory, filename);

            WriteLine($"[Logger::Initialize] Initializing at {location}");

            if (Initialized)
            {
                WriteLine("[Logger::Initialize] Failed to initialize because logger is already initialized");
                return;
            }

            Directory.CreateDirectory(directory);

            if (File.Exists(location))
            {
                WriteLine("[Logger::Initialize] Failed to initialize because log file already exists");
                return;
            }

            _filestream = File.Open(location, FileMode.Create, FileAccess.Write, FileShare.Read);

            if (Backlog.Count > 0)
                WriteToLog(string.Join("\r\n", Backlog));

            WriteLine($"[Logger::Initialize] Finished initializing!");

            Initialized = true;
            FileLocation = location;

            // clean up any logs older than a week
            if (Directories.Initialized && Directory.Exists(Directories.Logs))
            {
                foreach (FileInfo log in new DirectoryInfo(Directories.Logs).GetFiles())
                {
                    if (log.LastWriteTimeUtc.AddDays(7) > DateTime.UtcNow)
                        continue;

                    App.Logger.WriteLine($"[Logger::Initialize] Cleaning up old log file '{log.Name}'");
                    log.Delete();
                }
            }
        }

        public void WriteLine(string message)
        {
            string timestamp = DateTime.UtcNow.ToString("s") + "Z";
            string outcon = $"{timestamp} {message}";
            string outlog = outcon.Replace(Directories.UserProfile, "%UserProfile%");

            Debug.WriteLine(outcon);
            WriteToLog(outlog);
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
                await _filestream!.WriteAsync(Encoding.Unicode.GetBytes($"{message}\r\n"));
                await _filestream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
