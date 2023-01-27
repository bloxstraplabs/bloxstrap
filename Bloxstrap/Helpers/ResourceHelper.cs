using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bloxstrap.Helpers
{
    internal class ResourceHelper
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
