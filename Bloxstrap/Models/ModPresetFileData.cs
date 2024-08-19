using System.Security.Cryptography;
using System.Windows.Markup;

namespace Bloxstrap.Models
{
    public class ModPresetFileData
    {
        public string FilePath { get; private set; }

        public string FullFilePath => Path.Combine(Paths.Modifications, FilePath);
        
        public FileStream FileStream => File.OpenRead(FullFilePath);

        public string ResourceIdentifier { get; private set; }
        
        public Stream ResourceStream => Resource.GetStream(ResourceIdentifier);

        public byte[] ResourceHash { get; private set; }

        public ModPresetFileData(string contentPath, string resource) 
        {
            FilePath = contentPath;
            ResourceIdentifier = resource;

            using var stream = ResourceStream;
            ResourceHash = App.MD5Provider.ComputeHash(stream);
        }

        public bool HashMatches()
        {
            if (!File.Exists(FullFilePath))
                return false;

            using var fileStream = FileStream;
            var fileHash = App.MD5Provider.ComputeHash(fileStream);

            return fileHash.SequenceEqual(ResourceHash);
        }
    }
}
