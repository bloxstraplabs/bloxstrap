using System.Reflection;

namespace Bloxstrap
{
    static class Resource
    {
        static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        static readonly string[] resourceNames = assembly.GetManifestResourceNames();

        public static async Task<byte[]> Get(string name)
        {
            string path = resourceNames.Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(path)!)
            {
                using (MemoryStream memoryStream = new())
                {
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
