using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bloxstrap.Helpers
{
    // https://stackoverflow.com/a/53873141/11852173
    public class Logger
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly FileStream _filestream;

        public Logger(string filename)
        {
            string? directory = Path.GetDirectoryName(filename);
            
            if (directory is not null)
                Directory.CreateDirectory(directory);

            _filestream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            WriteLine($"[Logger::Logger] {App.ProjectName} v{App.Version} - Initialized at {filename}");
        }

        public async void WriteLine(string message)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            string conout = $"{timestamp} {message}";
            byte[] fileout = Encoding.Unicode.GetBytes($"{conout.Replace(Directories.UserProfile, "<UserProfileFolder>")}\r\n");

            Debug.WriteLine(conout);

            try
            {
                await _semaphore.WaitAsync();
                await _filestream.WriteAsync(fileout);
                await _filestream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
