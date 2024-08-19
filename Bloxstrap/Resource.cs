using System.Reflection;

namespace Bloxstrap
{
    static class Resource
    {
        static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        static readonly string[] resourceNames = assembly.GetManifestResourceNames();

        public static Stream GetStream(string name)
        {
            string path = resourceNames.Single(str => str.EndsWith(name));
            return assembly.GetManifestResourceStream(path)!;
        }

        public static async Task<byte[]> Get(string name)
        {
            using var stream = GetStream(name);
            using var memoryStream = new MemoryStream();
            
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
