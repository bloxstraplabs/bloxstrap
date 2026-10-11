using System.Resources;

namespace Bloxstrap.Extensions
{
    static class ResourceManagerEx
    {
        public static string GetStringSafe(this ResourceManager manager, string name) => manager.GetStringSafe(name, null);

        public static string GetStringSafe(this ResourceManager manager, string name, CultureInfo? culture)
        {
            string? resourceValue = manager.GetString(name, culture);

            return resourceValue ?? name;
        }
    }
}
