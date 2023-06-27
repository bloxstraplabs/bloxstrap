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
        private readonly List<string> _backlog = new();
        private FileStream? _filestream;

        public void Initialize(string filename)
        {
            if (_filestream is not null)
                throw new Exception("Logger is already initialized");

            string? directory = Path.GetDirectoryName(filename);

            if (directory is not null)
                Directory.CreateDirectory(directory);

            _filestream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);

            if (_backlog.Count > 0)
                WriteToLog(string.Join("\r\n", _backlog));

            WriteLine($"[Logger::Logger] Initialized at {filename}");
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
            if (_filestream is null)
            {
                _backlog.Add(message);
                return;
            }

            try
            {
                await _semaphore.WaitAsync();
                await _filestream.WriteAsync(Encoding.Unicode.GetBytes($"{message}\r\n"));
                await _filestream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
